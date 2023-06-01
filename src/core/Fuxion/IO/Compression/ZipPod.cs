using System.IO.Compression;

namespace Fuxion.IO.Compression;

public class ZipPod<TDiscriminator> : ICrossPod<TDiscriminator, byte[], byte[]>//, ZipPodCollection<TDiscriminator>>
	where TDiscriminator : notnull
{
	ZipPod(TDiscriminator discriminator)
	{
		Discriminator = discriminator;
		CompressedData = default!;
		UncompressedData = default!;
	}
	public static ZipPod<TDiscriminator> CreateToCompress(TDiscriminator discriminator, byte[] dataToCompress) =>
		new(discriminator)
		{
			UncompressedData = dataToCompress, CompressedData = Compress(dataToCompress)
		};
	public static ZipPod<TDiscriminator> CreateToDecompress(TDiscriminator discriminator, byte[] dataToDecompress) =>
		new(discriminator)
		{
			UncompressedData = Decompress(dataToDecompress), CompressedData = dataToDecompress
		};
	byte[] CompressedData { get; set; }
	byte[] UncompressedData { get; set; }
	public TDiscriminator Discriminator { get; }
	public byte[] Outside() => UncompressedData;
	public byte[] Inside() => CompressedData;
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