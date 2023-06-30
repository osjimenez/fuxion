using System.Runtime.CompilerServices;

namespace Fuxion.Test.Reflection;

public class UriTest(ITestOutputHelper output) : BaseTest<TypeKeyTest>(output)
{
	void PrintVariable(string value, [CallerArgumentExpression(nameof(value))] string? name = null) 
		=> Output.WriteLine($"{name}: {value}");
	[Fact(DisplayName = "Uri1")]
	public void Uri1()
	{
		void Print(Uri uri)
		{
			Output.WriteLine("======== " + uri);
			if (uri.IsAbsoluteUri)
			{
				PrintVariable(uri.Authority);
				PrintVariable(uri.AbsoluteUri);
				PrintVariable(uri.AbsolutePath);
				PrintVariable(uri.Fragment);
				PrintVariable(uri.Host);
				PrintVariable(uri.Query);
				PrintVariable(uri.Scheme);
				PrintVariable(uri.IdnHost);
				PrintVariable(uri.LocalPath);
				PrintVariable(uri.UserInfo);
				PrintVariable(uri.DnsSafeHost);
				PrintVariable(uri.PathAndQuery);
				PrintVariable(uri.Port.ToString());
				PrintVariable(uri.IsFile.ToString());
				PrintVariable(uri.IsLoopback.ToString());
				PrintVariable(uri.IsUnc.ToString());
				PrintVariable(uri.HostNameType.ToString());
				PrintVariable(uri.IsDefaultPort.ToString());
				Output.WriteLine("Segments:");
				foreach (var seg in uri.Segments) Output.WriteLine("\t" + seg);
			}
			PrintVariable(uri.OriginalString);
			PrintVariable(uri.UserEscaped.ToString());
			PrintVariable(uri.IsAbsoluteUri.ToString());
		}
		Print(new("https://userInfo@fuxion.dev/one/two/three/?query=query#fragment"));
		Print(new("https://fuxion.dev/one/two/three"));
		Print(new("https://one/two/three/file.txt"));
		Print(new(@"\\server\folder\file.txt"));
		Print(new(@"\\127.0.0.1\folder\file.txt"));
		Print(new(@"C:\folder\file.txt"));
		Print(new(@"file:///C:/folder/file.txt"));
		
		Uri u1 = new("http://fuxion.dev/one?val=123");
		Uri u11 = new("http://fuxion.dev/one#fra");
		Uri u2 = new("http://fuxion.dev/one/two?other=other");
		Assert.True(u11.IsBaseOf(u1));
		Assert.True(u1.IsBaseOf(u11));
		Assert.True(u1.IsBaseOf(u2));
		var ur = u1.MakeRelativeUri(u2); // ../one?val=123
		Uri u3 = new(u1, ur);
		Print(ur);
		Print(u3);
	}
	[Fact(DisplayName = "Uri2")]
	public void Uri2()
	{
		var uris = new[]
		{
			new Uri("http://fuxion.dev/"),
			new Uri("http://fuxion.dev/one/#fra"),
			new Uri("http://fuxion.dev/one/two/"),
			new Uri("http://fuxion.dev/one/other/"),
		};
		var ut = new Uri("http://fuxion.dev/one/other/");
		Output.WriteLine($"Bases of {ut}");
		foreach (var uri in uris.Where(u => u.IsBaseOf(ut)))
		{
			Output.WriteLine($"\t{uri}");
		}
		UriBuilder ub = new UriBuilder("https://userInfo@fuxion.dev/one/two/three/?query=query#fragment");
		ub.UserName = null;
		ub.Fragment = null;
		ub.Query = null;
		ub.Port = -1;
		
		// Equals if change Fragment, UserInfo or Port
		Assert.Equal(new Uri("https://fuxion.dev/one#fra"),new Uri("https://fuxion.dev/one"));
		Assert.Equal(new Uri("https://userInfo@fuxion.dev/one"),new Uri("https://fuxion.dev/one"));
		Assert.Equal(new Uri("https://fuxion.dev:443/one"),new Uri("https://fuxion.dev/one"));
		// Not equals if change Scheme or Query
		Assert.NotEqual(new Uri("http://fuxion.dev/one"),new Uri("https://fuxion.dev/one"));
		Assert.NotEqual(new Uri("https://fuxion.dev/one?query=query"),new Uri("https://fuxion.dev/one"));
		
		// Base if change Fragment, Port or Query
		Assert.True(new Uri("https://fuxion.dev/one/").IsBaseOf(new Uri("https://fuxion.dev/one/#fra")));
		Assert.True(new Uri("https://fuxion.dev/one/").IsBaseOf(new Uri("https://fuxion.dev:443/one/")));
		Assert.True(new Uri("https://fuxion.dev/one/").IsBaseOf(new Uri("https://fuxion.dev/one/?query=query")));
		// Not base if change UserInfo or Scheme
		Assert.False(new Uri("https://fuxion.dev/one").IsBaseOf(new Uri("https://userInfo@fuxion.dev/one")));
		Assert.False(new Uri("https://fuxion.dev/one").IsBaseOf(new Uri("http://fuxion.dev/one")));

		// How to parse query parameters
		var queryUri = new Uri($"https://fuxion.dev/one?par1=val1&par2=val2&par2=val22&"
			+ $"interface={Uri.EscapeDataString("http://fuxion.dev/one/interface1/?i1par1=i1val1&i1par2=i1val2")}&"
			+ $"interface={Uri.EscapeDataString("http://fuxion.dev/one/interface2/?i2par1=i2val1&i2par2=i2val2")}");
		var pars = System.Web.HttpUtility.ParseQueryString(queryUri.Query);
		Output.WriteLine($"Query:");
		foreach(var par in pars.AllKeys)
			Output.WriteLine($"\tkey: {par} - val: {pars[par]}");
		Assert.Equal("val1", pars["par1"]);
		Assert.Equal("val2,val22", pars["par2"]);
		Assert.Equal("http://fuxion.dev/one/interface1/?i1par1=i1val1&i1par2=i1val2,http://fuxion.dev/one/interface2/?i2par1=i2val1&i2par2=i2val2", pars["interface"]);
		Assert.Null(pars["par3"]);
		Output.WriteLine(queryUri.ToString());
		Output.WriteLine(queryUri.OriginalString);
		Output.WriteLine(Uri.EscapeDataString(queryUri.OriginalString));
		Output.WriteLine(new Uri(Uri.UnescapeDataString(Uri.EscapeDataString(queryUri.OriginalString))).OriginalString);
	}
}