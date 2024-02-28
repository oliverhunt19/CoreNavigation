using FluentAssertions;
using RoutePlanning;

namespace TestRoutePlanning
{
    [TestClass]
    public class TestDMS
    {
        [TestMethod]
        public void TestDDtoDMS()
        {
            DD dD = new DD(53.32055555555556);
            DMS dMS = dD.ToDMS();
            double tol = 1e-9;

            dMS.Degrees.Should().Be(53);
            dMS.Minutes.Should().Be(19);
            dMS.Seconds.Should().BeApproximately(14, tol);
        }

        [TestMethod]
        public void TestDMStoDD()
        {
            DMS dMS = new DMS(53, 19, 14);
            DD dD = dMS.ToDD();
            dD.DecimalDegrees.Should().BeApproximately(53.32055555555556, 1e-9);
        }

        [TestMethod]
        public void TestNegDMStoDD()
        {
            DMS dMS = new DMS(-53, 19, 14);
            DD dD = dMS.ToDD();
            dD.DecimalDegrees.Should().BeApproximately(-53.32055555555556, 1e-9);
        }

        [TestMethod]
        public void Constructor_ValidInput_InitializesCorrectly()
        {
            // Arrange
            int degrees = 45;
            int minutes = 30;
            double seconds = 15.5;

            // Act
            DMS dms = new DMS(degrees, minutes, seconds);

            // Assert
            Assert.AreEqual(degrees, dms.Degrees);
            Assert.AreEqual(minutes, dms.Minutes);
            Assert.AreEqual(seconds, dms.Seconds);
        }

        [TestMethod]
        public void Constructor_InvalidMinutes_ThrowsArgumentException()
        {
            // Arrange
            int degrees = 45;
            int invalidMinutes = 61; // Invalid minutes value

            // Act & Assert

            
            Assert.ThrowsException<ArgumentException>(() => new DMS(degrees, invalidMinutes, 0));
        }

        [TestMethod]
        public void Constructor_InvalidSeconds_ThrowsArgumentException()
        {
            // Arrange
            int degrees = 45;
            double invalidSeconds = 61.5; // Invalid seconds value

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => new DMS(degrees, 30, invalidSeconds));
        }

        [TestMethod]
        public void TestExceptionThrowWhenDMSConstruction1()
        {

            Action action = () => {new DMS(12,64,54); };
            action.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void TestExceptionThrowWhenDMSConstruction2()
        {

            Action action = () => {new DMS(12,54,64); };
            action.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void ToDD_ConversionIsCorrect()
        {
            // Arrange
            DMS dms = new DMS(45, 30, 15.5);

            // Act
            DD dd = dms.ToDD();

            // Assert
            Assert.AreEqual(45.504305555555556, dd.DecimalDegrees, 0.000001);
        }
    }
}
