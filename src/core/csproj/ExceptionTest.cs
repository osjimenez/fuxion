using System;
using System.Collections.Generic;
using System.Text;

namespace Fuxion
{
	public class ExceptionTest
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
