using System.IO.Compression;

namespace Fuxion.IO.Compression;

public class ZipPod<TDiscriminator>(TDiscriminator discriminator, byte[] dataToCompress) : Pod<TDiscriminator, byte[]>(discriminator, Compress(dataToCompress))
	where TDiscriminator : notnull
{
	static byte[] Compress(byte[] bytes)
	{
		using var msi = new MemoryStream(bytes);
		using var memoryStream = new MemoryStream();
		using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress)) msi.CopyTo(gZipStream);
		return memoryStream.ToArray();
	}
}

public class UnzipPod2<TDiscriminator>(TDiscriminator discriminator, byte[] dataToDecompress) : Pod<TDiscriminator, byte[]>(discriminator, Decompress(dataToDecompress))
	where TDiscriminator : notnull
{
	static byte[] Decompress(byte[] bytes)
	{
		using var msi = new MemoryStream(bytes);
		using var memoryStream = new MemoryStream();
		using (var gZipStream = new GZipStream(msi, CompressionMode.Decompress)) gZipStream.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}
}