using Fuxion.Identity.Test.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test
{
    public class SongList : List<Song>
    {
        public SongList()
        {
            AddRange(new[] { Song_1, Song_2 });
        }
        public const string SONG_1 = nameof(SONG_1);
        public Song Song_1 = new Song
        {
            Id = SONG_1,
        }; public const string SONG_2 = nameof(SONG_2);
        public Song Song_2 = new Song
        {
            Id = SONG_2,
        };
    }
}
