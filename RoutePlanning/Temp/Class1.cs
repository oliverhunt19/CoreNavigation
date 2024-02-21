using GoogleApi;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Maps.Common;
using GoogleApi.Entities.Maps.Common.Enums;
using GoogleApi.Entities.Maps.Directions.Request;
using GoogleApi.Entities.Places.AutoComplete.Request;
using GoogleApi.Entities.Places.Search.Common.Enums;
using GoogleApi.Entities.Places.Search.NearBy.Request;

namespace RoutePlanning.Temp
{
    public class Class1
    {
        public async Task testAPI()
        {

            var request1 = new PlacesAutoCompleteRequest
            {
                Key = "AIzaSyDyV-wnHeO8VmZjpYXxwBpKw1iT6w4VImo",
                Input = "Renishaw exeter"
            };

            var response1 = await GooglePlaces.AutoComplete.QueryAsync(request1);
            var results1 = response1.Predictions.ToArray();

            var request2 = new PlacesAutoCompleteRequest
            {
                Key = "AIzaSyDyV-wnHeO8VmZjpYXxwBpKw1iT6w4VImo",
                Input = "19 Dukes orchard bradninch"
            };

            var response2 = await GooglePlaces.AutoComplete.QueryAsync(request2);
            var results2 = response2.Predictions.ToArray();

            var origin = new Address(results1[0].Description);
            var destination = new Address(results2[0].Description);
            var request = new DirectionsRequest
            {
                Key = "AIzaSyDyV-wnHeO8VmZjpYXxwBpKw1iT6w4VImo",
                Origin = new LocationEx(origin),
                Destination = new LocationEx(destination),
                WayPoints = new List<WayPoint>()
                {
                    new WayPoint(new LocationEx(new PlusCode("9C2RPFMV+GG"))),
                },
                DepartureTime = DateTime.Now.AddHours(2),
                TravelMode = TravelMode.Driving,
                Alternatives = true,
            };

            var result = await GoogleMaps.Directions.QueryAsync(request);

        }

        public async Task TestPetrol()
        {
            var request = new PlacesNearBySearchRequest
            {
                Key = "AIzaSyDyV-wnHeO8VmZjpYXxwBpKw1iT6w4VImo",
                Location = new Coordinate(50.73005358264513, -3.5100744656480476),
                Radius = 5000,
                Type = SearchPlaceType.GasStation
            };

            var response = await GooglePlaces.Search.NearBySearch.QueryAsync(request);
            var results = response.Results.ToList();
            //results[0].PlusCode.GlobalCode;
        }
    }
}
