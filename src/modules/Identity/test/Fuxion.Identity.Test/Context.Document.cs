using Fuxion.Identity.Test.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Fuxion.Identity.Test.Context;
namespace Fuxion.Identity.Test
{
    public class DocumentList : List<Document>
    {
        public DocumentList()
        {
            AddRange(new[] { Doc_1 });
        }
        public const string DOC_1 = nameof(DOC_1);
        public Document Doc_1 = new WordDocument
        {
            Id = DOC_1,
            //CircleId = Circles.Circle_1.Id
        };
    }
}
