namespace Fuxion.Identity;

public interface ICurrentUserNameProvider
{
	string GetCurrentUserName();
}

public class GenericCurrentUserNameProvider : ICurrentUserNameProvider
{
	public GenericCurrentUserNameProvider(Func<string> getCurrentUsernameFunction) => this.getCurrentUsernameFunction = getCurrentUsernameFunction;
	readonly Func<string> getCurrentUsernameFunction;
	public string GetCurrentUserName() => getCurrentUsernameFunction();
}