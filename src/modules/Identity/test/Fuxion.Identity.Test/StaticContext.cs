using Fuxion.Identity.Test.Entity;
using Fuxion.Identity.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Fuxion.Identity.Functions;
using static Fuxion.Identity.Test.StaticContext;
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
    public class DepartmentList : List<Department>
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

        public Department Acme = new Department { Id = "ACME", Name = "ACME" };

        public Department Sales = new Department { Id = "SAL", Name = "Sales" };
        public Department Marketing = new Department { Id = "MARK", Name = "Marketing" };
        public Department Export = new Department { Id = "EXP", Name = "Export" };
        public Department ExportEurope = new Department { Id = "EXP_EU", Name = "European exports" };
        public Department ExportAsia = new Department { Id = "EXP_AS", Name = "Asian exports" };

        public Department Maitenance = new Department { Id = "MAN", Name = "Maitenance" };
        public Department InformationTechnology = new Department { Id = "IT", Name = "Information technology" };
        public Department Electricity = new Department { Id = "EL", Name = "Electricity" };
        public Department Mechanical = new Department { Id = "ME", Name = "Mechanical" };
        public Department Cleaning = new Department { Id = "CLE", Name = "Cleaning" };

        public Department Production = new Department { Id = "PRO", Name = "Production" };
        public Department Cars = new Department { Id = "CAR", Name = "Cars production" };
        public Department Trucks = new Department { Id = "TRUCK", Name = "Trucks production" };

        public Department Finance = new Department { Id = "FIN", Name = "Finance" };

        public Department HR = new Department { Id = "HR", Name = "Human resources" };
    }
    public class GroupsList : List<Group>
    {
        public GroupsList()
        {
            Admins.Permissions = new[] { new Permission {
                Id = "oka",
                Function = Admin,
                Value = true,
                Scopes = new Scope[] { } // When no scopes specified, this permission apply to any entity
            }}.ToList();

            Salesmen.Groups = new[] { Publishers, Exporters }.ToList();
            Exporters.Groups = new[] { EuropeanExporters, AsianExporters }.ToList();

            AddRange(new[] { Admins, Salesmen, Publishers, Exporters, EuropeanExporters, AsianExporters });
        }

        public Group Admins = new Group { Id = "ADMINS", Name = "Administrators" };

        public Group Salesmen = new Group { Id = "SALS", Name = "Salesmen" };
        public Group Publishers = new Group { Id = "PUBS", Name = "Publishers" };
        public Group Exporters = new Group { Id = "EXPS", Name = "Exporters" };
        public Group EuropeanExporters = new Group { Id = "EXPS_EU", Name = "European exporters" };
        public Group AsianExporters = new Group { Id = "EXPS_AS", Name = "Asian exporters" };
    }
    public class IdentityList : List<Entity.Identity>
    {
        public IdentityList()
        {
            IPasswordProvider pp = new PasswordProvider();
            // Root
            Root.Groups = new[] { Groups.Admins }.ToList();
            byte[] salt, hash;
            pp.Generate("root", out salt, out hash);
            Root.PasswordSalt = salt;
            Root.PasswordHash = hash;

            AddRange(new[] { Root, Seller, Publicist, ExportSeller, EuropeanExportSeller });
        }

        public Entity.Identity Root = new Entity.Identity { Id = "ROOT", UserName = "root", Name = "Root" };
        public Entity.Identity Seller = new Entity.Identity { Id = "SELL", UserName = "sell", Name = "Seller" };
        public Entity.Identity Publicist = new Entity.Identity { Id = "PUB", UserName = "pub", Name = "Publisher" };
        public Entity.Identity ExportSeller = new Entity.Identity { Id = "EXP", UserName = "exp", Name = "Exporter" };
        public Entity.Identity EuropeanExportSeller = new Entity.Identity { Id = "EXP_EU", UserName = "exp_eu", Name = "European exporter" };
    }
    public class StaticContext
    {
        public static LocationList Locations { get; } = new LocationList();
        public static DepartmentList Departments { get; } = new DepartmentList();
        public static GroupsList Groups { get; } = new GroupsList();
        public static IdentityList Identities { get; } = new IdentityList();
    }
}
