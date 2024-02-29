using System.Collections.ObjectModel;
using UnitsNet;

namespace RoutePlanning
{
    public abstract class CollectionOfILocations<TLocation> : ReadOnlyCollection<TLocation> where TLocation : ILocation
    {
        private readonly PlaceSearch<TLocation> _aroundPoint;

        protected CollectionOfILocations(IList<TLocation> locations) : base(locations) 
        {
            _aroundPoint = new PlaceSearch<TLocation>(locations.ToList());
        }

        public PlacesAroundPoint<TLocation> GetAroundPoint(LatLng centre, Length length)
        {
            return _aroundPoint.GetAroundPoint(centre, length);
        }
    }
}
