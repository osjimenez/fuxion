using Fuxion.Identity.Test.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Fuxion.Identity.Functions;
using static Fuxion.Identity.Test.Context;
namespace Fuxion.Identity.Test
{
    public class InvoiceList : List<Invoice>
    {
        public InvoiceList()
        {
            AddRange(new[] { SAL_Invoice });
        }
        public const string SAL_INVOICE = nameof(SAL_INVOICE);
        public Invoice SAL_Invoice = new Invoice
        {
            Id = SAL_INVOICE,
            Department = Departments.Sales,
            DepartmentId = Departments.Sales.Id,
        };
    }
}
