using Fuxion.Identity.Test.Entity;
using System;
using System.Collections.Generic;
namespace Fuxion.Identity.Test
{
    public class LocationList : List<Location>
    {
        public LocationList()
        {
            USA.States = new[] { California, NewYork };

            California.Country = USA;
            California.Cities = new[] { SanFrancisco, LosAngeles };
            SanFrancisco.State = California;
            LosAngeles.State = California;

            NewYork.Country = USA;
            NewYork.Cities = new[] { NewYorkCity };
            NewYorkCity.State = NewYork;

            Spain.States = new[] { Madrid };

            Madrid.Country = Spain;
            Madrid.Cities = new[] { MadridCity };
            MadridCity.State = Madrid;

            AddRange(new Location[] { USA, California, SanFrancisco, LosAngeles, NewYork, NewYorkCity, Spain, Madrid, MadridCity });
        }
        public Country USA = new Country { Id = "US", Name = "USA" };

        public State California = new State { Id = "CA", Name = "California" };
        public City SanFrancisco = new City { Id = "SF", Name = "San Francisco" };
        public City LosAngeles = new City { Id = "LA", Name = "Los Angeles" };

        public State NewYork = new State { Id = "NY", Name = "New york" };
        public City NewYorkCity = new City { Id = "NYC", Name = "New York City" };

        public Country Spain = new Country { Id = "ES", Name = "Spain" };
        public State Madrid = new State { Id = "MAD", Name = "Madrid community" };
        public City MadridCity = new City { Id = "M", Name = "Madrid" };
    }
}
