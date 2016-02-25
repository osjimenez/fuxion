using Fuxion.Identity.Test.Entity;
using Fuxion.Identity.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Fuxion.Identity.Functions;
using static Fuxion.Identity.Test.Context;
namespace Fuxion.Identity.Test
{
    public class Context
    {
        public static CircleList Circles { get; } = new CircleList();
        public static LocationList Locations { get; } = new LocationList();
        public static GroupsList Groups { get; } = new GroupsList();
        public static IdentityList Identities { get; } = new IdentityList();
        public static SongList Songs { get; } = new SongList();
        public static AlbumList Albums { get; } = new AlbumList();
    }
}
