using FluentAssertions;
using RoutePlanning;

namespace TestRoutePlanning
{
    [TestClass]
    public class TestRouteBoxer
    {
        [TestMethod]
        public void TestRouteBoxerTest()
        {
            RouteBoxer routeBoxer = new RouteBoxer();

            LatLng ne = new LatLng(50, 0);
            LatLng sw = new LatLng(0, 0);

            List<LatLng> twopoints = new List<LatLng>
            {
                sw,
                ne,
            };

            IReadOnlyList<LatLngBounds> latLngBounds = routeBoxer.Box(twopoints, 1);
            latLngBounds.Count.Should().Be(1);
            latLngBounds[0].NorthEast.Lat.DecimalDegrees.Should().Be(ne.Lat.DecimalDegrees);
            latLngBounds[0].NorthEast.Lng.DecimalDegrees.Should().Be(ne.Lng.DecimalDegrees);

            latLngBounds[0].SouthWest.Lng.DecimalDegrees.Should().Be(sw.Lng.DecimalDegrees);
            latLngBounds[0].SouthWest.Lng.DecimalDegrees.Should().Be(sw.Lng.DecimalDegrees);
            
        }

        [TestMethod]
        public void TestSingleLineRouteBoxer()
        {
            double startingValue = 0;
            double endValue = 10;
            double increment = 0.1;

            var result = Enumerable
              .Range(0, (int)((endValue - startingValue) / increment) + 1)
              .Select(i => startingValue + increment * i).ToList();

            List<LatLng> latLngs = result.Select(x => new LatLng(x, 0)).ToList();

            RouteBoxer routeBoxer = new RouteBoxer();
            IReadOnlyList<LatLngBounds> latLngBounds = routeBoxer.Box(latLngs, 1);

            latLngBounds.Count.Should().Be(1);
        }
    }
}
