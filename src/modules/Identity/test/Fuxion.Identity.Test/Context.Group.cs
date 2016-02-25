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
            Admins.Groups = new Group[] { };
            Admins.Permissions = new[] { new Permission {
                Id = Guid.NewGuid().ToString("N"),
                Function = Admin.Id.ToString(),
                Value = true,
                Scopes = new Scope[] { } // When no scopes specified, this permission apply to any discriminator
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
                        Discriminator = Circles.Circle_1,
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
                        Discriminator = Circles.Circle_1,
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
            AddRange(new[] { Admins, CaliforniaSellers, NewYorkSellers });
        }
        public Group Admins = new Group { Id = "ADMINS", Name = "Administrators" };
        public Group CaliforniaSellers = new Group { Id = "CA_SELLS", Name = "California sellers" };
        public Group NewYorkSellers = new Group { Id = "NY_SELLS", Name = "New York sellers" };
    }
}
