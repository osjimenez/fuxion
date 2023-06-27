using System.ComponentModel.Design;
using System.Numerics;
using System.Text;
using System.Text.Json.Nodes;

namespace Fuxion;

public static class PodBuilder2Extensions
{
	public static IPodBuilder2<ICollectionPod2<TDiscriminator, T>> BuildPod2<TDiscriminator, T>(this T me, TDiscriminator discriminator) 
		where TDiscriminator : notnull 
		where T : notnull =>
		new PodBuilder2<TDiscriminator, T, ICollectionPod2<TDiscriminator, T>>(new Pod2<TDiscriminator, T>(discriminator, me));
	// public static IPodBuilder2<IPod2<TDiscriminator, T>> BuildPod2<TDiscriminator, T>(this IPod2<TDiscriminator, T> me) 
	// 	where TDiscriminator : notnull 
	// 	where T : notnull =>
	// 	new PodBuilder2<TDiscriminator, T, IPod2<TDiscriminator, T>>(me);
	// public static IPodBuilder2<TPod> BuildPod2<TDiscriminator, TPayload, TPod>(this TPod me) 
	// 	where TDiscriminator : notnull 
	// 	where TPayload : notnull 
	// 	where TPod : IPod2<TDiscriminator, TPayload> =>
	// 	new PodBuilder2<TDiscriminator, TPayload, TPod>(me);
	public static IPodBuilder2<TPod> RebuildPod2<TPod>(this TPod me) 

		where TPod : IPod2<object, object> =>
		new PodBuilder2<object, object, TPod>(me);
	public static IPodBuilder2<TPod> AddHeader<TDiscriminator, TPod>(this IPodBuilder2<TPod> me, IPod2<TDiscriminator, object> header)
		where TPod : ICollectionPod2<TDiscriminator, object>
	{
		me.Pod.Add(header);
		return me;
	}
	public static IPodBuilder2<TPod> AddHeader<TDiscriminator, TPod>(this IPodBuilder2<TPod> me, TDiscriminator discriminator, object payload)
	where TDiscriminator : notnull
		where TPod : ICollectionPod2<TDiscriminator, object>
	{
		me.Pod.Add(new Pod2<TDiscriminator, object>(discriminator, payload));
		return me;
	}
}
