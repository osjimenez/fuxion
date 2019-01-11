using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Helpers
{
    public static class TypeDiscriminatorIds
    {
        public const string Base = nameof(Base);

        public const string Location = nameof(Location);
        public const string Country = nameof(Country);
        public const string State = nameof(State);
        public const string City = nameof(City);

        public const string Category = nameof(Category);

        public const string Person = nameof(Person);
        public const string Rol = nameof(Rol);
        public const string Identity = nameof(Identity);
        public const string Group = nameof(Group);

        public const string File = nameof(File);

        public const string Media = nameof(Media);

        public const string Document = nameof(Document);
        public const string PdfDocument = nameof(PdfDocument);
        public const string OfficeDocument = nameof(OfficeDocument);
        public const string WordDocument = nameof(WordDocument);
        public const string ExcelDocument = nameof(ExcelDocument);
    }
}
