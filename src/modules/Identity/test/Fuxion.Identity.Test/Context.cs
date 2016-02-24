using Fuxion.Identity.Test.Entity;
using Fuxion.Identity.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Fuxion.Identity.Functions;
using static Fuxion.Identity.Test.Context;
namespace Fuxion.Identity.Test
{
    public class DemoList : List<Demo>
    {
        public DemoList()
        {
            AddRange(new[] { Demo1 });
        }
        public const string DEMO1 = nameof(DEMO1);
        public Demo Demo1 = new Demo
        {
            Id = DEMO1,
            ReceptionCity = Locations.SanFrancisco,
            ReceptionCityId = Locations.SanFrancisco.Id,
            ShipmentCity = Locations.LosAngeles,
            ShipmentCityId = Locations.LosAngeles.Id
        };
    }
    public class Context
    {
        public static LocationList Locations { get; } = new LocationList();
        public static DepartmentList Departments { get; } = new DepartmentList();
        public static GroupsList Groups { get; } = new GroupsList();
        public static IdentityList Identities { get; } = new IdentityList();
        public static SellOrderList SellOrders { get; } = new SellOrderList();
        public static InvoiceList Invoices { get; } = new InvoiceList();
        public static DemoList Demos { get; } = new DemoList();
    }
}
