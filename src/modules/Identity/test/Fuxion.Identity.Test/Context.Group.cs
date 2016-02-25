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
            
            AddRange(new[] { Admins });
        }
        public Group Admins = new Group { Id = "ADMINS", Name = "Administrators" };
    }
}
