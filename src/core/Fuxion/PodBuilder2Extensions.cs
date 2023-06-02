using System.ComponentModel.Design;
using System.Numerics;
using System.Text;
using System.Text.Json.Nodes;

namespace Fuxion;

public static class PodBuilder2Extensions
{
	public static IPodBuilder2<IPod2<TDiscriminator, T>> BuildPod2<TDiscriminator, T>(this T me, TDiscriminator discriminator) 
		where TDiscriminator : notnull 
		where T : notnull =>
		new PodBuilder2<TDiscriminator, T, IPod2<TDiscriminator, T>>(new Pod2<TDiscriminator, T>(discriminator, me));
}