using RoutePlanning;

namespace TestRoutePlanning
{
    [TestClass]
    public class TestDD
    {
        [TestMethod]
        public void Constructor_ValidInput_InitializesCorrectly()
        {
            // Arrange
            double decimalDegrees = 45.504305555555556;

            // Act
            DD dd = new DD(decimalDegrees);

            // Assert
            Assert.AreEqual(decimalDegrees, dd.DecimalDegrees, 0.000001);
        }

        [TestMethod]
        public void ToDMS_ConversionIsCorrect()
        {
            // Arrange
            DD dd = new DD(45.504305555555556);

            // Act
            DMS dms = dd.ToDMS();

            // Assert
            Assert.AreEqual(45, dms.Degrees);
            Assert.AreEqual(30, dms.Minutes);
            Assert.AreEqual(15.5, dms.Seconds, 0.000001);
        }

        [TestMethod]
        public void ToRad_ConversionIsCorrect()
        {
            // Arrange
            DD dd = new DD(45.504305555555556);

            // Act
            double radians = dd.ToRad();

            // Assert
            Assert.AreEqual(0.793036688, radians, 0.000001);
        }

        [TestMethod]
        public void ImplicitConversionToDouble_IsCorrect()
        {
            // Arrange
            DD dd = new DD(45.504305555555556);

            // Act
            double result = dd;

            // Assert
            Assert.AreEqual(45.504305555555556, result, 0.000001);
        }
    }
}
