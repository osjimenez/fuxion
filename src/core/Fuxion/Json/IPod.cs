using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Fuxion.Json;

// public interface IPod<TDiscriminator, TPayload> : IPod<TDiscriminator, TPayload, IPodCollection<TDiscriminator>>
// 	{ }
public interface IPod<TDiscriminator, TPayload>
{
	TDiscriminator Discriminator { get; }
	TPayload? Payload { get; }
	T? As<T>();
	bool TryAs<T>([NotNullWhen(true)] out T? payload);
	bool Is<T>();
}
public interface IPod<TDiscriminator, TPayload, TCollection> : IPod<TDiscriminator, TPayload>
	where TCollection : IPodCollection<TDiscriminator, IPod<TDiscriminator, object, TCollection>>
{
	TCollection Headers { get; }
	// TDiscriminator Discriminator { get; }
	// TPayload? Payload { get; }
	// T? As<T>();
	// bool TryAs<T>([NotNullWhen(true)] out T? payload);
	// bool Is<T>();
	//TPod ToPod<TPod>();
}
public interface IPodCollection<TDiscriminator, TPod> : IEnumerable<TPod>
	//where TPod : IPod<TDiscriminator, object, IPodCollection<TDiscriminator, TPod>>
	//where TPodCollectionEntry : IPodCollectionEntry<TDiscriminator>
{
	void Add<TPayload>(TDiscriminator discriminator, TPayload payload);
	void Add<TPayload>(TPod pod);
	void Replace<TPayload>(TDiscriminator discriminator, TPayload payload);
	bool Has(TDiscriminator discriminator);
	TPod? GetPod<TPayload>(TDiscriminator discriminator);
	bool TryGetPod<TPayload>(TDiscriminator discriminator, [NotNullWhen(true)] out TPod? pod);
	TPayload? GetPayload<TPayload>(TDiscriminator discriminator);
	bool TryGetPayload<TPayload>(TDiscriminator discriminator, [NotNullWhen(true)] out TPayload? payload);
	TPod this[TDiscriminator discriminator] { get; }
	// IMPLEMENTED
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	void Modify<TPayload>(TDiscriminator discriminator, Func<TPayload, TPayload> modifyFunction)
		//=> Replace(discriminator, modifyFunction(GetPayload<TPayload>(discriminator)));
	{
		var payload = GetPayload<TPayload>(discriminator) ?? throw new ArgumentException($"ooooooooooooooooooooooooooooo");
		var res = modifyFunction(payload);
		Replace(discriminator, res);
	}
	async Task ModifyAsync<TPayload>(TDiscriminator discriminator, Func<TPayload, Task<TPayload>> modifyFunction)
		//=> Replace(discriminator, await modifyFunction(GetPayload<TPayload>(discriminator)));
	{
		var payload = GetPayload<TPayload>(discriminator) ?? throw new ArgumentException($"ooooooooooooooooooooooooooooo");
		var res = await modifyFunction(payload);
		Replace(discriminator, res);
	}
}

/*
Casos de uso:
- Serialización
- Crear un Pod, luego pasarlo JsonPod, de ahí a BsonPod, ZipPod, EncryptedPod, etc.
- Recibir algo como IPod y determinar de que tipo de Pod se trata
- Recibir un Pod y poder leer cualquier Header del tipo que sea
- Cabeceras con herencia en los Payloads
*/
// public class Format1Pod : IPod<string,string>
// {
// 	public IPodCollection<string, IPod<string, object,string>> Headers { get; }
// 	public string Discriminator { get; }
// 	public string? Payload { get; }
// 	public T? As<T>() => throw new NotImplementedException();
// 	public bool TryAs<T>(out T? payload) => throw new NotImplementedException();
// 	public bool Is<T>() => throw new NotImplementedException();
// 	//public TPod ToPod<TPod>() => throw new NotImplementedException();
// }
// public class Format2Pod : IPod<string,string>
// {
// 	public IPodCollection<string> Headers { get; }
// 	public string Discriminator { get; }
// 	public string? Payload { get; }
// 	public T? As<T>() => throw new NotImplementedException();
// 	public bool TryAs<T>(out T? payload) => throw new NotImplementedException();
// 	public bool Is<T>() => throw new NotImplementedException();
// 	//public TPod ToPod<TPod>() => throw new NotImplementedException();
// }
// public class Test
// {
// 	public void ManagerPod()
// 	{
// 		
// 	}
// 	public void ManageHeaders()
// 	{
// 		IPod<string,string> f1pod = new Format1Pod();
// 		f1pod.Headers.Add("date", DateTime.Now);
// 		f1pod.Headers.Modify<DateTime>("date", _ => DateTime.Now);
// 		f1pod.Headers.Add(new Format1Pod());
// 		var f2pod = f1pod.ToPod<Format2Pod>();
// 	}
// }
