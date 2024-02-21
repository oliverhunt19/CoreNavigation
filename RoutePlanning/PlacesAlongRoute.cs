using GoogleApi.Entities.Maps.Directions.Response;

namespace RoutePlanning
{
    public class PlacesAlongRoute<T>
        where T : ILocation
    {
        public PlacesAlongRoute(Route route, IReadOnlyList<T> places)
        {
            Route = route;
            Places = places;
        }

        public Route Route { get; }

        public IReadOnlyList<T> Places { get; }
    }
}
