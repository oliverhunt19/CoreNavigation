using FluentAssertions;
using RoutePlanning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestRoutePlanning.TestClasses;
using UnitsNet;
using UnitsNet.Units;

namespace TestRoutePlanning
{

    [TestClass]
    public class PlaceSearchTests
    {
        [TestMethod]
        public void GetAroundPoint_ShouldReturnPlacesAroundPoint_LatOnly_1Point()
        {
            // Arrange
            List<ILocation> allPlaces = new List<ILocation>
        {
            // Initialize some sample locations for testing
            new TestIlocation(new LatLng(1,0)),
            new TestIlocation(new LatLng()),
            // Add more locations as needed
        };

            PlaceSearch<ILocation> placeSearch = new PlaceSearch<ILocation>(allPlaces);
            LatLng centre = new LatLng();
            Length length = new Length(10, LengthUnit.Kilometer);  // Replace with your desired length

            // Act
            PlacesAroundPoint<ILocation> result = placeSearch.GetAroundPoint(centre, length);

            result.Places.Count.Should().Be(1);
        }

        [TestMethod]
        public void GetAroundPoint_ShouldReturnPlacesAroundPoint_LatOnly_2Points()
        {
            // Arrange
            List<ILocation> allPlaces = new List<ILocation>
        {
            // Initialize some sample locations for testing
            new TestIlocation(new LatLng(1,0)),
            new TestIlocation(new LatLng()),
            // Add more locations as needed
        };

            PlaceSearch<ILocation> placeSearch = new PlaceSearch<ILocation>(allPlaces);
            LatLng centre = new LatLng();
            Length length = new Length(112, LengthUnit.Kilometer);  // Replace with your desired length

            // Act
            PlacesAroundPoint<ILocation> result = placeSearch.GetAroundPoint(centre, length);

            result.Places.Count.Should().Be(2);
        }

        [TestMethod]
        public void GetAroundPoint_ShouldReturnPlacesAroundPoint_LatLng_1Point()
        {
            // Arrange
            List<ILocation> allPlaces = new List<ILocation>
        {
            // Initialize some sample locations for testing
            new TestIlocation(new LatLng(1,1)),
            new TestIlocation(new LatLng()),
            // Add more locations as needed
        };

            PlaceSearch<ILocation> placeSearch = new PlaceSearch<ILocation>(allPlaces);
            LatLng centre = new LatLng();
            Length length = new Length(100, LengthUnit.Kilometer);  // Replace with your desired length

            // Act
            PlacesAroundPoint<ILocation> result = placeSearch.GetAroundPoint(centre, length);

            result.Places.Count.Should().Be(1);
        }

        [TestMethod]
        public void GetAroundPoint_ShouldReturnPlacesAroundPoint_LatLng_2Points()
        {
            // Arrange
            List<ILocation> allPlaces = new List<ILocation>
        {
            // Initialize some sample locations for testing
            new TestIlocation(new LatLng(1,1)),
            new TestIlocation(new LatLng()),
            // Add more locations as needed
        };

            PlaceSearch<ILocation> placeSearch = new PlaceSearch<ILocation>(allPlaces);
            LatLng centre = new LatLng();
            Length length = new Length(112, LengthUnit.Kilometer);  // Replace with your desired length

            // Act
            PlacesAroundPoint<ILocation> result = placeSearch.GetAroundPoint(centre, length);

            result.Places.Count.Should().Be(2);
        }

        [TestMethod]
        public void GetAlongRoute_ShouldReturnPlacesAlongRoute()
        {

            Assert.Fail();

            // Arrange
            List<ILocation> allPlaces = new List<ILocation>
        {
            // Initialize some sample locations for testing
            new TestIlocation(new LatLng()),
            new TestIlocation(new LatLng()),
            // Add more locations as needed
        };

            PlaceSearch<ILocation> placeSearch = new PlaceSearch<ILocation>(allPlaces);
            BoundedRoute boundedRoute = new BoundedRoute(new List<LatLngBounds>() { new LatLngBounds(new LatLng(),new LatLng())});

            // Act
            PlacesAlongRoute<ILocation> result = placeSearch.GetAlongRoute(boundedRoute);

            // Assert
            // Add assertions to verify that the result contains the expected places along the route
            Assert.IsNotNull(result, "PlacesAlongRoute result should not be null.");
            // Add more assertions based on your specific logic and expectations
        }

        // Add more test methods to cover other functionalities of your PlaceSearch<T> class
    }

}
