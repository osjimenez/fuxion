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

            AddRange(new[] { Root });
        }

        public Entity.Identity Root = new Entity.Identity { Id = "ROOT", UserName = "root", Name = "Root" };
    }
}
