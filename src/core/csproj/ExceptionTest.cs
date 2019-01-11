using System;
using System.Collections.Generic;
using System.Text;

namespace Fuxion
{
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
		public void LaunchException()
		{
			PrivateLaunchException();
		}
		private void PrivateLaunchException()
		{
			throw new Exception("Exception for testing purpose");
		}
	}
}
