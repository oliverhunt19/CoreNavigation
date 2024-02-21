using GoogleApi.Entities.Maps.Directions.Response;
using RoutePlanning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestRoutePlanning.TestClasses;
using UnitsNet;

namespace TestRoutePlanning
{
    [TestClass]
    public class RouteInfoTests
    {
        [TestMethod]
        public void FindRouteCost_ShouldCalculateCorrectCost()
        {
            // Arrange
            Route route = TestUtils.GetDefaultRoute();
            RouteInfo routeInfo = new RouteInfo(route);
            FuelEfficiency fuelEfficiency = TestUtils.GetDefaultFuelEfficiency();
            CostPerUnit<Volume> fuelCostPerLitre = new CostPerUnit<Volume>(GBP.FromPound(1), Volume.FromLiters(1));

            // Act
            GBP cost = routeInfo.FindRouteCost(fuelEfficiency, fuelCostPerLitre);

            // Assert
            // You should have an expected result based on your input values
            // Replace the expectedValue with the actual expected result
            GBP expectedValue = GBP.FromPound(1);
            Assert.AreEqual(expectedValue, cost, "The calculated cost does not match the expected value.");
        }

        // Add more test methods to cover other functionalities of your RouteInfo class
    }
}
