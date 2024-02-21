using GoogleApi.Entities.Common;
using GoogleApi.Entities.Maps.Common;
using GoogleApi.Entities.Maps.Common.Enums;
using GoogleApi.Entities.Maps.Directions.Request;
using WayPoint = GoogleApi.Entities.Maps.Directions.Request.WayPoint;

namespace RoutePlanning.RouteOptimisation
{
    public class RouteRequestBase
    {
        /// <summary>
        /// avoid (optional) indicates that the calculated route(s) should avoid the indicated features. Currently, this parameter supports the following two arguments:
        /// tolls indicates that the calculated route should avoid toll roads/bridges.
        /// highways indicates that the calculated route should avoid highways.
        /// (For more information see Route Restrictions below.)
        /// </summary>
        public virtual AvoidWay Avoid { get; set; } = AvoidWay.Nothing;

        /// <summary>
        /// Traffic mdel (defaults to best_guess).
        /// Specifies the assumptions to use when calculating time in traffic. This setting affects the value returned
        /// in the duration_in_traffic field in the response, which contains the predicted time in traffic based on historical averages.
        /// The traffic_model parameter may only be specified for requests where the travel mode is driving, and where the request includes a departure_time,
        /// and only if the request includes an API key or a Google Maps APIs Premium Plan client ID.The available values for this parameter are:
        /// </summary>
        public virtual TrafficModel TrafficModel { get; set; } = TrafficModel.Best_Guess;


        /// <summary>
        /// The time of arrival.
        /// Required when TravelMode = Transit
        /// Specifies the desired time of arrival for transit directions, in seconds since midnight, January 1, 1970 UTC.
        /// You can specify either departure_time or arrival_time, but not both.
        /// Note that arrival_time must be specified as an integer.
        /// </summary>
        public virtual DateTime? ArrivalTime { get; set; }

        /// <summary>
        /// The time of departure.
        /// Required when TravelMode = Transit
        /// Specifies the desired time of departure.
        /// You can specify the time as an integer in seconds since midnight, January 1, 1970 UTC.
        /// Alternatively, you can specify a value of now, which sets the departure time to the current time (correct to the nearest second).
        /// The departure time may be specified in two cases:
        /// 1. For requests where the travel mode is transit:
        /// You can optionally specify one of departure_time or arrival_time.
        /// If neither time is specified, the departure_time defaults to now (that is, the departure time defaults to the current time).
        /// 2. For requests where the travel mode is driving:
        /// You can specify the departure_time to receive a route and trip duration (response field: duration_in_traffic)
        /// that take traffic conditions into account.
        /// This option is only available if the request contains a valid API key, or a valid Google Maps APIs Premium Plan client ID and signature.
        /// The departure_time must be set to the current time or some time in the future. It cannot be in the past.
        /// </summary>
        public virtual DateTime? DepartureTime { get; set; }



        public virtual string Key { get; set; }
    }

    public class RouteOptimisationRequest<T> : RouteRequestBase
        where T : ILocation
    {

        public PlacesAlongRoute<T> Route { get; set; }

        public RouteOptimisationRequest()
        {

        }

        public bool RouteOk()
        {
            return Route.Route.Legs.Count() == 1;
        }

        internal DirectionsRequest GetDirectionsRequest(T location)
        {
            return new DirectionsRequest()
            {
                Origin = new LocationEx(Route.Route.Legs.First().StartLocation.ToCoordinateEx()),
                Destination = new LocationEx(Route.Route.Legs.First().EndLocation.ToCoordinateEx()),
                Alternatives = false,
                ArrivalTime = ArrivalTime,
                DepartureTime = DepartureTime,
                Avoid = Avoid,
                TrafficModel = TrafficModel,
                Key = Key,
                WayPoints = new List<WayPoint>()
                {
                    new WayPoint(new LocationEx(location.Coordinates.ToGoogleCoordinateEx()))
                }
            };
        }
    }

    public static class RouteOptimisationExtensions
    {
        public static CoordinateEx ToCoordinateEx(this Coordinate coordinate)
        {
            return new CoordinateEx(coordinate.Latitude, coordinate.Longitude);
        }
    }
}
