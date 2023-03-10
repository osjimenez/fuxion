using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fuxion.Json;
using Fuxion.Reflection;

namespace Fuxion.Domain;

[EditorBrowsable(EditorBrowsableState.Never)]
public interface IFeature { }

public interface IFeature<in TFeaturizable> : IFeature where TFeaturizable : IFeaturizable<TFeaturizable>
{
	public void OnAttach(TFeaturizable featurizable) { }
	public void OnDetach(TFeaturizable featurizable) { }
}

public interface IFeaturizable<in TFeaturizable> where TFeaturizable : IFeaturizable<TFeaturizable>
{
	IFeatureCollection<TFeaturizable> Features { get; }
	public bool Has(TypeKey key)
		=> Features.Has(key);
	public bool Has<TFeature>()
		where TFeature : IFeature
		=> Features.Has<TFeature>();
	public void Add(Type type)
	{
		if (Has(type.GetTypeKey())) throw new FeatureAlreadyExistException($"'{GetType().GetSignature()}' object already has '{type.Name}' feature");
		var feaObj = Activator.CreateInstance(type);
		// TFeature fea = new();
		((IFeature<TFeaturizable>)feaObj).OnAttach((TFeaturizable)this);
		// initializeAction?.Invoke(fea);
		Features.Add((IFeature)feaObj);
	}
	public void Add<TFeature>(Action<TFeature>? initializeAction = null) 
		where TFeature : IFeature, new()
	{
		if (Has<TFeature>()) throw new FeatureAlreadyExistException($"'{GetType().GetSignature()}' object already has '{typeof(TFeature).Name}' feature");
		TFeature fea = new();
		((IFeature<TFeaturizable>)fea).OnAttach((TFeaturizable)this);
		initializeAction?.Invoke(fea);
		Features.Add(fea);
	}
	public bool Remove<TFeature>(bool exceptionIfNotFound = true)
		where TFeature : IFeature
	{
		if (!Has<TFeature>() && exceptionIfNotFound) throw new FeatureNotFoundException($"Feature {typeof(TFeature).GetSignature()} was not found");
		((IFeature<TFeaturizable>)Features.Get<TFeature>()).OnDetach((TFeaturizable)this);
		return Features.Remove(typeof(TFeature).GetTypeKey());
	}
	public TFeature Get<TFeature>()
		where TFeature : IFeature
		=> Features.Get<TFeature>();
	public bool TryGet<TFeature>([NotNullWhen(true)] out TFeature? feature)
		where TFeature : IFeature
		=> Features.TryGet(out feature);
	public IEnumerable<TFeature> All<TFeature>()
		where TFeature : IFeature
		=> Features.All<TFeature>();
}
public interface IFeatureCollection<in TFeaturizable> where TFeaturizable : IFeaturizable<TFeaturizable>
{
	internal bool Has(TypeKey key);
	internal bool Has<TFeature>() where TFeature : IFeature;
	internal void Add(IFeature feature);
	internal bool Remove(TypeKey key);
	internal TFeature Get<TFeature>() where TFeature : IFeature;
	internal bool TryGet<TFeature>([NotNullWhen(true)] out TFeature? feature) where TFeature : IFeature;
	internal dynamic Get(TypeKey key);
	internal IEnumerable<TFeature> All<TFeature>() where TFeature : IFeature;
	public static IFeatureCollection<TFeaturizable> Create() => new FeatureCollection<TFeaturizable>();
}
class FeatureCollection<TFeaturizable> : IFeatureCollection<TFeaturizable>
	where TFeaturizable : IFeaturizable<TFeaturizable>
{
	Dictionary<TypeKey, IFeature> Features { get; } = new();
	public bool Has(TypeKey key)
		=> Features.ContainsKey(key);
	public bool Has<TFeature>()
		where TFeature : IFeature
		=> Features.ContainsKey(typeof(TFeature).GetTypeKey());
	public void Add(IFeature feature)
		=> Features.Add(feature.GetType().GetTypeKey(), feature);
	public bool Remove(TypeKey key) => Features.Remove(key);
	public TFeature Get<TFeature>()
		where TFeature : IFeature
		=> ((IFeatureCollection<TFeaturizable>)this).Has<TFeature>() 
			? (TFeature)Features[typeof(TFeature).GetTypeKey()]
			: throw new FeatureNotFoundException($"Feature of type '{typeof(TFeature).GetSignature()}' not found");
	
	public bool TryGet<TFeature>([NotNullWhen(true)] out TFeature? feature)
		where TFeature : IFeature
	{
		if (((IFeatureCollection<TFeaturizable>)this).Has<TFeature>())
		{
			feature = (TFeature)Features[typeof(TFeature).GetTypeKey()];
			return true;
		}
		feature = default;
		return false;
	}
	public dynamic Get(TypeKey key)
	{
		if (!Features.ContainsKey(key)) throw new FeatureNotFoundException($"Feature with {nameof(TypeKey)} '{key}' was not found");
		var obj = Features[key];
		return obj;
	}
	public IEnumerable<TFeature> All<TFeature>()
		where TFeature : IFeature
		=> Features.Values.Where(_ => _ is TFeature).Cast<TFeature>();
}


