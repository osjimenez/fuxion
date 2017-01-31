using Fuxion.Identity.Test.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Dao
{
    [Table(nameof(FileDao))]
    public abstract class FileDao : BaseDao { }
    [Table(nameof(DocumentDao))]
    public abstract class DocumentDao : FileDao { }
    [Table(nameof(PdfDocumentDao))]
    public class PdfDocumentDao : DocumentDao { }
    [Table(nameof(ExcelDocumentDao))]
    public class ExcelDocumentDao : DocumentDao { }
    [Table(nameof(WordDocumentDao))]
    [TypeDiscriminated(TypeDiscriminatorIds.WordDocument)]
    public class WordDocumentDao : DocumentDao {
        public CategoryDao Category { get; set; }
        [DiscriminatedBy(typeof(CategoryDao))]
        public string CategoryId { get; set; }
    }
    [Table(nameof(MediaDao))]
    public abstract class MediaDao : FileDao { }
    [Table(nameof(FilmDao))]
    public class FilmDao : MediaDao { }
    [Table(nameof(SongDao))]
    public class SongDao : MediaDao
    {
        public IList<AlbumDao> Albums { get; set; }
    }
    [Table(nameof(PackageDao))]
    [TypeDiscriminated(false)]
    public abstract class PackageDao : FileDao
    {
        public IList<FileDao> Files { get; set; }
    }
    [Table(nameof(AlbumDao))]
    [TypeDiscriminated(true)]
    public class AlbumDao : PackageDao
    {
        public IList<SongDao> Songs { get; set; }
    }
    [Table(nameof(SoftwareDao))]
    //[TypeDiscriminated(true)]
    public class SoftwareDao : PackageDao { }
}
