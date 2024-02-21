using GoogleApi.Entities.Maps.Common;
using GoogleApi.Entities.Maps.Directions.Response;
using UnitsNet;

namespace RoutePlanning
{
    public class RouteInfo
    {
        public Route Route { get; }

        public Length Distance => Route.GetRouteDistance();

        public TimeSpan Duration => Route.GetRouteDuration();


        public RouteInfo(Route route)
        {
            Route = route;
        }

        public GBP FindRouteCost(FuelEfficiency fuelEfficiency, CostPerUnit<Volume> FuelCostPerLitre)
        {
            Length routeLength = Route.GetRouteDistance();

            double litres = routeLength.Kilometers / fuelEfficiency.As(UnitsNet.Units.FuelEfficiencyUnit.KilometerPerLiter);

            Volume fuelAmount = Volume.FromLiters(litres);

            return FuelCostPerLitre.CostForQuanity(fuelAmount);
        }

    }

    public static class RouteExtensions
    {
        public static Length GetRouteDistance(this Route route)
        {
            IEnumerable<Distance> distances = route.Legs.Select(x => x.Distance);
            Distance distance = distances.Sum();

            return Length.FromMeters(distance.Value);

        }

        public static TimeSpan GetRouteDuration(this Route route)
        {
            IEnumerable<GoogleApi.Entities.Maps.Common.Duration> durations = route.Legs.Select(x => x.Duration);
            return TimeSpan.FromSeconds(durations.Sum(x => x.Value));
        }

        public static Distance Add(this Distance d1, Distance d2)
        {
            return new Distance()
            {
                Value = d1.Value + d2.Value,
            };
        }

        public static Distance Sum(this IEnumerable<Distance> distances)
        {
            Distance distance = distances.First();
            for (var i = 1; i < distances.Count(); i++)
            {
                distance = distance.Add(distances.ElementAt(i));
            }
            return distance;
        }

        public static TimeSpan ToTimeSpan(this GoogleApi.Entities.Maps.Common.Duration duration)
        {
            return TimeSpan.FromSeconds(duration.Value);
        }


    }
}
