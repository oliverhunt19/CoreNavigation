using GoogleApi;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Maps.Geocoding;
using GoogleApi.Entities.Maps.Geocoding.Address.Request;
using GoogleApi.Entities.Places.AutoComplete.Request;
using GoogleApi.Entities.Places.AutoComplete.Response;

namespace RoutePlanning
{
    public static class AddressAutoComplete
    {

        public static async Task<IReadOnlyList<Address>> FindAddress(string address, string key)
        {
            PlacesAutoCompleteRequest query = new()
            {
                Input = address,
                Key = key
            };
            PlacesAutoCompleteResponse responce = await GooglePlaces.AutoComplete.QueryAsync(query);

            return responce.Predictions.Select(x => new Address(x.Description)).ToList();
        }

        public static async Task<Coordinate> FindCoordinate(this Address address, string Key)
        {
            AddressGeocodeRequest addressGeocodeRequest = new()
            {
                Address = address.String,
                Key = Key,
            };
            GeocodeResponse response = await GoogleMaps.Geocode.AddressGeocode.QueryAsync(addressGeocodeRequest);
            return response.Results.First().Geometry.Location;
        }

        public static async Task<LatLng> FindLatlng(this Address address, string Key)
        {
            Coordinate coordinate = await FindCoordinate(address, Key);
            return LatLng.FromGoogleCoordinates(coordinate);
        }
    }
}
