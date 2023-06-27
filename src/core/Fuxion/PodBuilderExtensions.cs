namespace Fuxion;

public static class PodBuilder2Extensions
{
	public static IPodPreBuilder<T> BuildPod<T>(this T me)
		where T : notnull
		=> new PodPreBuilder<T>(me);
	public static IPodBuilder<Pod<TDiscriminator, TPayload>> ToPod<TDiscriminator, TPayload>(this IPodPreBuilder<TPayload> me, TDiscriminator discriminator)
		where TDiscriminator : notnull
		where TPayload : notnull
		=> new PodBuilder<TDiscriminator, TPayload, Pod<TDiscriminator, TPayload>>(new(discriminator, me.Payload));
	public static IPodBuilder<TPod> RebuildPod<TPod>(this TPod me)
		where TPod : IPod<object, object>
		=> new PodBuilder<object, object, TPod>(me);
	public static IPodBuilder<TPod> AddHeader<TDiscriminator, TPod>(this IPodBuilder<TPod> me, IPod<TDiscriminator, object> header)
		where TPod : ICollectionPod<TDiscriminator, object>
	{
		me.Pod.Add(header);
		return me;
	}
	public static IPodBuilder<TPod> AddHeader<TDiscriminator, TPod>(this IPodBuilder<TPod> me, TDiscriminator discriminator, object payload)
		where TDiscriminator : notnull
		where TPod : ICollectionPod<TDiscriminator, object>
	{
		me.Pod.Add(new Pod<TDiscriminator, object>(discriminator, payload));
		return me;
	}
}