using System.Web;

namespace Fuxion.Test;

public class UriTest(ITestOutputHelper output) : BaseTest<UriKeyTest>(output)
{
	void PrintUri(Uri uri)
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
	[Fact(DisplayName = "Print")]
	public void Print()
	{
		PrintUri(new("https://fuxion.dev/folder/1.2.3-beta.1.2+metadata"));
		PrintUri(new("https://userInfo@fuxion.dev/one/two/three/?query=query#fragment"));
		PrintUri(new("https://FUXION.dev/one/two/three"));
		PrintUri(new("https://fuxion.dev/one/two/three"));
		PrintUri(new("https://one/two/three/file.txt"));
		PrintUri(new(@"\\server\folder\file.txt"));
		PrintUri(new(@"\\127.0.0.1\byIP\file.txt"));
		PrintUri(new(@"C:\WithLetter\file.txt"));
		PrintUri(new(@"file:///C:/UNC/file.txt"));
		PrintUri(new(@"../one?val=123", UriKind.Relative));
		PrintUri(new(@"/one/two/?val=123", UriKind.Relative));
		PrintUri(new(@"one/two/?val=123", UriKind.Relative));
	}
	[Fact]
	public void Equality()
	{
		// Same uri, obviously are equals
		IsTrue(new Uri("https://fuxion.dev/one").Equals(new Uri("https://fuxion.dev/one")));
		// Fragment is ignored
		IsTrue(new Uri("https://fuxion.dev/one").Equals(new Uri("https://fuxion.dev/one#fragment")));
		// User info is ignored
		IsTrue(new Uri("https://userInfo@fuxion.dev/one").Equals(new Uri("https://fuxion.dev/one")));
		// Default port is ignored
		IsTrue(new Uri("https://fuxion.dev/one").Equals(new Uri("https://fuxion.dev:443/one")));
		// Different path not equals
		IsFalse(new Uri("https://fuxion.dev/one").Equals(new Uri("https://fuxion.dev/one/")));
		// Different query not equals
		IsFalse(new Uri("https://fuxion.dev/one").Equals(new Uri("https://fuxion.dev/one?paremeter=value")));
	}
	[Fact(DisplayName = "Base of")]
	public void BaseOf()
	{
		Uri u1 = new(
			"https://chain3reset2.com/Chain3Reset2_Folder/1.0.0?__base1=https%3A%2F%2Fchain3reset1.com%2FChain3Reset1_Folder%2FChain3Reset1_Echelon1_Folder%2F1.0.0&__base2=https%3A%2F%2Fchain3.com%2FChain3_Folder%2FChain3_Echelon1_Folder%2F1.0.0");
		Uri u2 = new(
			"https://chain3reset2.com/Chain3Reset2_Folder/moreFolders/1.0.0?__base1=https%3A%2F%2Fchain3reset2.com%2FChain3Reset2_Folder%2FChain3Reset2_Echelon1_Folder%2F1.0.0&__base2=https%3A%2F%2Fchain3reset1.com%2FChain3Reset1_Folder%2FChain3Reset1_Echelon1_Folder%2F1.0.0&__base3=https%3A%2F%2Fchain3.com%2FChain3_Folder%2FChain3_Echelon1_Folder%2F1.0.0");
		IsTrue(u1.IsBaseOf(u2));
		
		IsTrue(new Uri("https://fuxion.dev/one").IsBaseOf(new("https://fuxion.dev/one/")));
		IsTrue(new Uri("https://fuxion.dev/one/").IsBaseOf(new("https://fuxion.dev/one/")));
		IsTrue(new Uri("https://fuxion.dev/one/").IsBaseOf(new("https://fuxion.dev/one/two")));
		IsTrue(new Uri("https://fuxion.dev/one/").IsBaseOf(new("https://fuxion.dev/one/two/")));
		// Base if change Fragment, default Port or Query
		IsTrue(new Uri("https://fuxion.dev/one/").IsBaseOf(new("https://fuxion.dev/one/#fra")));
		IsTrue(new Uri("https://fuxion.dev/one/").IsBaseOf(new("https://fuxion.dev:443/one/")));
		IsTrue(new Uri("https://fuxion.dev/one/").IsBaseOf(new("https://fuxion.dev/one/?query=query")));
		IsTrue(new Uri("https://fuxion.dev/one/").IsBaseOf(new("https://FUXION.dev/one/")));
		// TODO Issue when call IsBaseOf and UserInfo differs
		// https://github.com/dotnet/runtime/issues/88265
		// Assert.True(new Uri("https://user@domain.com").IsBaseOf(new("https://domain.com")));
		// Not base if change UserInfo or Scheme
		IsFalse(new Uri("https://fuxion.dev/one").IsBaseOf(new("https://userInfo@fuxion.dev/one")));
		IsFalse(new Uri("https://fuxion.dev/one").IsBaseOf(new("http://fuxion.dev/one")));
		
		IsFalse(new Uri("https://fuxion.dev/one/two/").IsBaseOf(new("https://fuxion.dev/one/")));
		IsFalse(new Uri("https://fuxion.dev/one/").IsBaseOf(new("https://fuxion.dev/one")));
	}
	[Fact(DisplayName = "Comparison")]
	public void Comparison()
	{
		// Equals if change Fragment, UserInfo or Port
		Assert.Equal(new("https://fuxion.dev/one#fra"), new Uri("https://fuxion.dev/one"));
		Assert.Equal(new("https://userInfo@fuxion.dev/one"), new Uri("https://fuxion.dev/one"));
		Assert.Equal(new("https://fuxion.dev:443/one"), new Uri("https://fuxion.dev/one"));
		// Not equals if change Scheme or Query
		Assert.NotEqual(new("http://fuxion.dev/one"), new Uri("https://fuxion.dev/one"));
		Assert.NotEqual(new("https://fuxion.dev/one?query=query"), new Uri("https://fuxion.dev/one"));
	}
	[Fact(DisplayName = "Normalization")]
	public void Normalization()
	{
		// Normalizar una Uri es quitarle UserInfo, Fragment, Query, Port (si es default) y forzar que el Path acabe con '/'
		void Normalize(string uri)
		{
			Output.WriteLine($"Original   Uri: {uri}");
			UriBuilder ub = new(uri);
			ub.UserName = null;
			ub.Fragment = null;
			ub.Query = null;
			if (ub.Uri.IsDefaultPort) ub.Port = -1;
			ub.Path = ub.Path.EndsWith('/') ? ub.Path : ub.Path + '/';
			Output.WriteLine($"Normalized Uri: {ub.Uri}");
		}
		Normalize("https://userInfo@fuxion.dev/one/two/three/?query=query#fragment");
		Normalize("https://userInfo@fuxion.dev/one/two/three?query=query#fragment");
	}

