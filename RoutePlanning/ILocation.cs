using GoogleApi.Entities.Common;

namespace RoutePlanning
{
    public interface ILocation
    {
        LatLng Coordinates { get; }

        Address GetAddress();
    }
}
