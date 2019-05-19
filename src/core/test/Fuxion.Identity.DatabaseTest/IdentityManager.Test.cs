using Fuxion.Identity.Test;
using Fuxion.Identity.Test.Dao;
using Fuxion.Identity.Test.Mocks;
using System.Linq;
using Xunit;
using static Fuxion.Identity.Functions;
namespace Fuxion.Identity.DatabaseTest
{
	public class IdentityManagerTest
	{
		public IdentityManagerTest()
		{
			Context.Initialize();
			//TypeDiscriminator.KnownTypes = AppDomain.CurrentDomain.GetAssemblies()
			//    .Where(a => a.FullName.StartsWith("Fuxion"))
			//    .SelectMany(a => a.DefinedTypes).ToArray();
			var rep = new IdentityDatabaseRepository();
			// Uncomment EnsureDeleted() call to clear database
			//rep.Database.EnsureDeleted();
			if (rep.Database.EnsureCreated() || !rep.Identity.Any())
			{
				var ides = Identities.All;
				var iii = ides.ToArray();
				rep.Identity.AddRange(ides);
				rep.AttachRange(ides);
				rep.SaveChanges();
			}
		}

		private IdentityManager? _IdentityManager;

		private IdentityManager IM
		{
			get
			{
				if (_IdentityManager == null)
					_IdentityManager = new IdentityManager(new PasswordProviderMock(), null!, new IdentityDatabaseRepository());
				return _IdentityManager;
			}
		}
		[Fact(Skip = "Desactivado")]
		public void CheckCredentials()
		{
			// Check when null values
			Assert.False(IM.CheckCredentials(null!, null!));
			Assert.False(IM.CheckCredentials("root", null!));
			Assert.False(IM.CheckCredentials(null!, "root"));
			// Check when empty values
			Assert.False(IM.CheckCredentials("", ""));
			Assert.False(IM.CheckCredentials("root", ""));
			Assert.False(IM.CheckCredentials("", "root"));
			// Check when wrong values
			Assert.False(IM.CheckCredentials("wrong", "root"));
			Assert.False(IM.CheckCredentials("root", "wrong"));
			// Check when success
			Assert.True(IM.CheckCredentials("root", "root"));
		}
		[Fact(Skip = "Desactivado")]
		public void CheckFunctionAssigned()
		{
			var r1 = IM.GetCurrent().Can(Read).Type<DocumentDao>();
			IM.GetCurrent().EnsureCan(Read).Type<DocumentDao>();
			var r2 = IM.GetCurrent().Can(Read).AllInstances(new DocumentDao?[] { null, null, null, null, null, null });
			IM.GetCurrent().EnsureCan(Read).AnyInstances(new DocumentDao?[] { null, null, null, null, null, null });
			// Login
			//if (!IM.IsAuthenticated)
			//    Assert.True(IM.Login("root", "root"));
			//// Check if can create & delete objects of type Entity
			//Assert.True(
			//    IM.Current
			//        .Can(Create, Delete)
			//        .OfType<Order>());
			//// Check if can create & delete objects of types Entity AND DerivedEntity
			//Assert.True(
			//    IM.Current
			//        .Can(Create, Delete)
			//        .OfAllTypes<Order, Invoice>());
			return;
		}
	}
}
