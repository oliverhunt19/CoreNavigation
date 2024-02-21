using GoogleApi;
using GoogleApi.Entities.Maps.Directions.Response;
using GoogleApi.Entities.Maps.StaticMaps.Request;
using GoogleApi.Entities.Maps.StaticMaps.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutePlanning
{
    public class MapRouteImageGenerator<T>
        where T : class, IGetRoute
    {

        public string Key { get; set; }

        public MapRouteImageGenerator(string Key)
        {
            this.Key = Key;
        }

        public async Task<RouteMapImage<T>> GetImage(T route)
        {
            StaticMapsRequest request = new StaticMapsRequest()
            {
                Paths = new List<MapPath>()
                {
                    new MapPath()
                    {
                        Points = route.GetRoute().OverviewPath.Line.Select(x=>new GoogleApi.Entities.Maps.Common.Location(x)),
                    }
                },
                Key = Key,
            };
            var resp = await GoogleMaps.StaticMaps.QueryAsync(request);
            return new RouteMapImage<T>(resp, route);
        }
    }

    public interface IGetRoute
    {
        Route GetRoute();
    }



    public class RouteMapImage<T>
    {
        public StaticMapsResponse staticMapsResponse { get; }

        public T Value { get; }

        public RouteMapImage(StaticMapsResponse staticMapsResponse, T value)
        {
            this.staticMapsResponse = staticMapsResponse;
            Value = value;
        }

        public Stream Stream => staticMapsResponse.Stream;

        public Task SaveToFile(string path)
        {
            return SaveToFile(new FileInfo(path));
        }

        public async Task SaveToFile(FileInfo file)
        {
            FileStream fileStream = file.OpenWrite();
            await staticMapsResponse.Stream.CopyToAsync(fileStream);
        }
    }
}
