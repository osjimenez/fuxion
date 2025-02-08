using System.Diagnostics;
using Fuxion.Identity.Test;
using Fuxion.Identity.Test.Dao;
using Fuxion.Identity.Test.Mocks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Identity.DatabaseEFTest;

using static Functions;

public class IdentityManagerTest
{
	public IdentityManagerTest(ITestOutputHelper output)
	{
		Context.Initialize();
		Printer.WriteLineAction = m => {
			Debug.WriteLine(m);
			output.WriteLine(m);
		};
	}
#if DEBUG
	//public const string scenarios = MEMORY;//+"·"+DATABASE;
	//public const string scenarios = DATABASE;
	public const string scenarios = "";
#else
		//public const string scenarios = MEMORY+"·"+DATABASE;
		public const string scenarios = "";
#endif
	IdentityManager? _IdentityManager;
	IdentityManager IM => _IdentityManager ??= new(
		new PasswordProviderMock(),
		new CurrentUserNameProviderMock(() => "root"),
		new IdentityMemoryTestRepository());

	#region Login
	[Theory]
	[InlineData("Username and password null, must fail", scenarios, null, null, false)]
	[InlineData("Password null, must fail", scenarios, "root", null, false)]
	[InlineData("Username null, must fail", scenarios, null, "root", false)]
	[InlineData("Username and password empty, must fail", scenarios, "", "", false)]
	[InlineData("Password empty, must fail", scenarios, "root", "", false)]
	[InlineData("Username empty, must fail", scenarios, "", "root", false)]
	[InlineData("Username and password wrong, must fail", scenarios, "wrong", "wrong", false)]
	[InlineData("Password wrong, must fail", scenarios, "root", "wrong", false)]
	[InlineData("Username wrong, must fail", scenarios, "wrong", "root", false)]
	[InlineData("Must be success", scenarios, "root", "root", true)]
	public void Login(string _, string scenarios, string username, string password, bool expected)
	{
		//foreach (var scenario in scenarios.Split('·'))
		//{
		//	using (Printer.Indent2($"Scenario = { scenario}"))
		//	{
		//		Load(scenario);
		//		var im = Factory.Get<IdentityManager>();
		if (expected)
			Assert.True(IM.CheckCredentials(username, password), $"Login fail unexpected: username<{username}> password<{password}>");
		else
			Assert.False(IM.CheckCredentials(username, password), $"Login success unexpected: username<{username}> password<{password}>");
		//	}
		//}
	}
	#endregion

	#region Check
	[Theory]
	[InlineData("Root can create and delete documents", scenarios, "root", "root", new[] {
		"CREATE", "DELETE"
	}, typeof(DocumentDao), "Verify that 'root' user can 'Create' and 'Delete' entities of type 'Document'", true)]
	//[InlineData(new object[] {
	//    "Root can create and delete orders and invoices", scenarios,
	//    "root", "root",
	//    new[] { "CREATE", "DELETE" },
	//    new[] { typeof(Document), typeof(Circle) },
	//    "Verify that 'root' user can 'Create' and 'Delete' entities of type 'Document' and 'Circle'",
	//    true })]
	[InlineData("Root can create documents", scenarios, "root", "root", new[] {
		"CREATE"
	}, typeof(DocumentDao), "Verify that 'California seller' user can NOT 'Create' entities of type 'Document'", true)]
	public void Check(string _, string scenarios, string username, string password, string[] functionsIds, Type type, string message, bool expected)
	{
		//Assert.True(false, "Require revision after change all context design");
		Printer.WriteLine($"{message}");
		Printer.WriteLine("");
		//foreach (var scenario in scenarios.Split('·'))
		//{
		//	using (Printer.Indent2($"Scenario = { scenario}"))
		//	{
		//		Load(scenario);
		//		var im = Factory.Get<IdentityManager>();
		var functions = functionsIds.Select(id => GetById(id)).ToArray();
		Assert.True(IM.CheckCredentials(username, password), $"Login fail unexpected: username<{username}> password<{password}>");
		using (Printer.Indent("Parameters:"))
		{
			Printer.WriteLine($"Username: {username}");
			Printer.WriteLine($"Functions: {functions.Aggregate("", (a, c) => a + c.Name + "·")}");
			Printer.WriteLine($"Type: {type.Name}");
		}
		var strArgs = $"\r\nscenario<{scenarios}>\r\nusername<{username}>\r\nfunctions<{functions.Aggregate("", (a, c) => a + c.Name + "·")}>\r\ntype<{type.Name}>";
		if (expected)
			Assert.True(IM.GetCurrent()?.Can(functions).Type(type), $"Function assignment failed unexpected: {strArgs}");
		else
			Assert.False(IM.GetCurrent()?.Can(functions).Type(type), $"Function assignment success unexpected: {strArgs}");
		//	}
		//}
	}
	#endregion