public static class IFeaturizableExtensions
{
	public static IFeaturizable<T> Features<T>(this T me) where T : IFeaturizable<T> => me;
}
public class FeatureAlreadyExistException : FuxionException
{
	public FeatureAlreadyExistException(string message) : base(message) { }
}
public class FeatureNotFoundException : FuxionException
{
	public FeatureNotFoundException(string message) : base(message) { }
}

// [JsonConverter(typeof(FeaturizablePodConverterFactory))]
public class FeaturizablePod<TFeaturizable> : TypeKeyPod<TFeaturizable> where TFeaturizable : IFeaturizable<TFeaturizable>
{
	[JsonConstructor]
	protected FeaturizablePod() { }
	public FeaturizablePod(TFeaturizable payload) : base(payload)
	{
		// foreach (var feature in payload.Features().All<IFeature<TFeaturizable>>())
		// 	Headers.Add<IFeature<TFeaturizable>>(feature);
	}
}

public class FeaturizablePodJsonConverter<TFeaturizable> : JsonPodConverter<FeaturizablePod<TFeaturizable>, TypeKey, TFeaturizable> 
	where TFeaturizable : IFeaturizable<TFeaturizable>
{
	ITypeKeyResolver _typeKeyResolver;
	public FeaturizablePodJsonConverter(ITypeKeyResolver typeKeyResolver) => _typeKeyResolver = typeKeyResolver;
	public override FeaturizablePod<TFeaturizable>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var pod = base.Read(ref reader, typeToConvert, options);
		if (pod is null) return pod;
		if (pod.Payload is null) return pod;
		foreach (var header in pod.Headers)
		{
			var obj = pod.As(_typeKeyResolver[pod.Discriminator]);
			//var obj = value.Deserialize(_typeKeyResolver[key], options);
			if (obj is null) continue;
			pod.Payload.Features().Add(_typeKeyResolver[pod.Discriminator]);
			// pod.Payload.Features().Add(_typeKeyResolver[key]);
		}
		return pod;
	}
}
public class FeaturizablePodConverterFactory : JsonConverterFactory
{
	ITypeKeyResolver _typeKeyResolver;
	public FeaturizablePodConverterFactory(ITypeKeyResolver typeKeyResolver)
	{
		_typeKeyResolver = typeKeyResolver;
	}
	public override bool CanConvert(Type typeToConvert) => typeToConvert.IsSubclassOfRawGeneric(typeof(FeaturizablePod<>));
	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var types = typeToConvert.GetGenericArguments();
		var converterType = typeof(FeaturizablePodJsonConverter<>).MakeGenericType(types);
		return (JsonConverter)(Activator.CreateInstance(converterType, new object[]
		{
			_typeKeyResolver
		}) ?? throw new InvalidCastException($"'{converterType.GetSignature()}' can not be created"));
	}
}