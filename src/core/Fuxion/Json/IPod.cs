using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Fuxion.Json;

public interface IPod<out TDiscriminator, out TOutside, out TInside>
{
	TDiscriminator Discriminator { get; }
	TOutside Outside();
	TInside Inside();
	// T? As<T>();
	// bool TryAs<T>([NotNullWhen(true)] out T? payload);
	// bool Is<T>();
}
public interface IPod<out TDiscriminator, out TOutside, out TInside, out TCollection> : IPod<TDiscriminator, TOutside, TInside>
	where TCollection : IPodCollection<TDiscriminator, IPod<TDiscriminator, object, object, TCollection>>
{
	TCollection Headers { get; }
}
public interface IPodCollection<TDiscriminator, out TPod> : IEnumerable<TPod>
	//where TPod : IPod<TDiscriminator, object, object, IPodCollection<TDiscriminator, TPod>>
	//where TPod : IPod<TDiscriminator, object, object, object>
	where TPod : IPod<TDiscriminator, object, object>
	//where TPodCollectionEntry : IPodCollectionEntry<TDiscriminator>
{
	bool Has(TDiscriminator discriminator);
	TPod this[TDiscriminator discriminator] { get; }
	void Add(IPod<TDiscriminator, object, object> pod);
	void Remove(TDiscriminator discriminator);
	
	
	
	
	// void Add<TOutside, TInside>(TDiscriminator discriminator, TOutside outside);
	// void Add<TOutside, TInside>(TDiscriminator discriminator, TInside inside);
	// void AddByOutside<TOutside>(TDiscriminator discriminator, TOutside outside);
	// void AddByInside<TInside>(TDiscriminator discriminator, TInside inside);
	// void Replace<TOutside>(TDiscriminator discriminator, TOutside payload);
	//
	// TPod? GetPod<TOutside>(TDiscriminator discriminator);
	// bool TryGetPod<TOutside>(TDiscriminator discriminator, [NotNullWhen(true)] out TPod? pod);
	// TOutside? GetPayload<TOutside>(TDiscriminator discriminator);
	// bool TryGetPayload<TOutside>(TDiscriminator discriminator, [NotNullWhen(true)] out TOutside? payload);
	
	// IMPLEMENTED
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	// void Modify<TOutside>(TDiscriminator discriminator, Func<TOutside, TOutside> modifyFunction)
	// {
	// 	var payload = GetPayload<TOutside>(discriminator) ?? throw new ArgumentException($"ooooooooooooooooooooooooooooo");
	// 	var res = modifyFunction(payload);
	// 	Replace(discriminator, res);
	// }
	// async Task ModifyAsync<TOutside>(TDiscriminator discriminator, Func<TOutside, Task<TOutside>> modifyFunction)
	// {
	// 	var payload = GetPayload<TOutside>(discriminator) ?? throw new ArgumentException($"ooooooooooooooooooooooooooooo");
	// 	var res = await modifyFunction(payload);
	// 	Replace(discriminator, res);
	// }
}
public class Pod<TDiscriminator> : IPod<TDiscriminator, object, object,PodCollection<TDiscriminator>>
	where TDiscriminator : notnull
{
	public Pod(TDiscriminator discriminator, object @object)
	{
		Discriminator = discriminator;
		_object = @object;
	}
	object _object;
	public TDiscriminator Discriminator { get; }
	public object Outside() => _object;
	public object Inside() => _object;
	public PodCollection<TDiscriminator> Headers { get; } = new();
}

public class Pod<TDiscriminator, TOutside, TInside> : Pod<TDiscriminator>,IPod<TDiscriminator, TOutside, TInside,PodCollection<TDiscriminator>>
	where TDiscriminator : notnull
	where TInside : notnull
{
	public Pod(TDiscriminator discriminator, TInside @object) : base(discriminator, @object) { }
	// public static implicit operator TInside(Pod<TDiscriminator, TOutside, TInside> pod) => pod.Inside();
	// public static implicit operator TOutside(Pod<TDiscriminator, TOutside, TInside> pod) => pod.Outside();
	public new TOutside Outside() => (TOutside)base.Outside();
	public new TInside Inside() => (TInside)base.Inside();
}
public class PodCollection<TDiscriminator>:IPodCollection<TDiscriminator,Pod<TDiscriminator>>
	where TDiscriminator : notnull
{
	readonly Dictionary<TDiscriminator, IPod<TDiscriminator, object, object>> dic = new();
	public bool Has(TDiscriminator discriminator) => dic.ContainsKey(discriminator);
	public Pod<TDiscriminator> this[TDiscriminator discriminator] => new(discriminator, dic[discriminator].Outside());
	public void Add(IPod<TDiscriminator, object, object> pod) => dic.Add(pod.Discriminator, pod);
	public void Remove(TDiscriminator discriminator) => dic.Remove(discriminator);
	public IEnumerator<Pod<TDiscriminator>> GetEnumerator() => dic.Values.Select(_ => new Pod<TDiscriminator>(_.Discriminator, _.Outside())).GetEnumerator();
}
/*
Casos de uso:
- Serialización
- Crear un Pod, luego pasarlo JsonPod, de ahí a BsonPod, ZipPod, EncryptedPod, etc.
- Recibir algo como IPod y determinar de que tipo de Pod se trata
- Recibir un Pod y poder leer cualquier Header del tipo que sea
- Herencia en los Payloads y como afecta eso a los discriminadores
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
