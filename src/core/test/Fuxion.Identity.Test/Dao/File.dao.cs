using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using static Fuxion.Identity.Test.Helpers.TypeDiscriminatorIds;

namespace Fuxion.Identity.Test.Dao
{
	[Table(nameof(FileDao))]
	[TypeDiscriminated(Helpers.TypeDiscriminatorIds.File, AvoidedInclusions = new[] { Media })]
	public abstract class FileDao : BaseDao
	{
		public FileDao(string id, string name) : base(id, name) { }
	}

	[Table(nameof(DocumentDao))]
	[TypeDiscriminated(Document, AdditionalInclusions = new[] { OfficeDocument }, AvoidedInclusions = new[] { WordDocument, ExcelDocument })]
	public abstract class DocumentDao : FileDao
	{
		public DocumentDao(string id, string name) : base(id, name) { }
	}

	[Table(nameof(PdfDocumentDao))]
	[TypeDiscriminated(PdfDocument)]
	public class PdfDocumentDao : DocumentDao
	{
		public PdfDocumentDao(string id, string name) : base(id, name) { }
	}

	[Table(nameof(ExcelDocumentDao))]
	[TypeDiscriminated(ExcelDocument, ExplicitExclusions = new[] { OfficeDocument })]
	public class ExcelDocumentDao : DocumentDao
	{
		public ExcelDocumentDao(string id, string name) : base(id, name) { }
	}

	[Table(nameof(WordDocumentDao))]
	[TypeDiscriminated(WordDocument, ExplicitExclusions = new[] { OfficeDocument })]
	public class WordDocumentDao : DocumentDao
	{
		public WordDocumentDao(string id, string name) : base(id, name)
		{
		}
		public CategoryDao? Category { get; set; }
		[DiscriminatedBy(typeof(CategoryDao))]
		public string? CategoryId { get; set; }
	}

	[Table(nameof(MediaDao))]
	[TypeDiscriminated(Media, ExplicitExclusions = new[] { Base })]
	public abstract class MediaDao : FileDao
	{
		public MediaDao(string id, string name) : base(id, name) { }
	}

	[Table(nameof(FilmDao))]
	public class FilmDao : MediaDao
	{
		public FilmDao(string id, string name) : base(id, name) { }
	}

	[Table(nameof(SongDao))]
	public class SongDao : MediaDao
	{
		public SongDao(string id, string name) : base(id, name) { }
		public IList<AlbumDao> Albums { get; set; } = new List<AlbumDao>();
	}

	[Table(nameof(PackageDao))]
	[TypeDiscriminated(TypeDiscriminationDisableMode.DisableType)]
	public class PackageDao : FileDao
	{
		public PackageDao(string id, string name) : base(id, name) { }
		public IList<FileDao> Files { get; set; } = new List<FileDao>();
	}

	[Table(nameof(AlbumDao))]
	public class AlbumDao : PackageDao
	{
		public AlbumDao(string id, string name) : base(id, name) { }
		public IList<SongDao> Songs { get; set; } = new List<SongDao>();
	}

	[Table(nameof(SoftwareDao))]
	public class SoftwareDao : PackageDao
	{
		public SoftwareDao(string id, string name) : base(id, name) { }
	}
}
