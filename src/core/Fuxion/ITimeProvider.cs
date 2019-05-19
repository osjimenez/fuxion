using System;

namespace Fuxion
{
	[DefaultSingletonInstance(typeof(LocalMachinneTimeProvider))]
	public interface ITimeProvider
	{
		DateTime Now();
		DateTimeOffset NowOffsetted();
		DateTime UtcNow();
	}
}