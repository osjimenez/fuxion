using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Dvo
{
    public abstract class FileDvo<TFile> : BaseDvo<TFile>
        where TFile : FileDvo<TFile>
    { }
    public abstract class DocumentDvo<TDocument> : FileDvo<TDocument>
        where TDocument : DocumentDvo<TDocument>
    { }
    public class PdfDocumentDvo : DocumentDvo<PdfDocumentDvo> { }
    public class ExcelDocumentDvo : DocumentDvo<ExcelDocumentDvo> { }
    public class WordDocumentDvo : DocumentDvo<WordDocumentDvo> { }
    public abstract class MediaDvo<TMedia> : FileDvo<TMedia>
        where TMedia : MediaDvo<TMedia>
    { }
    public class FilmDvo : MediaDvo<FilmDvo> { }
    public class SongDvo : MediaDvo<SongDvo>
    {
        public IList<AlbumDvo> Albums { get; set; }
    }
    [TypeDiscriminated(TypeDiscriminationMode.DisableType)]
    public abstract class PackageDvo<TPackage> : FileDvo<TPackage>
        where TPackage : PackageDvo<TPackage>
    {
    }
    public class AlbumDvo : PackageDvo<AlbumDvo>
    {
        public IList<SongDvo> Songs { get; set; }
    }
    public class SoftwareDvo : PackageDvo<SoftwareDvo> { }
}