	#region Filter
	//[Theory(Skip = "Skipped")]
	//[InlineData(new object[] {
	//    "Two discriminators of same type", scenarios,
	//    "root", "root",
	//    new[] { READ },
	//    typeof(Circle),
	//    new[] { CircleList.CIRCLE_1 },
	//    true,
	//    new string[] { } })]
	//[InlineData(new object[] {
	//    "Two discriminators of distinct type", scenarios,
	//    "root", "root",
	//    new[] { READ },
	//    typeof(Circle),
	//    new[] { CircleList.CIRCLE_1, CircleList.CIRCLE_2 },
	//    true,
	//    new string[] { } })]
	//[InlineData(new object[] {
	//    "One discriminator", scenarios,
	//    "root", "root",
	//    new[] { READ },
	//    typeof(Circle),
	//    new[] { CircleList.CIRCLE_1 },
	//    true,
	//    new string[] { } })]
	void Filter(string _, string scenarios, string username, string password, string[] functionsIds, Type type, string[] expectedIds, bool allowOtherResults, string[] unexpectedIds)
	{
		//Assert.True(false, "Require revision after change all context design");
		//foreach (var scenario in scenarios.Split('·'))
		//{
		//	using (Printer.Indent2($"Scenario = { scenario}"))
		//	{
		//Load(scenario);
		//var im = Factory.Get<IdentityManager>();
		var functions = functionsIds.Select(id => GetById(id)).ToArray();
		//var rep = Factory.Get<IIdentityTestRepository>();
		Assert.True(IM.CheckCredentials(username, password), $"Login fail unexpected: username<{username}> password<{password}>");
		var strArgs = $"\r\nscenario<{scenarios}>\r\nusername<{username}>";
		var dbSet = typeof(IIdentityTestRepository).GetMethod("GetByType")?.MakeGenericMethod(type).Invoke(IM.Repository, null);
		IEnumerable<object>? res = null;
		if (dbSet is IQueryable)
			res = (IQueryable<object>)(typeof(System_Extensions).GetMethods()
				.Where(m => m.Name == "AuthorizedTo" && m.GetParameters().First().ParameterType.GetGenericTypeDefinition() == typeof(IQueryable<>)).First()
				.MakeGenericMethod(type).Invoke(null, new[] {
					dbSet, functions
				}) ?? new List<object>());
		else
			res = (IEnumerable<object>)(typeof(System_Extensions).GetMethods()
				.Where(m => m.Name == "AuthorizedTo" && m.GetParameters().First().ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				.First().MakeGenericMethod(type).Invoke(null, new[] {
					dbSet, functions
				}) ?? new List<object>());
		var list = res.ToList().Cast<BaseDao>();
		if (allowOtherResults)
			Assert.True(list.Any(e => expectedIds.Contains(e.Id)), $"Some expected ids '{expectedIds.Aggregate("", (a, c) => a + c + "·")}' not found");
		else
			Assert.True(list.All(e => expectedIds.Contains(e.Id)), $"Strict expected ids '{expectedIds.Aggregate("", (a, c) => a + c + "·")}' not found");
		//	}
		//}
	}
	#endregion
}