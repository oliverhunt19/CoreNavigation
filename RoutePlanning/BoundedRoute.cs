using GoogleApi.Entities.Maps.Directions.Response;

namespace RoutePlanning
{
    public class BoundedRoute
    {
        public BoundedRoute(Route route, IReadOnlyList<LatLngBounds> routeBounds)
        {
            Route = route;
            RouteBounds = routeBounds;
        }

        public BoundedRoute(IReadOnlyList<LatLngBounds> routeBounds)
        {
            RouteBounds = routeBounds;
        }

        public Route? Route { get; }

        public IReadOnlyList<LatLngBounds> RouteBounds { get; }

    }
}
