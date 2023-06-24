using System.Text;
using Fuxion.Json;

namespace Fuxion.IO.Compression;

public static class ZipPod2Extensions
{
	public static IPodBuilder2<ZipPod2<TDiscriminator>> ToZip<TDiscriminator>(this IPodBuilder2<IPod2<TDiscriminator, byte[]>> me, TDiscriminator discriminator) 
		where TDiscriminator : notnull =>
		// TODO Remove nullable Payload!
		new PodBuilder2<TDiscriminator, object, ZipPod2<TDiscriminator>>(new ZipPod2<TDiscriminator>(discriminator, me.Pod.Payload!));
	public static IPodBuilder2<UnzipPod2<TDiscriminator>> FromZip<TDiscriminator>(this IPodBuilder2<IPod2<TDiscriminator, byte[]>> me) 
		where TDiscriminator : notnull =>
		// TODO Remove nullable Payload!
		new PodBuilder2<TDiscriminator, object, UnzipPod2<TDiscriminator>>(new UnzipPod2<TDiscriminator>(me.Pod.Discriminator, me.Pod.Payload!));
	public static IPodBuilder2<IPod2<TDiscriminator, string>> FromUtf8<TDiscriminator>(this IPodBuilder2<UnzipPod2<TDiscriminator>> me) where TDiscriminator : notnull =>
		new PodBuilder2<TDiscriminator, string, IPod2<TDiscriminator, string>>(
			// TODO Remove nullable Payload!
			new Pod2<TDiscriminator, string>(me.Pod.Discriminator, Encoding.UTF8.GetString(me.Pod.Payload!)));
}