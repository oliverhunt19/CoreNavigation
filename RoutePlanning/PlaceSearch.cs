using UnitsNet;

namespace RoutePlanning
{
    public class PlaceSearch<T> where T : ILocation
    {
        public PlaceSearch(IReadOnlyList<T> allPlaces)
        {
            AllPlaces = allPlaces;
        }

        public IReadOnlyList<T> AllPlaces { get; }


        public PlacesAroundPoint<T> GetAroundPoint(LatLng centre, Length length)
            
        {
            LatLngBounds boxBounds = LatLngBounds.GetBoundingBox(centre, length);
            IReadOnlyList<T> allInBox = boxBounds.GetAllInBox(AllPlaces);
            return new PlacesAroundPoint<T>(allInBox, centre);
        }

        public PlacesAlongRoute<T> GetAlongRoute(BoundedRoute boundedRoute)
        {
            IReadOnlyList<T> alongRoute = GetAllFromBounds(boundedRoute.RouteBounds, AllPlaces);
            return new PlacesAlongRoute<T>(boundedRoute.Route, alongRoute);
        }

        private static IReadOnlyList<T> GetAllFromBounds(IReadOnlyList<LatLngBounds> latLngBounds, IReadOnlyList<T> values)
        {
            LatLngBounds largestBox = LatLngBounds.GetLargestBox(latLngBounds);
            IReadOnlyList<T> allInBox = largestBox.GetAllInBox(values);

            List<T> result = new List<T>();
            foreach(LatLngBounds latLng in latLngBounds)
            {
                IReadOnlyList<T> AllInBox = latLng.GetAllInBox(allInBox);
                result.AddRange(AllInBox);
            }
            return result;
        }
    }
}