	[Fact(DisplayName = "Query parameters")]
	public void QueryParameters()
	{
		// How to parse query parameters
		var queryUri = new Uri($"https://fuxion.dev/one?par1=val1&par2=val2&par2=val22&" + $"interface={Uri.EscapeDataString("http://fuxion.dev/one/interface1/?i1par1=i1val1&i1par2=i1val2")}&"
			+ $"interface={Uri.EscapeDataString("http://fuxion.dev/one/interface2/?i2par1=i2val1&i2par2=i2val2")}");
		var pars = HttpUtility.ParseQueryString(queryUri.Query);
		Output.WriteLine("Query:");
		foreach (var par in pars.AllKeys) Output.WriteLine($"\tkey: {par} - val: {pars[par]}");
		Assert.Equal("val1", pars["par1"]);
		Assert.Equal("val2,val22", pars["par2"]);
		Assert.Equal("http://fuxion.dev/one/interface1/?i1par1=i1val1&i1par2=i1val2,http://fuxion.dev/one/interface2/?i2par1=i2val1&i2par2=i2val2", pars["interface"]);
		Assert.Null(pars["par3"]);
		Output.WriteLine(queryUri.ToString());
		Output.WriteLine(queryUri.OriginalString);
		Output.WriteLine(Uri.EscapeDataString(queryUri.OriginalString));
		Output.WriteLine(new Uri(Uri.UnescapeDataString(Uri.EscapeDataString(queryUri.OriginalString))).OriginalString);
	}
	[Fact(DisplayName = "Fragment")]
	public void Fragment()
	{
		PrintUri(new("https://fuxion.dev/one/two/#fragment"));
		PrintUri(new("https://fuxion.dev/one/two/?par1=val1&par2=val2#http://fuxion.dev/one/two?fpar1=fval1&fpar2=fval2"));
	}
	[Fact(DisplayName = "Relative uris")]
	public void RelativeUris()
	{
		Output.WriteLine(new Uri("one",UriKind.RelativeOrAbsolute).ToString());
		Output.WriteLine(new Uri("one#fragment",UriKind.RelativeOrAbsolute).ToString());
	}
}