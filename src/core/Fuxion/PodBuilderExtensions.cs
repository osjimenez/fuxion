using System.ComponentModel.Design;
using System.Text;
using System.Text.Json.Nodes;
using Fuxion.Reflection;
using Fuxion.Text.Json;

namespace Fuxion;

public static class PodBuilder2Extensions
{
	public static IPodPreBuilder<TPayload> BuildPod<TPayload>(this TPayload me)
		where TPayload : notnull
		=> new PodPreBuilder<TPayload>(me);
	public static IPodBuilder<TDiscriminator, TPayload, Pod<TDiscriminator, TPayload>> ToPod<TDiscriminator, TPayload>(this IPodPreBuilder<TPayload> me, TDiscriminator discriminator)
		where TDiscriminator : notnull
		where TPayload : notnull
		=> new PodBuilder<TDiscriminator, TPayload, Pod<TDiscriminator, TPayload>>(new(discriminator, me.Payload));
	public static IPodBuilder<TDiscriminator, TPayload, TPod> RebuildPod<TDiscriminator, TPayload, TPod>(this TPod me)
		where TDiscriminator : notnull
		where TPayload : notnull
		where TPod : IPod<TDiscriminator, TPayload>
		=> new PodBuilder<TDiscriminator, TPayload, TPod>(me);
	public static TPodBuilder AddHeader<TDiscriminator, TPodBuilder>(this TPodBuilder me, IPod<TDiscriminator, object> header)
		where TPodBuilder : IPodBuilder<TDiscriminator, object, ICollectionPod<TDiscriminator, object>>
	{
		me.Pod.Add(header);
		return me;
	}
	public static TPodBuilder AddHeader<TDiscriminator, TPodBuilder>(this TPodBuilder me, TDiscriminator discriminator, object payload)
		where TDiscriminator : notnull
		where TPodBuilder : IPodBuilder<TDiscriminator, object, ICollectionPod<TDiscriminator, object>>
	{
		me.Pod.Add(new Pod<TDiscriminator, object>(discriminator, payload));
		return me;
	}

	
	public static IPodBuilder<TDiscriminator, byte[], IPod<TDiscriminator, byte[]>> ToUtf8Bytes<TDiscriminator>(this IPodBuilder<TDiscriminator, string, IPod<TDiscriminator, string>> me, TDiscriminator discriminator)
		where TDiscriminator : notnull
		=> new PodBuilder<TDiscriminator, byte[], IPod<TDiscriminator, byte[]>>(new Pod<TDiscriminator, byte[]>(discriminator, Encoding.UTF8.GetBytes(me.Pod.Payload)));
	public static IPodBuilder<TDiscriminator, string, IPod<TDiscriminator, string>> FromUtf8Bytes<TDiscriminator>(this IPodBuilder<TDiscriminator, byte[], IPod<TDiscriminator, byte[]>> me, TDiscriminator discriminator)
		where TDiscriminator : notnull
		=> new PodBuilder<TDiscriminator, string, IPod<TDiscriminator, string>>(new Pod<TDiscriminator, string>(discriminator, Encoding.UTF8.GetString(me.Pod.Payload)));
	public static IPodBuilder<TDiscriminator, string, IPod<TDiscriminator, string>> FromUtf8Bytes<TDiscriminator>(this IPodPreBuilder<byte[]> me, TDiscriminator discriminator)
		where TDiscriminator : notnull
		=> new PodBuilder<TDiscriminator, string, IPod<TDiscriminator, string>>(new Pod<TDiscriminator, string>(discriminator, Encoding.UTF8.GetString(me.Payload)));
}