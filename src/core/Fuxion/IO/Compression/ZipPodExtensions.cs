using System.Text;
using Fuxion.Json;

namespace Fuxion.IO.Compression;

public static class ZipPodExtensions
{
	public static IPodBuilder<ZipPod<TDiscriminator>> ToZip<TDiscriminator>(this IPodBuilder<ICrossPod<TDiscriminator, object, byte[]>> me, TDiscriminator discriminator) 
		where TDiscriminator : notnull =>
		new PodBuilder<TDiscriminator, object, byte[],ZipPod<TDiscriminator>>(ZipPod<TDiscriminator>.CreateToCompress(discriminator, me.Pod.Inside()));
	public static IPodBuilder<ZipPod<TDiscriminator>> FromZip<TDiscriminator>(this IPodBuilder<ICrossPod<TDiscriminator, object, byte[]>> me) 
		where TDiscriminator : notnull =>
		new PodBuilder<TDiscriminator, object, byte[],ZipPod<TDiscriminator>>(ZipPod<TDiscriminator>.CreateToDecompress(me.Pod.Discriminator, me.Pod.Inside()));
	public static IPodBuilder<ICrossPod<TDiscriminator, string, string>> FromUtf8<TDiscriminator>(this IPodBuilder<ZipPod<TDiscriminator>> me) where TDiscriminator : notnull =>
		new PodBuilder<TDiscriminator, string, string, ICrossPod<TDiscriminator, string, string>>(
			new BypassPod<TDiscriminator, string, string>(me.Pod.Discriminator, Encoding.UTF8.GetString(me.Pod.Outside())));
}