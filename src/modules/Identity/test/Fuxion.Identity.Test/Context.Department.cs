using Fuxion.Identity.Test.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test
{
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
            ExportAsia.Children = new Department[] { };
            ExportEurope.Parent = Export;
            ExportEurope.Children = new Department[] { };
            Marketing.Parent = Sales;
            Marketing.Children = new Department[] { };

            Maitenance.Parent = Acme;
            Maitenance.Children = new[] { InformationTechnology, Electricity, Mechanical, Cleaning };
            InformationTechnology.Parent = Maitenance;
            InformationTechnology.Children = new Department[] { };
            Electricity.Parent = Maitenance;
            Electricity.Children = new Department[] { };
            Mechanical.Parent = Maitenance;
            Mechanical.Children = new Department[] { };
            Cleaning.Parent = Maitenance;
            Cleaning.Children = new Department[] { };

            Production.Parent = Acme;
            Production.Children = new[] { Cars, Trucks };
            Cars.Parent = Production;
            Cars.Children = new Department[] { };
            Trucks.Parent = Production;
            Trucks.Children = new Department[] { };

            Finance.Parent = Acme;
            Finance.Children = new Department[] { };

            HR.Parent = Acme;
            HR.Children = new Department[] { };

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
}
