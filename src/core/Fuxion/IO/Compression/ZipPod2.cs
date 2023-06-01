using System.IO.Compression;
using System.Text.Json.Serialization;

namespace Fuxion.IO.Compression;

public class ZipPod2<TDiscriminator> : IRawPod2<TDiscriminator, byte[], byte[], BypassPodCollection2<TDiscriminator>>
	where TDiscriminator : notnull
{
	ZipPod2(TDiscriminator discriminator)
	{
		Discriminator = discriminator;
		CompressedData = default!;
		UncompressedData = default!;
	}
	public static ZipPod2<TDiscriminator> CreateToCompress(TDiscriminator discriminator, byte[] dataToCompress) =>
		new(discriminator)
		{
			UncompressedData = dataToCompress, CompressedData = Compress(dataToCompress)
		};
	public static ZipPod2<TDiscriminator> CreateToDecompress(TDiscriminator discriminator, byte[] dataToDecompress) =>
		new(discriminator)
		{
			UncompressedData = Decompress(dataToDecompress), CompressedData = dataToDecompress
		};
	byte[] CompressedData { get; set; }
	byte[] UncompressedData { get; set; }
	public TDiscriminator Discriminator { get; }
	public byte[] Payload => UncompressedData;
	[JsonIgnore]
	public byte[] Raw => CompressedData;
	// public byte[] Outside() => UncompressedData;
	// public byte[] Inside() => CompressedData;
	public BypassPodCollection2<TDiscriminator> Headers { get; } = new();
	static byte[] Compress(byte[] bytes)
	{
		using var msi = new MemoryStream(bytes);
		using var memoryStream = new MemoryStream();
		using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
		{
			msi.CopyTo(gZipStream);
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
		}
		return memoryStream.ToArray();
	}
}