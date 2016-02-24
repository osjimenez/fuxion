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
    public class SellOrderList : List<SellOrder>
    {
        public SellOrderList()
        {
            AddRange(new[] { CA_SellOrder, NY_SellOrder, MAD_SellOrder });
        }
        public SellOrder CA_SellOrder = new SellOrder
        {
            Id = "CA_SellOrder",
            Department = Departments.Sales,
            DepartmentId = Departments.Sales.Id,
            ShipmentCity = Locations.SanFrancisco,
            ShipmentCityId = Locations.SanFrancisco.Id,
            ReceptionCity = Locations.LosAngeles,
            ReceptionCityId = Locations.LosAngeles.Id,
            Seller = Identities.CaliforniaSeller,
            SellerId = Identities.CaliforniaSeller.Id
        };
        public SellOrder NY_SellOrder = new SellOrder
        {
            Id = "NY_SellOrder",
            Department = Departments.Sales,
            DepartmentId = Departments.Sales.Id,
            ShipmentCity = Locations.NewYorkCity,
            ShipmentCityId = Locations.NewYorkCity.Id,
            ReceptionCity = Locations.SanFrancisco,
            ReceptionCityId = Locations.SanFrancisco.Id,
            Seller = Identities.NewYorkSeller,
            SellerId = Identities.NewYorkSeller.Id
        };
        public SellOrder MAD_SellOrder = new SellOrder
        {
            Id = "MAD_SellOrder",
            Department = Departments.Sales,
            DepartmentId = Departments.Sales.Id,
            ShipmentCity = Locations.NewYorkCity,
            ShipmentCityId = Locations.NewYorkCity.Id,
            ReceptionCity = Locations.MadridCity,
            ReceptionCityId = Locations.MadridCity.Id,
            Seller = Identities.NewYorkSeller,
            SellerId = Identities.NewYorkSeller.Id
        };
    }
}
