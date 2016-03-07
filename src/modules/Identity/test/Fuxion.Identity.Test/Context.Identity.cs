using Fuxion.Identity.Test.Entity;
using Fuxion.Identity.Test.Helpers;
using System.Collections.Generic;
using static Fuxion.Identity.Functions;
using static Fuxion.Identity.Test.Context;
namespace Fuxion.Identity.Test
{
    public class IdentityList : List<Entity.Identity>
    {
        public IdentityList()
        {
            IPasswordProvider pp = new PasswordProvider();
            // Root
            Root.Groups = new[] { Groups.Admins };
            Root.Permissions = new Permission[] { };
            byte[] salt, hash;
            pp.Generate("root", out salt, out hash);
            Root.PasswordSalt = salt;
            Root.PasswordHash = hash;

            Customer.Groups = new Group[] { };
            Customer.Permissions = new[]
            {
                new Permission
                {
                    Value = true,
                    Function = READ,
                    Rol = Customer,
                    Scopes = new[]
                    {
                        new Scope
                        {
                            Discriminator = Circles.Circle_1,
                            Propagation = ScopePropagation.ToMe
                        }
                    }
                }
            };
            pp.Generate("cus", out salt, out hash);
            Customer.PasswordSalt = salt;
            Customer.PasswordHash = hash;

            AddRange(new[] { Root, Customer });
        }

        public Entity.Identity Root = new Entity.Identity { Id = "ROOT", UserName = "root", Name = "Root" };
        public Entity.Identity Customer = new Entity.Identity { Id = "CUS", UserName = "cus", Name = "Customer" };
    }
}
