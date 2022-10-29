namespace Fuxion;

public class ExceptionTest
{
	public void LaunchException()
	{
		var test = new InternalExceptionTest();
		test.LaunchException();
	}
}

class InternalExceptionTest
{
	public void LaunchException()        => PrivateLaunchException();
	void        PrivateLaunchException() => throw new("Exception for testing purpose");
}