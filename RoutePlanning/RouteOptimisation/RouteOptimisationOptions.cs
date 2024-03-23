namespace RoutePlanning.RouteOptimisation
{
    public class RouteOptimisationOptions
    {
        public int LimitLocationChecks { get; set; }

        public bool SortCosts { get; set; }  

        public RouteOptimisationOptions()
        {
            SortCosts = true;
            LimitLocationChecks = int.MaxValue;
        }

        public static RouteOptimisationOptions Default => new RouteOptimisationOptions();
    }
}
