using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [Table(nameof(File))]
    public abstract class File : Base { }
    [Table(nameof(Document))]
    public abstract class Document : File { }
    [Table(nameof(PdfDocument))]
    public class PdfDocument : Document { }
    [Table(nameof(ExcelDocument))]
    public class ExcelDocument : Document { }
    [Table(nameof(WordDocument))]
    public class WordDocument : Document { }
    [Table(nameof(Media))]
    public abstract class Media : File { }
    [Table(nameof(Film))]
    public class Film : Media { }
    [Table(nameof(Song))]
    public class Song : Media
    {
        public IList<Album> Albums { get; set; }
    }
    [Table(nameof(Package))]
    public abstract class Package : File
    {
        public IList<File> Files { get; set; }
    }
    [Table(nameof(Album))]
    public class Album : Package
    {
        public IList<Song> Songs { get; set; }
    }
    [Table(nameof(Software))]
    public class Software : Package { }
}
