namespace Fuxion.Identity.DatabaseEFTest
{
	public class AlwaysRootCurrentUserNameProvider : ICurrentUserNameProvider
	{
		public string GetCurrentUserName() => "root";
	}
}
