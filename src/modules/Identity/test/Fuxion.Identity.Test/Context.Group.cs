using Fuxion.Identity.Test.Entity;
using System;
using System.Collections.Generic;
using static Fuxion.Identity.Functions;
using static Fuxion.Identity.Test.Context;
namespace Fuxion.Identity.Test
{
    public class GroupsList : List<Group>
    {
        public GroupsList()
        {
            // Admins
            Admins.Permissions = new[] { new Permission {
                Id = Guid.NewGuid().ToString("N"),
                Function = Admin.Id.ToString(),
                Value = true,
                Scopes = new Scope[] { } // When no scopes specified, this permission apply to any entity
            }};

            // California sellers
            CaliforniaSellers.Groups = new Group[] { };
            CaliforniaSellers.Permissions = new[] {new Permission
            {
                Id = Guid.NewGuid().ToString("N"),
                Function = Create.Id.ToString(),
                Value = true,
                Scopes = new Scope[]
                {
                    new Scope
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        Discriminator = Departments.Sales,
                        Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions
                    },
                    new Scope
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        Discriminator = Locations.California,
                        Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions
                    }
                }
            } };

            // New York sellers
            NewYorkSellers.Groups = new Group[] { };
            NewYorkSellers.Permissions = new[] {new Permission
            {
                Id = Guid.NewGuid().ToString("N"),
                Function = Create.Id.ToString(),
                Value = true,
                Scopes = new Scope[]
                {
                    new Scope
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        Discriminator = Departments.Sales,
                        Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions
                    },
                    new Scope
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        Discriminator = Locations.NewYork,
                        Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions
                    }
                }
            } };


            //Salesmen.Groups = new[] { Publishers, Exporters }.ToList();
            //Exporters.Groups = new[] { EuropeanExporters, AsianExporters }.ToList();

            //AddRange(new[] { Admins, Salesmen, Publishers, Exporters, EuropeanExporters, AsianExporters });
            AddRange(new[] { Admins, CaliforniaSellers, NewYorkSellers });
        }

        public Group Admins = new Group { Id = "ADMINS", Name = "Administrators" };

        public Group CaliforniaSellers = new Group { Id = "CA_SELLS", Name = "California sellers" };
        public Group NewYorkSellers = new Group { Id = "NY_SELLS", Name = "New York sellers" };

        //public Group Salesmen = new Group { Id = "SALS", Name = "Salesmen" };
        //public Group Publishers = new Group { Id = "PUBS", Name = "Publishers" };
        //public Group Exporters = new Group { Id = "EXPS", Name = "Exporters" };
        //public Group EuropeanExporters = new Group { Id = "EXPS_EU", Name = "European exporters" };
        //public Group AsianExporters = new Group { Id = "EXPS_AS", Name = "Asian exporters" };
    }
}
