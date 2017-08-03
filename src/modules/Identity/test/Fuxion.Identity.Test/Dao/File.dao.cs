using Fuxion.Identity.Test.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Fuxion.Identity.Test.Helpers.TypeDiscriminatorIds;

namespace Fuxion.Identity.Test.Dao
{
    [Table(nameof(FileDao))]
    public abstract class FileDao : BaseDao { }

    [Table(nameof(DocumentDao))]
    //[TypeDiscriminated(Document, ExplicitInclusions = new[] { OfficeDocument, PdfDocument })]
    [TypeDiscriminated(Document, 
          AddInclusions = new[] { OfficeDocument }, 
        AvoidInclusions = new[] { ExcelDocument, WordDocument })]
    //[TypeDiscriminated(Document)]
    public abstract class DocumentDao : FileDao { }

    [Table(nameof(PdfDocumentDao))]
    [TypeDiscriminated(PdfDocument)]
    public class PdfDocumentDao : DocumentDao { }

    [Table(nameof(ExcelDocumentDao))]
    [TypeDiscriminated(ExcelDocument, ExplicitExclusions = new[] { OfficeDocument })]
    public class ExcelDocumentDao : DocumentDao { }

    [Table(nameof(WordDocumentDao))]
    [TypeDiscriminated(WordDocument, ExplicitExclusions = new[] { OfficeDocument })]
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
