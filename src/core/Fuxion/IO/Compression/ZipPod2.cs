using System.IO.Compression;
using System.Text.Json.Serialization;

namespace Fuxion.IO.Compression;

public class ZipPod2<TDiscriminator> : Pod2<TDiscriminator, byte[]>
	where TDiscriminator : notnull
{
	public ZipPod2(TDiscriminator discriminator, byte[] dataToCompress) : base(discriminator, Compress(dataToCompress)) { }
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
}
public class UnzipPod2<TDiscriminator> : Pod2<TDiscriminator, byte[]>
	where TDiscriminator : notnull
{
	public UnzipPod2(TDiscriminator discriminator, byte[] dataToDecompress) : base(discriminator, Decompress(dataToDecompress)) { }
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
// public class ZipPod2<TDiscriminator> : ICollectionPod2<TDiscriminator, byte[]>
// 	where TDiscriminator : notnull
// {
// 	ZipPod2(TDiscriminator discriminator)
// 	{
// 		Discriminator = discriminator;
// 		CompressedData = default!;
// 		UncompressedData = default!;
// 	}
// 	public static ZipPod2<TDiscriminator> CreateToCompress(TDiscriminator discriminator, byte[] dataToCompress) =>
// 		new(discriminator)
// 		{
// 			UncompressedData = dataToCompress, CompressedData = Compress(dataToCompress)
// 		};
// 	public static ZipPod2<TDiscriminator> CreateToDecompress(TDiscriminator discriminator, byte[] dataToDecompress) =>
// 		new(discriminator)
// 		{
// 			UncompressedData = Decompress(dataToDecompress), CompressedData = dataToDecompress
// 		};
// 	#region Collection
// 	readonly Dictionary<TDiscriminator, IPod2<TDiscriminator, object>> dic = new();
// 	public bool Has(TDiscriminator discriminator) => dic.ContainsKey(discriminator);
// 	public IPod2<TDiscriminator, object> this[TDiscriminator discriminator] => dic[discriminator];
// 	public void Add(IPod2<TDiscriminator, object> pod) => dic.Add(pod.Discriminator, pod);
// 	public bool Remove(TDiscriminator discriminator) => dic.Remove(discriminator);
// 	public IEnumerator<IPod2<TDiscriminator, object>> GetEnumerator() => dic.Values.GetEnumerator();
// 	#endregion
// 	byte[] CompressedData { get; set; }
// 	byte[] UncompressedData { get; set; }
// 	public TDiscriminator Discriminator { get; }
// 	public byte[] Payload => UncompressedData;
// 	// [JsonIgnore]
// 	// public byte[] Raw => CompressedData;
// 	// public byte[] Outside() => UncompressedData;
// 	// public byte[] Inside() => CompressedData;
// 	// public PodCollection2<TDiscriminator> Headers { get; } = new();
// 	static byte[] Compress(byte[] bytes)
// 	{
// 		using var msi = new MemoryStream(bytes);
// 		using var memoryStream = new MemoryStream();
// 		using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
// 		{
// 			msi.CopyTo(gZipStream);
// 		}
// 		return memoryStream.ToArray();
// 	}
// 	static byte[] Decompress(byte[] bytes)
// 	{
// 		using var msi = new MemoryStream(bytes);
// 		using var memoryStream = new MemoryStream();
// 		using (var gZipStream = new GZipStream(msi, CompressionMode.Decompress))
// 		{
// 			gZipStream.CopyTo(memoryStream);
// 		}
// 		return memoryStream.ToArray();
// 	}
// }