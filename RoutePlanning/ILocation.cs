using GoogleApi.Entities.Common;

namespace RoutePlanning
{
    public interface ILocation : ICoordinate
    {
        

        Address GetAddress();
    }

    public interface ICoordinate
    {
        LatLng Coordinates { get; }
    }
}
