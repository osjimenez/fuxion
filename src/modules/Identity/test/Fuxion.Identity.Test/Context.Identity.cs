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

            // California seller
            CaliforniaSeller.Groups = new[] { Groups.CaliforniaSellers };
            CaliforniaSeller.Permissions = new Permission[] { };
            pp.Generate("ca_sell", out salt, out hash);
            CaliforniaSeller.PasswordSalt = salt;
            CaliforniaSeller.PasswordHash = hash;

            // California seller
            NewYorkSeller.Groups = new[] { Groups.CaliforniaSellers };
            pp.Generate("ny_sell", out salt, out hash);
            NewYorkSeller.PasswordSalt = salt;
            NewYorkSeller.PasswordHash = hash;

            AddRange(new[] { Root, CaliforniaSeller, NewYorkSeller });
        }

        public Entity.Identity Root = new Entity.Identity { Id = "ROOT", UserName = "root", Name = "Root" };
        public Seller CaliforniaSeller = new Seller { Id = "CA_SELL", UserName = "ca_sell", Name = "California seller" };
        public Seller NewYorkSeller = new Seller { Id = "NY_SELL", UserName = "ny_sell", Name = "New York seller" };
    }
}
