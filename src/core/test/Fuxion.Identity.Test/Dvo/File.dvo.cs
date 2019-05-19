using System.Collections.Generic;
using static Fuxion.Identity.Test.Helpers.TypeDiscriminatorIds;
namespace Fuxion.Identity.Test.Dvo
{
	[TypeDiscriminated(File, AvoidedInclusions = new[] { Media })]
	public abstract class FileDvo<TFile> : BaseDvo<TFile>
		where TFile : FileDvo<TFile>
	{
		public FileDvo(string id, string name) : base(id, name) { }
	}

	[TypeDiscriminated(Document, AdditionalInclusions = new[] { OfficeDocument }, AvoidedInclusions = new[] { WordDocument, ExcelDocument })]
	public abstract class DocumentDvo<TDocument> : FileDvo<TDocument>
		where TDocument : DocumentDvo<TDocument>
	{
		public DocumentDvo(string id, string name) : base(id, name) { }
	}

	[TypeDiscriminated(PdfDocument)]
	public class PdfDocumentDvo : DocumentDvo<PdfDocumentDvo>
	{
		public PdfDocumentDvo(string id, string name) : base(id, name) { }
	}

	[TypeDiscriminated(ExcelDocument, ExplicitExclusions = new[] { OfficeDocument })]
	public class ExcelDocumentDvo : DocumentDvo<ExcelDocumentDvo>
	{
		public ExcelDocumentDvo(string id, string name) : base(id, name) { }
	}

	[TypeDiscriminated(WordDocument, ExplicitExclusions = new[] { OfficeDocument })]
	public class WordDocumentDvo : DocumentDvo<WordDocumentDvo>
	{
		public WordDocumentDvo(string id, string name) : base(id, name) { }
	}

	[TypeDiscriminated(Media, ExplicitExclusions = new[] { Base })]
	public abstract class MediaDvo<TMedia> : FileDvo<TMedia>
		where TMedia : MediaDvo<TMedia>
	{
		public MediaDvo(string id, string name) : base(id, name) { }
	}
	public class FilmDvo : MediaDvo<FilmDvo>
	{
		public FilmDvo(string id, string name) : base(id, name) { }
	}
	public class SongDvo : MediaDvo<SongDvo>
	{
		public SongDvo(string id, string name) : base(id, name) { }
		public IList<AlbumDvo> Albums { get; set; } = new List<AlbumDvo>();
	}
	[TypeDiscriminated(TypeDiscriminationDisableMode.DisableType)]
	public abstract class PackageDvo<TPackage> : FileDvo<TPackage>
		where TPackage : PackageDvo<TPackage>
	{
		public PackageDvo(string id, string name) : base(id, name) { }
	}
	public class AlbumDvo : PackageDvo<AlbumDvo>
	{
		public AlbumDvo(string id, string name) : base(id, name) { }
		public IList<SongDvo> Songs { get; set; } = new List<SongDvo>();
	}
	public class SoftwareDvo : PackageDvo<SoftwareDvo>
	{
		public SoftwareDvo(string id, string name) : base(id, name) { }
	}
}
