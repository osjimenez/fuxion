using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Fuxion.Identity.Test.Helpers.TypeDiscriminatorIds;
namespace Fuxion.Identity.Test.Dvo
{
    [TypeDiscriminated(File, AvoidedInclusions = new[] { Media })]
    public abstract class FileDvo<TFile> : BaseDvo<TFile>
        where TFile : FileDvo<TFile>
    { }

    [TypeDiscriminated(Document, AdditionalInclusions = new[] { OfficeDocument }, AvoidedInclusions = new[] { WordDocument, ExcelDocument })]
    public abstract class DocumentDvo<TDocument> : FileDvo<TDocument>
        where TDocument : DocumentDvo<TDocument>
    { }

    [TypeDiscriminated(PdfDocument)]
    public class PdfDocumentDvo : DocumentDvo<PdfDocumentDvo> { }

    [TypeDiscriminated(ExcelDocument, ExplicitExclusions = new[] { OfficeDocument })]
    public class ExcelDocumentDvo : DocumentDvo<ExcelDocumentDvo> { }

    [TypeDiscriminated(WordDocument, ExplicitExclusions = new[] { OfficeDocument })]
    public class WordDocumentDvo : DocumentDvo<WordDocumentDvo> { }

    [TypeDiscriminated(Media, ExplicitExclusions = new[] { Base })]
    public abstract class MediaDvo<TMedia> : FileDvo<TMedia>
        where TMedia : MediaDvo<TMedia>
    { }
    public class FilmDvo : MediaDvo<FilmDvo> { }
    public class SongDvo : MediaDvo<SongDvo>
    {
        public IList<AlbumDvo> Albums { get; set; }
    }
    [TypeDiscriminated(TypeDiscriminationDisableMode.DisableType)]
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
