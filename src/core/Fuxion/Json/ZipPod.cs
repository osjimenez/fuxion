using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Text;

namespace Fuxion.Json;

public class ZipPod<TDiscriminator> : IPod<TDiscriminator, byte[], byte[]>//, ZipPodCollection<TDiscriminator>>
	where TDiscriminator : notnull
{
	ZipPod(TDiscriminator discriminator)
	{
		Discriminator = discriminator;
		CompressedData = default!;
		UncompressedData = default!;
	}
	public static ZipPod<TDiscriminator> CreateToCompress(TDiscriminator discriminator, byte[] dataToCompress)
	{
		var pod = new ZipPod<TDiscriminator>(discriminator);
		pod.UncompressedData = dataToCompress;
		pod.CompressedData = Compress(dataToCompress);
		return pod;
	}
	public static ZipPod<TDiscriminator> CreateToDecompress(TDiscriminator discriminator, byte[] dataToDecompress)
	{
		var pod = new ZipPod<TDiscriminator>(discriminator);
		pod.UncompressedData = Decompress(dataToDecompress);
		pod.CompressedData = dataToDecompress;
		return pod;
	}
	byte[] CompressedData { get; set; }
	byte[] UncompressedData { get; set; }
	public TDiscriminator Discriminator { get; }
	public byte[] Outside() => UncompressedData;
	public byte[] Inside() => CompressedData;
	// public static void Save(Stream source, Stream destination) {
	// 	byte[] bytes = new byte[4096];
	//
	// 	int count;
	//
	// 	while ((count = source.Read(bytes, 0, bytes.Length)) != 0) {
	// 		destination.Write(bytes, 0, count);
	// 	}
	// }
	static byte[] Compress(byte[] bytes)
	{
		using var msi = new MemoryStream(bytes);
		using var memoryStream = new MemoryStream();
		using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
		{
			msi.CopyTo(gZipStream);
			// Save(msi, gZipStream);
		}
		return memoryStream.ToArray();
	}
	static byte[] Decompress(byte[] bytes)
	{
		using var msi = new MemoryStream(bytes);
		using var memoryStream = new MemoryStream();
		using (var gZipStream = new GZipStream(msi, CompressionMode.Decompress))
		{
			gZipStream.CopyTo(memoryStream);
			// Save(gZipStream, memoryStream);
		}
		return memoryStream.ToArray();
	}
}