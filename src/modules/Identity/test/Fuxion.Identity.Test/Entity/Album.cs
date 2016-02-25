using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [Table(nameof(Album))]
    public class Album : FilePackage
    {
        public IList<Song> Songs { get; set; }
        public IList<Circle> ShareWith { get; set; }
    }
}
