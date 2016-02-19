using Fuxion.Identity.Test.Entity;
using Fuxion.Identity.Test.Helpers;
using Fuxion.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Fuxion.Identity.Functions;
using System.Collections;
using Fuxion.Identity.DatabaseTest.Entity;

namespace Fuxion.Identity.Test
{
    class LocationList : List<Location>
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
        internal Country USA = new Country { Id = "US", Name = "USA" };

        internal State California = new State { Id = "CA", Name = "California" };
        internal City SanFrancisco = new City { Id = "SF", Name = "San Francisco" };
        internal City LosAngeles = new City { Id = "LA", Name = "Los Angeles" };

        internal State NewYork = new State { Id = "NY", Name = "New york" };
        internal City NewYorkCity = new City { Id = "NYC", Name = "New York City" };

        internal Country Spain = new Country { Id = "ES", Name = "Spain" };
        internal State Madrid = new State { Id = "MAD", Name = "Madrid community" };
        internal City MadridCity = new City { Id = "M", Name = "Madrid" };
    }
    class DepartmentList : List<Department>
    {
        public DepartmentList()
        {
            Acme.Children = new[] { Sales, Maitenance, Production, Finance, HR };

            Sales.Parent = Acme;
            Sales.Children = new[] { Export, Marketing };
            Export.Parent = Sales;
            Export.Children = new[] { ExportEurope, ExportAsia };
            ExportAsia.Parent = Export;
            ExportEurope.Parent = Export;
            Marketing.Parent = Sales;

            Maitenance.Parent = Acme;
            Maitenance.Children = new[] { InformationTechnology, Electricity, Mechanical, Cleaning };
            InformationTechnology.Parent = Maitenance;
            Electricity.Parent = Maitenance;
            Mechanical.Parent = Maitenance;
            Cleaning.Parent = Maitenance;

            Production.Parent = Acme;
            Production.Children = new[] { Cars, Trucks };
            Cars.Parent = Production;
            Trucks.Parent = Production;

            Finance.Parent = Acme;

            HR.Parent = Acme;

            AddRange(new[] { Acme, Sales, Export, ExportEurope, ExportAsia, Marketing, Maitenance, InformationTechnology, Electricity, Mechanical, Cleaning, Production, Cars, Trucks, Finance, HR });
        }

        internal Department Acme = new Department { Id = "ACME", Name = "ACME" };

        internal Department Sales = new Department { Id = "SAL", Name = "Sales" };
        internal Department Marketing = new Department { Id = "MARK", Name = "Marketing" };
        internal Department Export = new Department { Id = "EXP", Name = "Export" };
        internal Department ExportEurope = new Department { Id = "EXP_EU", Name = "European exports" };
        internal Department ExportAsia = new Department { Id = "EXP_AS", Name = "Asian exports" };

        internal Department Maitenance = new Department { Id = "MAN", Name = "Maitenance" };
        internal Department InformationTechnology = new Department { Id = "IT", Name = "Information technology" };
        internal Department Electricity = new Department { Id = "EL", Name = "Electricity" };
        internal Department Mechanical = new Department { Id = "ME", Name = "Mechanical" };
        internal Department Cleaning = new Department { Id = "CLE", Name = "Cleaning" };

        internal Department Production = new Department { Id = "PRO", Name = "Production" };
        internal Department Cars = new Department { Id = "CAR", Name = "Cars production" };
        internal Department Trucks = new Department { Id = "TRUCK", Name = "Trucks production" };

        internal Department Finance = new Department { Id = "FIN", Name = "Finance" };

        internal Department HR = new Department { Id = "HR", Name = "Human resources" };
    }
    class GroupsList : List<Group>
    {
        public GroupsList()
        {
            Admins.Permissions = new[] { new Permission {
                Function = Admin,
                Value = true,
                Scopes = new IScope[] { } // When no scopes specified, this permission apply to any entity
            }};

            Salesmen.Groups = new[] { Publishers, Exporters };
            Exporters.Groups = new[] { EuropeanExporters, AsianExporters };

            AddRange(new[] { Admins, Salesmen, Publishers, Exporters, EuropeanExporters, AsianExporters });
        }

        internal Group Admins = new Group { Id = "ADMINS", Name = "Administrators" };

        internal Group Salesmen = new Group { Id = "SALS", Name = "Salesmen" };
        internal Group Publishers = new Group { Id = "PUBS", Name = "Publishers" };
        internal Group Exporters = new Group { Id = "EXPS", Name = "Exporters" };
        internal Group EuropeanExporters = new Group { Id = "EXPS_EU", Name = "European exporters" };
        internal Group AsianExporters = new Group { Id = "EXPS_AS", Name = "Asian exporters" };
    }
    class IdentityList : List<DatabaseTest.Entity.Identity>
    {
        public IdentityList()
        {
            IPasswordProvider pp = new PasswordProvider();
            // Root
            Root.Groups = new[] { IdentityMemoryRepository.Groups.Admins };
            byte[] salt, hash;
            pp.Generate("root", out salt, out hash);
            Root.PasswordSalt = salt;
            Root.PasswordHash = hash;

            AddRange(new[] { Root, Seller, Publicist, ExportSeller, EuropeanExportSeller });
        }

        internal DatabaseTest.Entity.Identity Root = new DatabaseTest.Entity.Identity { Id = "ROOT", UserName = "root", Name = "Root" };
        internal DatabaseTest.Entity.Identity Seller = new DatabaseTest.Entity.Identity { Id = "SELL", UserName = "root", Name = "Root" };
        internal DatabaseTest.Entity.Identity Publicist = new DatabaseTest.Entity.Identity { Id = "PUB", UserName = "root", Name = "Root" };
        internal DatabaseTest.Entity.Identity ExportSeller = new DatabaseTest.Entity.Identity { Id = "SELL_EXP", UserName = "root", Name = "Root" };
        internal DatabaseTest.Entity.Identity EuropeanExportSeller = new DatabaseTest.Entity.Identity { Id = "SELL_EXP_EU", UserName = "root", Name = "Root" };
    }
    class IdentityMemoryRepository : IKeyValueRepository<IdentityKeyValueRepositoryValue, string, IIdentity>
    {
        #region Lists
        internal static LocationList Locations { get; } = new LocationList();
        internal static DepartmentList Departments { get; } = new DepartmentList();
        internal static GroupsList Groups { get; } = new GroupsList();
        internal static IdentityList Identities { get; } = new IdentityList();
        #endregion
        public bool Exist(string key) { return false; }
        public Task<bool> ExistAsync(string key) { return Task.FromResult(false); }
        public IIdentity Find(string key) { return Identities.FirstOrDefault(i => i.UserName == key); }
        public Task<IIdentity> FindAsync(string key) { return Task.FromResult(Find(key)); }
        public IIdentity Get(string key) { return null; }
        public Task<IIdentity> GetAsync(string key) { return Task.FromResult<IIdentity>(null); }
        public void Remove(string key) { }
        public Task RemoveAsync(string key) { return Task.CompletedTask; }
        public void Set(string key, IIdentity value) { }
        public Task SetAsync(string key, IIdentity value) { return Task.CompletedTask; }
    }
}
