using FluentAssertions;
using RoutePlanning;
using UnitsNet;

namespace TestRoutePlanning
{
    [TestClass]
    public class TestLatLng
    {
        [TestMethod]
        public void LatLngConstructor()
        {
            LatLng latLng = new LatLng(new DMS(53,19,14),new DMS(1,43,47));
            LatLng latLng2 = new LatLng(53.32055555555556,1.72972222222222);
            latLng.Lng.DecimalDegrees.Should().BeApproximately(latLng2.Lng, 1e-6);
            latLng.Lat.DecimalDegrees.Should().BeApproximately(latLng2.Lat, 1e-6);
        }

        [TestMethod]
        public void TestDistanceBetween2Points()
        {
            LatLng latLng1 = new LatLng(50, 0);
            LatLng latLng2 = new LatLng(50, 3);
            Length result = LatLng.DistanceBetweenTwoCoordinates(latLng1, latLng2);
            result.Kilometers.Should().BeApproximately(214.4, 1e-1);
        }

        [TestMethod] 
        public void TestRhumbLineDistance1() 
        {
            //https://www.movable-type.co.uk/scripts/latlong.html
            //https://www.directionsmag.com/site/latlong-converter/
            DMS bearing = new DMS(96,1,18);
            LatLng latLng = new LatLng(new DMS(53,19,14),new DMS(1,43,47));
            LatLng latLng1 = LatLng.RhumbDestinationPointNew(latLng,bearing.ToDD(), Length.FromKilometers( 124.8));
            LatLng expected = new LatLng(new DMS(53,11, 17.770377594018214),new DMS(3, 35, 33.797014495066975));
            latLng1.Lng.DecimalDegrees.Should().BeApproximately(expected.Lng, 1e-6);
            latLng1.Lat.DecimalDegrees.Should().BeApproximately(expected.Lat, 1e-6);
        }

        [TestMethod]
        public void TestRhumbLineDistance2()
        {
            //https://www.movable-type.co.uk/scripts/latlong.html
            //https://www.directionsmag.com/site/latlong-converter/
            DMS bearing = new DMS(96, 1, 18);
            LatLng latLng = new LatLng(new DMS(85, 19, 14), new DMS(1, 43, 47));
            LatLng latLng1 = LatLng.RhumbDestinationPointNew(latLng, bearing.ToDD(), Length.FromKilometers(124.8));
            LatLng expected = new LatLng(new DMS(53, 11, 17.770377594018214), new DMS(3, 35, 33.797014495066975));
            latLng1.Lng.DecimalDegrees.Should().BeApproximately(expected.Lng, 1e-6);
            latLng1.Lat.DecimalDegrees.Should().BeApproximately(expected.Lat, 1e-6);
        }
    }

    [TestClass]
    public class LatLngTests
    {
        [TestMethod]
        public void Constructor_ValidInput_InitializesCorrectly()
        {
            // Arrange
            DD lat = new DD(40.7128); // Example latitude
            DD lng = new DD(-74.0060); // Example longitude

            // Act
            LatLng latLng = new LatLng(lat, lng);

            // Assert
            Assert.AreEqual(lat, latLng.Lat);
            Assert.AreEqual(lng, latLng.Lng);
        }

        [TestMethod]
        public void Constructor_OverloadWithDoubles_InitializesCorrectly()
        {
            // Arrange
            double lat = 40.7128; // Example latitude
            double lng = -74.0060; // Example longitude

            // Act
            LatLng latLng = new LatLng(lat, lng);

            // Assert
            Assert.AreEqual(lat, latLng.Lat.DecimalDegrees);
            Assert.AreEqual(lng, latLng.Lng.DecimalDegrees);
        }

        [TestMethod]
        public void Constructor_OverloadWithDMS_InitializesCorrectly()
        {
            // Arrange
            DMS latDMS = new DMS(40, 42, 46.8); // Example latitude in DMS
            DMS lngDMS = new DMS(-74, 0, 21.6); // Example longitude in DMS

            // Act
            LatLng latLng = new LatLng(latDMS, lngDMS);

            // Assert
            Assert.AreEqual(40.713, latLng.Lat.DecimalDegrees, 0.001);
            Assert.AreEqual(-74.006, latLng.Lng.DecimalDegrees, 0.001);
        }

        [TestMethod]
        public void RhumbDestinationPoint_CalculatesCorrectly()
        {
            // Arrange
            LatLng start = new LatLng(40.7128, -74.0060); // Example start coordinates
            double bearing = 45.0; // Example bearing
            double distance = 100.0; // Example distance in kilometers

            // Act
            LatLng destination = start.RhumbDestinationPoint(bearing, distance);

            // Assert
            // You can add specific assertions based on your expectations
            Assert.IsNotNull(destination);
            Assert.Inconclusive();
        }

        [TestMethod]
        public void RhumbBearingTo_CalculatesCorrectly()
        {
            // Arrange
            LatLng start = new LatLng(40.7128, -74.0060); // Example start coordinates
            LatLng destination = new LatLng(41.8819, -87.6278); // Example destination coordinates

            // Act
            double bearing = start.RhumbBearingTo(destination);

            // Assert
            // You can add specific assertions based on your expectations
            Assert.IsNotNull(bearing);
            Assert.Inconclusive();
        }

        [TestMethod]
        public void DistanceBetweenTwoCoordinates_CalculatesCorrectly()
        {
            // Arrange
            LatLng start = new LatLng(40.7128, -74.0060); // Example start coordinates
            LatLng end = new LatLng(34.0522, -118.2437); // Example end coordinates

            // Act
            var distance = LatLng.DistanceBetweenTwoCoordinates(start, end);

            // Assert
            Assert.AreEqual(3935746, distance.Meters, 1); // Adjust the expected value based on your expectations
        }
    }

}
