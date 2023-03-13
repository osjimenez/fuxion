using Fuxion.Json;

namespace Fuxion.IO.Compression;

public static class ZipPodExtensions
{
	public static IPodBuilder<ZipPod<TDiscriminator>> Zip<TDiscriminator>(this IPodBuilder<IPod<TDiscriminator, object, byte[]>> me, TDiscriminator discriminator) 
		where TDiscriminator : notnull =>
		new PodBuilder<TDiscriminator, object, byte[],ZipPod<TDiscriminator>>(ZipPod<TDiscriminator>.CreateToCompress(discriminator, me.Pod.Inside()));
	public static IPodBuilder<ZipPod<TDiscriminator>> Unzip<TDiscriminator>(this IPodBuilder<IPod<TDiscriminator, object, byte[]>> me, TDiscriminator discriminator) 
		where TDiscriminator : notnull =>
		new PodBuilder<TDiscriminator, object, byte[],ZipPod<TDiscriminator>>(ZipPod<TDiscriminator>.CreateToDecompress(discriminator, me.Pod.Inside()));
}