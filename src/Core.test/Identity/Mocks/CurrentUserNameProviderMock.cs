namespace Fuxion.Identity.Test.Mocks;

public class CurrentUserNameProviderMock : ICurrentUserNameProvider
{
	public CurrentUserNameProviderMock(Func<string> currentUserNameFunction) => this.currentUserNameFunction = currentUserNameFunction;
	readonly Func<string> currentUserNameFunction;
	public string GetCurrentUserName() => currentUserNameFunction();
}