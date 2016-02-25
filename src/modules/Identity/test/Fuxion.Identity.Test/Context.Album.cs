using Fuxion.Identity.Test.Entity;
using System.Collections.Generic;
using static Fuxion.Identity.Test.Context;
namespace Fuxion.Identity.Test
{
    public class AlbumList : List<Album>
    {
        public AlbumList()
        {
            AddRange(new[] { Album_1 });
        }
        public const string ALBUM_1 = nameof(ALBUM_1);
        public Album Album_1 = new Album
        {
            Id = ALBUM_1,
            Songs = new[] { Songs.Song_1 }
        };
    }
}
