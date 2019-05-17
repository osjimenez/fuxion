using System;

namespace Fuxion
{
	public interface ITimeProvider
	{
		DateTime Now();
		DateTimeOffset NowOffsetted();
		DateTime UtcNow();
	}
}