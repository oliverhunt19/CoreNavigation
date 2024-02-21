namespace RoutePlanning
{
    //Decimal Degrees
    //https://gsp.humboldt.edu/olm/Lessons/GIS/01%20SphericalCoordinates/Reporting_Geographic_Coordinates.html#:~:text=First%2C%20recognize%20that%3A,3600%20seconds%20%3D%20one%20degree
    public struct DD
    {
        public DD(double decimalDegrees)
        {
            DecimalDegrees = decimalDegrees;
        }

        public double DecimalDegrees { get; }

        public DMS ToDMS()
        {
            int degrees = (int)Math.Floor(DecimalDegrees);
            double DM = (DecimalDegrees - degrees)*60;

            int minute = (int)Math.Floor(DM);
            double DS = (DM - minute)*60;
            return new DMS(degrees, minute, DS);
        }

        public double ToRad()
        {
            return DecimalDegrees * Math.PI / 180;
        }

        public static implicit operator double (DD d)
        {
            return d.DecimalDegrees;
        }

        public override string ToString()
        {
            return DecimalDegrees.ToString();
        }
    }
}
