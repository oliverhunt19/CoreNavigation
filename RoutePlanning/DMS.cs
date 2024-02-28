namespace RoutePlanning
{
    //Degrees Minutes Seconds
    public struct DMS
    {
        public DMS(int degrees, int minutes, double seconds)
        {
            if (minutes > 60)
            {
                throw new ArgumentException("minutes cannot be larger than 60");
            }
            if(seconds > 60)
            {
                throw new ArgumentException("Seconds cannot be larger than 60");
            }
            Degrees = degrees;
            Minutes = minutes;
            Seconds = seconds;
        }

        public int Degrees { get; }

        public int Minutes { get; }

        public double Seconds { get; }

        public DD ToDD()
        {
            int sign = Math.Sign(Degrees);
            double DM = Minutes + Seconds / 60;
            double DD = Degrees + sign * DM / 60;
            return new DD(DD);
        }
    }
}
