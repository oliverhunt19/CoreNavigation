using GoogleApi;
using GoogleApi.Entities.Maps.Directions.Response;

namespace RoutePlanning.RouteOptimisation
{
    public class RouteOptimisation<T> where T : ILocation
    {
        private RouteOptimisationTest optimisationTest;
        public RouteOptimisationTest OptimisationTest => optimisationTest;
        public IReadOnlyList<RouteOptimisationTrialResult>? Results { get; private set; }

        private IReadOnlyList<RouteOptimisationTrial>? optimisationTrials;
        public IReadOnlyList<RouteOptimisationTrial>? RouteOptimisationTrials => optimisationTrials;

        public RouteOptimisation(RouteOptimisationTest optimisationTest)
        {
            this.optimisationTest = optimisationTest;
        }


        public async Task<RouteOptimisationResult> RunOptimisation(RouteOptimisationRequest<T> optimisationRequest, CancellationToken cancellationToken = default)
        {
            
            List<Task<RouteOptimisationTrial>> trials = new List<Task<RouteOptimisationTrial>>();
            foreach (T ilocation in optimisationRequest.Route.Places)
            {
                trials.Add(RouteOptimisationTrial.RunTrial(optimisationRequest, ilocation));
            }

            optimisationTrials = (await Task.WhenAll(trials).WaitAsync(cancellationToken).ConfigureAwait(false)).ToList();

            return await RunTest();
        }

        public async Task<RouteOptimisationResult> RunTest(RouteOptimisationTest? test = null, CancellationToken cancellationToken = default)
        {
            if (test != null)
            {
                optimisationTest = test;
            }
            if (optimisationTrials == null)
            {
                throw new ArgumentNullException("OptimisationTrials", "There are no trial routes to check the tests on");
            }
            IEnumerable<Task<RouteOptimisationTrialResult>> testTasks = optimisationTrials.Select(x => OptimisationTest.GetCost(x));
            List<RouteOptimisationTrialResult> routeOptimisationResults = (await Task.WhenAll(testTasks).WaitAsync(cancellationToken).ConfigureAwait(false)).ToList();

            List<RouteOptimisationTrialResult> routeOptimisationResultsOrdered = routeOptimisationResults.OrderByDescending(x => x.Cost).Reverse().ToList();

            Results = routeOptimisationResultsOrdered;

            return new RouteOptimisationResult(routeOptimisationResultsOrdered);
        }

        /// <summary>
        /// This is to house the test the optimisation will run to quanitfy which route is best
        /// </summary>
        public abstract class RouteOptimisationTest
        {
            protected abstract Task<double> GetCostInternal(RouteOptimisationTrial value);

            public async Task< RouteOptimisationTrialResult> GetCost(RouteOptimisationTrial value)
            {
                double _cost = await GetCostInternal(value).ConfigureAwait(false);
                RouteOptimisationTrialResult result = new RouteOptimisationTrialResult(_cost, value.Route, value.Location);
                return result;
            }
        }

        /// <summary>
        /// This is a result for the trial of the test 
        /// </summary>
        public class RouteOptimisationTrialResult : IGetRoute
        {
            public RouteOptimisationTrialResult(double cost, Route route, T location)
            {
                Cost = cost;
                Route = new RouteInfo(route);
                Location = location;
            }

            public RouteOptimisationTrialResult(double cost, RouteInfo route, T location)
            {
                Cost = cost;
                Route = route;
                Location = location;
            }

            public double Cost { get; }
            public RouteInfo Route { get; }



            public T Location { get; }

            public Route GetRoute()
            {
                return Route.Route;
            }
        }

        /// <summary>
        /// This is the outputed result that also gives other recomendations
        /// </summary>
        public class RouteOptimisationResult : RouteOptimisationTrialResult
        {
            const int MaxCount = 20;
            public RouteOptimisationResult(IReadOnlyList<RouteOptimisation<T>.RouteOptimisationTrialResult> alternatives) : base(alternatives.First().Cost, alternatives.First().Route, alternatives.First().Location)
            {
                Alternatives = alternatives.Take(new Range(1, MaxCount)).ToList();
                Results = alternatives.Take(new Range(0, MaxCount)).ToList();
            }

            public IReadOnlyList<RouteOptimisationTrialResult> Alternatives { get; }

            public IReadOnlyList<RouteOptimisationTrialResult> Results { get; }
        }

        public class RouteOptimisationTrial
        {
            private RouteOptimisationTrial(T location, Route route)
            {
                Location = location;
                Route = route;
            }

            public T Location { get; }

            public Route Route { get; }

            public static async Task<RouteOptimisationTrial> RunTrial(RouteOptimisationRequest<T> directionsRequest, T ilocation)
            {
                DirectionsResponse response = await GoogleMaps.Directions.QueryAsync(directionsRequest.GetDirectionsRequest(ilocation)).ConfigureAwait(false);
                return new RouteOptimisationTrial(ilocation, response.Routes.First());
            }
        }
    }


}
