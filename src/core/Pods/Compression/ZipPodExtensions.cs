using System.Text;

namespace Fuxion.Pods.Compression;

public static class ZipPodExtensions
{
	 public static IPodBuilder<TDiscriminator, byte[], ZipPod<TDiscriminator>> ToZip<TDiscriminator>(this IPodBuilder<TDiscriminator, byte[], IPod<TDiscriminator, byte[]>> me, TDiscriminator discriminator)
		 where TDiscriminator : notnull
		 => new PodBuilder<TDiscriminator, byte[], ZipPod<TDiscriminator>>(new(discriminator, me.Pod.Payload));
	 public static IPodBuilder<TDiscriminator, byte[], UnzipPod<TDiscriminator>> FromZip<TDiscriminator>(this IPodBuilder<TDiscriminator, byte[], IPod<TDiscriminator, byte[]>> me)
		 where TDiscriminator : notnull
		 => new PodBuilder<TDiscriminator, byte[], UnzipPod<TDiscriminator>>(new(me.Pod.Discriminator, me.Pod.Payload));
	 public static IPodBuilder<TDiscriminator, byte[], UnzipPod<TDiscriminator>> FromZip<TDiscriminator>(this IPodPreBuilder<byte[]> me, TDiscriminator discriminator)
		 where TDiscriminator : notnull
		 => new PodBuilder<TDiscriminator, byte[], UnzipPod<TDiscriminator>>(new(discriminator, me.Payload));
	 public static IPodBuilder<TDiscriminator, string, IPod<TDiscriminator, string>> FromUtf8Bytes<TDiscriminator>(this IPodBuilder<TDiscriminator, byte[], UnzipPod<TDiscriminator>> me)
		 where TDiscriminator : notnull
		 => new PodBuilder<TDiscriminator, string, IPod<TDiscriminator, string>>(new Pod<TDiscriminator, string>(me.Pod.Discriminator, Encoding.UTF8.GetString(me.Pod.Payload)));
}