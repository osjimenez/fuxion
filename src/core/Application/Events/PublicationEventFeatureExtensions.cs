using Fuxion.Domain.Events;
using Fuxion.Domain;
using System;

namespace Fuxion.Application.Events
{
	public static class PublicationEventFeatureExtensions
	{
		public static bool HasPublication(this Event me)
			=> me.HasFeature<PublicationEventFeature>();
		public static PublicationEventFeature Publication(this Event me)
			=> me.GetFeature<PublicationEventFeature>();
		public static void AddPublication(this Event me, DateTime timestamp)
			=> me.AddFeature<PublicationEventFeature>(esef =>
			{
				esef.Timestamp = timestamp;
			});
		public static PublicationPod ToPublicationPod(this Event me) => new PublicationPod(me);
	}
}
