using System.Text;

namespace Fuxion.IO.Compression;

public static class ZipPodExtensions
{
	public static IPodBuilder<ZipPod<TDiscriminator>> ToZip<TDiscriminator>(this IPodBuilder<IPod<TDiscriminator, byte[]>> me, TDiscriminator discriminator)
		where TDiscriminator : notnull
		=> new PodBuilder<TDiscriminator, object, ZipPod<TDiscriminator>>(new(discriminator, me.Pod.Payload));
	public static IPodBuilder<UnzipPod2<TDiscriminator>> FromZip<TDiscriminator>(this IPodBuilder<IPod<TDiscriminator, byte[]>> me)
		where TDiscriminator : notnull
		=> new PodBuilder<TDiscriminator, object, UnzipPod2<TDiscriminator>>(new(me.Pod.Discriminator, me.Pod.Payload));
	public static IPodBuilder<IPod<TDiscriminator, string>> FromUtf8Bytes<TDiscriminator>(this IPodBuilder<UnzipPod2<TDiscriminator>> me)
		where TDiscriminator : notnull
		=> new PodBuilder<TDiscriminator, string, IPod<TDiscriminator, string>>(new Pod<TDiscriminator, string>(me.Pod.Discriminator, Encoding.UTF8.GetString(me.Pod.Payload)));
}