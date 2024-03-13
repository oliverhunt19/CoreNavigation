using GoogleApi.Entities.Common;
using GoogleApi.Entities.Maps.Common;
using UnitsNet;

namespace RoutePlanning
{

    public struct LatLng
    {
        public DD Lat { get; set; }
        public DD Lng { get; set; }

        public LatLng(DD lat, DD lng)
        {
            this.Lat = lat;
            this.Lng = lng;
        }

        public LatLng(double lat, double lng) : this(new DD(lat), new DD(lng))
        {

        }

        public LatLng(DMS lat, DMS lng) : this(lat.ToDD(), lng.ToDD())
        {

        }

        //public LatLng(double latdeg, double latmin, double lngdeg, double lngmin) : this(latdeg +latmin/60,lngdeg+lngmin/60)
        //{

        //}

        public LatLng RhumbDestinationPoint(double brng, double dist)
        {
            return LatLng.RhumbDestinationPoint(this, brng, dist);
        }

        public double RhumbBearingTo(LatLng dest)
        {
            return LatLng.RhumbBearingTo(this, dest);
        }

        public static LatLng FromGoogleCoordinates(Coordinate coordinate)
        {
            return new LatLng(coordinate.Latitude, coordinate.Longitude);
        }

        public Coordinate ToGoogleCoordinate()
        {
            return new Coordinate(this.Lat, this.Lng);
        }

        public CoordinateEx ToGoogleCoordinateEx()
        {
            return new CoordinateEx(this.Lat, this.Lng);
        }


        /* Based on the Latitude/longitude spherical geodesy formulae & scripts
               at http://www.movable-type.co.uk/scripts/latlong.html
               (c) Chris Veness 2002-2010
               */
        public static LatLng RhumbDestinationPoint(LatLng start, double brng, double dist)
        {
            double R = 6371; // earth's mean radius in km
            double d = dist / R;  // d = angular distance covered on earth's surface
            double lat1 = start.Lat.ToRad(), lon1 = start.Lng.ToRad();
            brng = brng.ToRad();
            double lat2 = lat1 + d * Math.Cos(brng);
            double dLat = lat2 - lat1;
            double dPhi = Math.Log(Math.Tan(lat2 / 2 + Math.PI / 4) / Math.Tan(lat1 / 2 + Math.PI / 4));
            double q = (Math.Abs(dLat) > 1e-10) ? dLat / dPhi : Math.Cos(lat1);
            double dLon = d * Math.Sin(brng) / q;
            // check for going past the pole
            if (Math.Abs(lat2) > Math.PI / 2)
            {
                lat2 = lat2 > 0 ? Math.PI - lat2 : -(Math.PI - lat2);
            }
            double lon2 = (lon1 + dLon + Math.PI) % (2 * Math.PI) - Math.PI;
            if (double.IsNaN(lat2) || double.IsNaN(lon2))
            {
                return default;
            }
            return new LatLng(lat2.ToDeg(), lon2.ToDeg());
            //return RhumbDestinationPointNew(start, new DD(brng), Length.FromKilometers(dist));
        }


