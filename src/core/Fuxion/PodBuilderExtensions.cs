using System.ComponentModel.Design;
using System.Numerics;
using System.Text;
using System.Text.Json.Nodes;

namespace Fuxion;

public static class PodBuilderExtensions
{
	public static IPodBuilder<IPod<TDiscriminator, T, T>> BuildPod<TDiscriminator, T>(this T me, TDiscriminator discriminator) 
		where TDiscriminator : notnull 
		where T : notnull =>
		new PodBuilder<TDiscriminator, T, T, IPod<TDiscriminator, T, T>>(new BypassPod<TDiscriminator, T, T>(discriminator, me));
}