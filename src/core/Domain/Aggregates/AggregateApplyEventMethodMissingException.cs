using System;
using System.Collections.Generic;
using System.Text;

namespace Fuxion.Domain.Aggregates
{
	public class AggregateApplyEventMethodMissingException : Exception
	{
		public AggregateApplyEventMethodMissingException(string message) : base(message)
		{

		}
	}
}
