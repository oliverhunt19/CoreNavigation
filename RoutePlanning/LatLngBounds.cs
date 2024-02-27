using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

namespace RoutePlanning
{
    public struct LatLngBounds : ICloneable
    {
        public LatLng NorthEast { get; }
        public LatLng SouthWest { get; }

        public LatLngBounds(LatLng southWest, LatLng northEast)
        {
            this.SouthWest = southWest;
            this.NorthEast = northEast;
        }

        public LatLngBounds Clone()
        {
            return new LatLngBounds(SouthWest, NorthEast);
        }

        public LatLngBounds Extend(LatLng point)
        {
            double swLat = Math.Min(this.SouthWest.Lat, point.Lat);
            double swLong = Math.Min(this.SouthWest.Lng, point.Lng);
            LatLng newSouthWest = new LatLng(swLat, swLong);


            double neLat = Math.Max(this.NorthEast.Lat, point.Lat);
            double neLon = Math.Max(this.NorthEast.Lng, point.Lng);
            LatLng newNorthEast = new LatLng(neLat, neLon);
            return new LatLngBounds(newSouthWest, newNorthEast);
        }

        public static LatLngBounds GetBounds(IReadOnlyList<LatLng> latLngs)
        {
            if (latLngs.Count < 1)
            {
                throw new ArgumentException("Not enough inputs");
            }
            LatLngBounds latLngBounds = new LatLngBounds(latLngs.First(), latLngs.First());
            foreach (var latlng in latLngs)
            {
                latLngBounds = latLngBounds.Extend(latlng);
            }
            return latLngBounds;
        }

        /// <summary>
        /// Find the centre of the bounding
        /// </summary>
        /// <returns>The latitude and longitude for the centre of the box</returns>
        public LatLng GetCenter()
        {
            double lat = (this.SouthWest.Lat + this.NorthEast.Lat) / 2;
            double lng = (this.SouthWest.Lng + this.NorthEast.Lng) / 2;
            return new LatLng(lat, lng);
        }

        /// <summary>
        /// Finds the distance between the edge of the box and the centre
        /// </summary>
        /// <returns></returns>
        public Length GetDistanceFromCentre()
        {
            LatLng centre = GetCenter();
            return LatLng.DistanceBetweenTwoCoordinates(centre, NorthEast);
        }

        /// <summary>
        /// Finds the bounding box for the given centre and the distance to the corner of the box
        /// </summary>
        /// <param name="centre"></param>
        /// <param name="dist"></param>
        /// <returns></returns>
        public static LatLngBounds GetBoundingBox(LatLng centre, Length dist)
        {
            double correctedDistance = dist.As(UnitsNet.Units.LengthUnit.Kilometer) * Math.Sqrt(2);
            return new LatLngBounds(
                LatLng.RhumbDestinationPoint(centre, 225, correctedDistance),
                LatLng.RhumbDestinationPoint(centre, 45, correctedDistance));
        }



        public IReadOnlyList<T> GetAllInBox<T>(IEnumerable<T> values)
                where T : ILocation
        {
            Func<ILocation, bool> Contained = ContainedInBox;

            List<T> Results = new List<T>();
            Parallel.ForEach(values, x =>
            {
                if (Contained(x))
                {
                    Results.Add(x);

                }
            });
            return Results;
        }

        private bool ContainedInLatitude(ILocation location)
        {
            double lat = location.Coordinates.Lat;
            return lat <= NorthEast.Lat && lat >= SouthWest.Lat;
        }
        private bool ContainedInLongitude(ILocation location)
        {
            double lng = location.Coordinates.Lng;
            return lng <= NorthEast.Lng && lng >= SouthWest.Lng;
        }

        /// <summary>
        /// Finds if the location is inside the box
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool ContainedInBox(ILocation location)
        {
            if (!ContainedInLatitude(location)) return false;
            if (!ContainedInLongitude(location)) return false;
            return true;
        }

        public static LatLngBounds GetLargestBox(IReadOnlyList<LatLngBounds> latLngBounds)
        {
            LatLngBounds firstBox = latLngBounds.First().Clone();
            foreach (LatLngBounds latLng in latLngBounds)
            {
                firstBox = firstBox.Extend(latLng.NorthEast);
                firstBox = firstBox.Extend(latLng.SouthWest);
            }

            return firstBox;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }


}
