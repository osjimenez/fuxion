namespace Fuxion;

public class ExceptionTest
{
	public void LaunchException()
	{
		var test = new InternalExceptionTest();
		test.LaunchException();
	}
}

internal class InternalExceptionTest
{
	public void LaunchException() => PrivateLaunchException();
	private void PrivateLaunchException() => throw new Exception("Exception for testing purpose");
}