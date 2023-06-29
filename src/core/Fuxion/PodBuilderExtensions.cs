﻿using System.Text;
using Fuxion.Reflection;

namespace Fuxion;

public static class PodBuilder2Extensions
{
	public static IPodPreBuilder<TPayload> BuildPod<TPayload>(this TPayload me)
		where TPayload : notnull
		=> new PodPreBuilder<TPayload>(me);
	public static IPodBuilder<Pod<TDiscriminator, TPayload>> ToPod<TDiscriminator, TPayload>(this IPodPreBuilder<TPayload> me, TDiscriminator discriminator)
		where TDiscriminator : notnull
		where TPayload : notnull
		=> new PodBuilder<TDiscriminator, TPayload, Pod<TDiscriminator, TPayload>>(new(discriminator, me.Payload));
	public static IPodBuilder<TPod> RebuildPod<TPod>(this TPod me)
		where TPod : IPod<object, object>
		=> new PodBuilder<object, object, TPod>(me);
	public static TPodBuilder AddHeader<TDiscriminator, TPodBuilder>(this TPodBuilder me, IPod<TDiscriminator, object> header)
		where TPodBuilder : IPodBuilder<ICollectionPod<TDiscriminator, object>>
	{
		me.Pod.Add(header);
		return me;
	}
	public static TPodBuilder AddHeader<TDiscriminator, TPodBuilder>(this TPodBuilder me, TDiscriminator discriminator, object payload)
		where TDiscriminator : notnull
		where TPodBuilder : IPodBuilder<ICollectionPod<TDiscriminator, object>>
	{
		me.Pod.Add(new Pod<TDiscriminator, object>(discriminator, payload));
		return me;
	}
	public static IPodBuilder<IPod<TDiscriminator, byte[]>> ToUtf8Bytes<TDiscriminator>(this IPodBuilder<IPod<TDiscriminator, string>> me, TDiscriminator discriminator)
		where TDiscriminator : notnull
		=> new PodBuilder<TDiscriminator, byte[], IPod<TDiscriminator, byte[]>>(new Pod<TDiscriminator, byte[]>(discriminator, Encoding.UTF8.GetBytes(me.Pod.Payload)));
	public static IPodBuilder<IPod<TDiscriminator, string>> FromUtf8Bytes<TDiscriminator>(this IPodBuilder<IPod<TDiscriminator, byte[]>> me, TDiscriminator discriminator)
		where TDiscriminator : notnull
		=> new PodBuilder<TDiscriminator, string, IPod<TDiscriminator, string>>(new Pod<TDiscriminator, string>(discriminator, Encoding.UTF8.GetString(me.Pod.Payload)));
	public static IPodBuilder<IPod<TDiscriminator, string>> FromUtf8Bytes<TDiscriminator>(this IPodPreBuilder<byte[]> me, TDiscriminator discriminator)
		where TDiscriminator : notnull
		=> new PodBuilder<TDiscriminator, string, IPod<TDiscriminator, string>>(new Pod<TDiscriminator, string>(discriminator, Encoding.UTF8.GetString(me.Payload)));
}