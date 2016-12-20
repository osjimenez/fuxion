using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [Table(nameof(Film))]
    public class Film : Media
    {
        //public IList<Actor> Actors { get; set; }
        //public IList<Screenwriter> Screenwriters { get; set; }
        //public FilmDirector Director { get; set; }
    }
}
