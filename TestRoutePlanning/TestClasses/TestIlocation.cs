using GoogleApi.Entities.Common;
using RoutePlanning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRoutePlanning.TestClasses
{
    internal class TestIlocation : ILocation
    {

        public TestIlocation(LatLng coordinates, Address address)
        {
            Coordinates = coordinates;
            Address = address;
        }

        public TestIlocation(LatLng coordinates) : this(coordinates,new Address(""))
        {

        }

        public LatLng Coordinates { get; }

        public Address Address { get; }

        public Address GetAddress()
        {
            return Address;
        }
    }
}
