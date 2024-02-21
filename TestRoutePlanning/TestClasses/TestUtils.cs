using GoogleApi.Entities.Maps.Directions.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

namespace TestRoutePlanning.TestClasses
{
    internal static class TestUtils
    {
        public static Route GetDefaultRoute()
        {
            return new Route()
            {
                Legs = new List<Leg>
                {
                    new Leg()
                    {
                        Distance = new GoogleApi.Entities.Maps.Common.Distance()
                        {
                            Value = (int)Length.FromKilometers(100).Meters,
                        }
                    }
                }
            };
        }

        public static FuelEfficiency GetDefaultFuelEfficiency()
        {
            return FuelEfficiency.FromKilometersPerLiters(100);
        }
    }
}
