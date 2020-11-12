using Microsoft.QualityTools.Testing.Fakes;
using Pose;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Fuxion.Test
{
	public class FakesTest
	{
		[Fact(DisplayName = "Microsoft Fakes - DateTime")]
		public void DateTimeTest()
		{
			int fixedYear = 2000;

			// Shims can be used only in a ShimsContext:
			using (ShimsContext.Create())
			{
				// Arrange:
				// Shim DateTime.Now to return a fixed date:
				System.Fakes.ShimDateTime.NowGet = () => new DateTime(fixedYear, 1, 1);


				// Act:
				int year = DateTime.Now.Year;

				// Assert:
				// This will always be true if the component is working:
				Assert.Equal(fixedYear, year);
			}
		}
	}
}
