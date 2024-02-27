using FluentAssertions;
using GoogleApi.Entities.Common;
using RoutePlanning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

namespace TestRoutePlanning
{
    [TestClass]
    public class TestLatLngBounds
    {
        [TestMethod]
        public void Extend_ExtendsBoundsCorrectly()
        {
            // Arrange
            LatLng southWest = new LatLng(10, 20);
            LatLng northEast = new LatLng(30, 40);
            LatLngBounds bounds = new LatLngBounds(southWest, northEast);
            LatLng point = new LatLng(5, 15);

            // Act
            LatLngBounds extendedBounds = bounds.Extend(point);

            // Assert
            extendedBounds.SouthWest.Lat.DecimalDegrees.Should().BeApproximately(5, 1e-9);
            extendedBounds.SouthWest.Lng.DecimalDegrees.Should().BeApproximately(15, 1e-9);
            extendedBounds.NorthEast.Lat.DecimalDegrees.Should().BeApproximately(30, 1e-9);
            extendedBounds.NorthEast.Lng.DecimalDegrees.Should().BeApproximately(40, 1e-9);
        }

        [TestMethod]
        public void TestGetBounds()
        {
            var LatList = new List<LatLng>()
            {
                new LatLng(10, 20),
                new LatLng(20,30),
            };
            LatLngBounds bounds = LatLngBounds.GetBounds(LatList);
            bounds.NorthEast.Lat.DecimalDegrees.Should().BeApproximately(20, 1e-9);
            bounds.NorthEast.Lng.DecimalDegrees.Should().BeApproximately(30, 1e-9);
            bounds.SouthWest.Lat.DecimalDegrees.Should().BeApproximately(10, 1e-9);
            bounds.SouthWest.Lng.DecimalDegrees.Should().BeApproximately(20, 1e-9);
        }

        [TestMethod]
        public void GetBounds_ReturnsCorrectBounds()
        {
            // Arrange
            var latLngs = new List<LatLng>
        {
            new LatLng(10, 20),
            new LatLng(30, 40),
            new LatLng(5, 15)
        };

            // Act
            var bounds = LatLngBounds.GetBounds(latLngs);

            // Assert
            Assert.AreEqual(5,  bounds.SouthWest.Lat.DecimalDegrees,1e-9);
            Assert.AreEqual(15, bounds.SouthWest.Lng.DecimalDegrees,1e-9);
            Assert.AreEqual(30, bounds.NorthEast.Lat.DecimalDegrees,1e-9);
            Assert.AreEqual(40, bounds.NorthEast.Lng.DecimalDegrees,1e-9);
        }

        [TestMethod]
        public void GetBounds_ThrowsException_WhenEmptyList()
        {
            // Arrange
            var emptyList = new List<LatLng>();

            // Assert
            Assert.ThrowsException<ArgumentException>(() => LatLngBounds.GetBounds(emptyList));
        }

        [TestMethod]
        public void GetCenter_ReturnsCorrectCenter()
        {
            // Arrange
            LatLng southWest = new LatLng(10, 20);
            LatLng northEast = new LatLng(30, 40);
            LatLngBounds bounds = new LatLngBounds(southWest, northEast);

            // Act
            LatLng center = bounds.GetCenter();

            // Assert
            center.Lat.DecimalDegrees.Should().BeApproximately(20, 1e-9);
            center.Lng.DecimalDegrees.Should().BeApproximately(30, 1e-9);
        }


        [TestMethod]
        public void GetBoundingBox_ReturnsCorrectBoundingBox()
        {
            // Arrange
            LatLng center = new LatLng(20, 30);
            Length distance = Length.FromKilometers(10); // Example distance
            LatLngBounds expectedBounds = new LatLngBounds(
                LatLng.RhumbDestinationPoint(center, 225, distance.Kilometers),
                LatLng.RhumbDestinationPoint(center, 45, distance.Kilometers));

            // Act
            LatLngBounds resultBounds = LatLngBounds.GetBoundingBox(center, distance);

            // Assert
            Assert.AreEqual(expectedBounds.SouthWest.Lat, resultBounds.SouthWest.Lat, 0.001); // Tolerate some rounding errors
            Assert.AreEqual(expectedBounds.SouthWest.Lng, resultBounds.SouthWest.Lng, 0.001);
            Assert.AreEqual(expectedBounds.NorthEast.Lat, resultBounds.NorthEast.Lat, 0.001);
            Assert.AreEqual(expectedBounds.NorthEast.Lng, resultBounds.NorthEast.Lng, 0.001);
        }

        [TestMethod]
        public void ContainedInBox_ReturnsTrue_WhenLocationInsideBox()
        {
            // Arrange
            LatLng southWest = new LatLng(10, 20);
            LatLng northEast = new LatLng(30, 40);
            LatLngBounds bounds = new LatLngBounds(southWest, northEast);
            ILocation locationInside = new MockLocation(15, 30); // Example location inside bounds

            // Act
            bool result = bounds.ContainedInBox(locationInside);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ContainedInBox_ReturnsFalse_WhenLocationOutsideBox()
        {
            // Arrange
            LatLng southWest = new LatLng(10, 20);
            LatLng northEast = new LatLng(30, 40);
            LatLngBounds bounds = new LatLngBounds(southWest, northEast);
            ILocation locationOutside = new MockLocation(5, 15); // Example location outside bounds

            // Act
            bool result = bounds.ContainedInBox(locationOutside);

            // Assert
            Assert.IsFalse(result);
        }

        // Mock implementation of ILocation for testing purposes
        public class MockLocation : ILocation
        {
            public LatLng Coordinates { get; }

            public MockLocation(double lat, double lng)
            {
                Coordinates = new LatLng(lat, lng);
            }

            public Address GetAddress()
            {
                throw new NotImplementedException();
            }
        }
    }
}
