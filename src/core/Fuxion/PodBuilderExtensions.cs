using System.ComponentModel.Design;
using System.Numerics;
using System.Text;
using System.Text.Json.Nodes;

namespace Fuxion;

public static class PodBuilderExtensions
{
	public static IPodBuilder<IPod<TDiscriminator, T, T>> ToPod<TDiscriminator, T>(this T me, TDiscriminator discriminator) 
		where TDiscriminator : notnull 
		where T : notnull =>
		new PodBuilder<TDiscriminator, T, T, IPod<TDiscriminator, T, T>>(new Pod<TDiscriminator, T, T>(discriminator, me));

	public static IPodBuilder<IPod<TDiscriminator, TOutside, TInside>> FromPod<TDiscriminator, TOutside, TInside>(this IPod<TDiscriminator, TOutside, TInside> me, TDiscriminator discriminator)
		where TDiscriminator : notnull
		where TOutside : notnull 
		where TInside : notnull
	{
		if (!me.Discriminator.Equals(discriminator))
			throw new ArgumentException($"The discriminator '{discriminator}' must be equal to pod discriminator '{me.Discriminator}'");
		return new PodBuilder<TDiscriminator, TOutside, TInside, IPod<TDiscriminator, TOutside, TInside>>(me);
	}
}