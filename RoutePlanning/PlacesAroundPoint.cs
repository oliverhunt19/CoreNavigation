using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutePlanning
{
    public class PlacesAroundPoint<T>
    {
        public PlacesAroundPoint(IReadOnlyList<T> places, LatLng centre)
        {
            Places = places;
            Centre = centre;
        }

        public IReadOnlyList<T> Places { get; }

        public LatLng Centre { get; }
    }
}