        public static LatLng RhumbDestinationPointNew(LatLng start, DD brng, Length dist)
        {
            double R = 6371; // earth's mean radius in km
            double d = dist.Kilometers / R;
            double lat1 = start.Lat.ToRad();
            double lng1 = start.Lng.ToRad();
            double theta = brng.ToRad();

            double lat2 = Math.Asin(Math.Sin(lat1) * Math.Cos(d) + Math.Cos(lat1) * Math.Sin(d) * Math.Cos(theta));
            double atan2_1 = Math.Sin(theta) * Math.Sin(d) * Math.Cos(lat1);
            double atan2_2 = Math.Cos(d) - Math.Sin(lat1) * Math.Sin(lat2);
            double lng2 = lng1 + Math.Atan2(atan2_1, atan2_2);
            return new LatLng(lat2.ToDeg(), lng2.ToDeg());
        }
        public static double RhumbBearingTo(LatLng start, LatLng dest)
        {
            double dLon = (dest.Lng - start.Lng).ToRad();
            double dPhi = Math.Log(Math.Tan(dest.Lat.ToRad() / 2 + Math.PI / 4) / Math.Tan(start.Lat.ToRad() / 2 + Math.PI / 4));
            if (Math.Abs(dLon) > Math.PI)
            {
                dLon = dLon > 0 ? -(2 * Math.PI - dLon) : (2 * Math.PI + dLon);
            }
            return Math.Atan2(dLon, dPhi).ToBrng();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        /// <see cref="https://www.omnicalculator.com/other/latitude-longitude-distance#obtaining-the-distance-between-two-points-on-earth-distance-between-coordinates"/>
        public static Length DistanceBetweenTwoCoordinates(LatLng start, LatLng end)
        {
            double R = 6371; // earth's mean radius in km
            double T1 = start.Lat.ToRad();
            double T2 = end.Lat.ToRad();

            double P1 = start.Lng.ToRad();
            double P2 = end.Lng.ToRad();

            double t2 = Math.Pow(Math.Sin((T2 - T1) / 2), 2);

            double t3 = Math.Cos(T1) * Math.Cos(T2) * Math.Pow(Math.Sin((P2 - P1) / 2), 2);
            double t4 = 2 * R * Math.Asin(Math.Sqrt(t2 + t3));
            return Length.FromKilometers(t4);
        }

        public override string ToString()
        {
            return $"{Lat}|{Lng}";
        }
    }

    public class EquirectangularProjection
    {
        private const double Radius = 6371;
        private readonly double cosLat;

        public readonly LatLng LatLngCentre;

        public EquirectangularProjection(LatLng latLng)
        {
            LatLngCentre = latLng;
            cosLat = Math.Cos(latLng.Lat.ToRad());
        }

        public EquirectangularCoordinate GetEquirectangularCoordinate(LatLng latLng)
        {
            return new EquirectangularCoordinate(Radius * (latLng.Lng.ToRad()-LatLngCentre.Lng.ToRad()) * cosLat,Radius*(latLng.Lat.ToRad()-LatLngCentre.Lat.ToRad()));
        }
    }

    public struct EquirectangularCoordinate
    {
        public readonly double X;

        public readonly double Y;

        public EquirectangularCoordinate(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public class PlaneProjection
    {
        public readonly DD LatLngCentre;

        public PlaneProjection(DD latLngCentre)
        {
            LatLngCentre = latLngCentre;
        }

        public PlaneCoordinate GetPlaneCoordinate(LatLng latLng)
        {
            double cCy = Math.Cos(LatLngCentre.ToRad());
            double sCy = Math.Sin(LatLngCentre.ToRad());
            //double cCx = Math.Cos(LatLngCentre.Lng.ToRad());
            //double sCx = Math.Sin(LatLngCentre.Lng.ToRad());


            double cPy = Math.Cos(latLng.Lat.ToRad());
            double sPy = Math.Sin(latLng.Lat.ToRad());
            double cPx = Math.Cos(latLng.Lng.ToRad());
            double sPx = Math.Sin(latLng.Lng.ToRad());

            //double x0 = cCx*cPy*cPx-sCx*cPy*sPx;
            //double y0 = sCx*cPy*sPx+cCx*cPy*sPx;
            //double z0 = sPy;


            //double x1 = cCy*x0+sCy*sPy;
            //double x2 = sCx*cPy*sPx+cCx*cPy*sPx;
            //double x3 = -sCy*x0+cCy*sPy;

            double x1 = cCy*cPy*cPx+sCy*sPy;
            double x2 = cPy*sPx;
            double x3 = -sCy*cPy*cPx+sPy*cCy;

            double y = Math.Asin(x3);
            double x = Math.Atan2(x2,x1);
            return new PlaneCoordinate(x.ToDeg(), y.ToDeg());
        }
    }

    public struct PlaneCoordinate
    {
        public readonly double X;

        public readonly double Y;

        public PlaneCoordinate(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
