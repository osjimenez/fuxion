using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [Table(nameof(FilePackage))]
    public abstract class FilePackage : File
    {
        public IList<File> Files { get; set; }
    }
}
