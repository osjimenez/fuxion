using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [Table(nameof(Song))]
    public class Song : Media {
        public IList<Album> Albums { get; set; }
        public IList<Circle> ShareWith { get; set; }
    }
}
