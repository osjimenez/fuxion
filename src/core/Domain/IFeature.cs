using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Fuxion.Domain;

[EditorBrowsable(EditorBrowsableState.Never)]
public interface IFeature { }

public interface IFeature<in TFeaturizable> : IFeature where TFeaturizable : IFeaturizable<TFeaturizable>
{
	public void OnAttach(TFeaturizable featurizable)
#if NETSTANDARD2_0 || NET462
		;
#else
		{ }
#endif
	public void OnDetach(TFeaturizable featurizable)
#if NETSTANDARD2_0 || NET462
		;
#else
		{ }
#endif
}

public interface IFeaturizable<in TFeaturizable>
	where TFeaturizable : IFeaturizable<TFeaturizable>
#if NETSTANDARD2_0 || NET462
// In legacy frameworks default interface implementations is not supported
// The interface will be implemented without implmentations
{
	IFeatureCollection<TFeaturizable> Features { get; }
	public bool Has(UriKey key);
	public bool Has<TFeature>() where TFeature : IFeature;
	public void Add(Type type);
	public void Add<TFeature>(Action<TFeature>? initializeAction = null)
		where TFeature : IFeature, new();
	public bool Remove<TFeature>(bool exceptionIfNotFound = true)
		where TFeature : IFeature;
	public TFeature Get<TFeature>()
		where TFeature : IFeature;
	public bool TryGet<TFeature>([NotNullWhen(true)] out TFeature? feature)
		where TFeature : IFeature;
	public IEnumerable<TFeature> All<TFeature>()
		where TFeature : IFeature;
}
// The implemented methods will be in a class
public class Featurizable<TFeaturizable> where TFeaturizable : IFeaturizable<TFeaturizable>
#endif
{
	IFeatureCollection<TFeaturizable> Features { get; }
#if NETSTANDARD2_0 || NET462
		= new FeatureCollection<TFeaturizable>();
#endif
	public bool Has(UriKey key)
		=> Features.Has(key);
	public bool Has<TFeature>()
		where TFeature : IFeature
		=> Features.Has<TFeature>();
	public void Add(Type type)
	{
		if (Has(type.GetUriKey())) throw new FeatureAlreadyExistException($"'{GetType().GetSignature()}' object already has '{type.Name}' feature");
		var feaObj = Activator.CreateInstance(type) ?? throw new InvalidProgramException($"Feature cannot be created");
		((IFeature<TFeaturizable>)feaObj).OnAttach((TFeaturizable)
#if NETSTANDARD2_0 || NET462
			(IFeaturizable<TFeaturizable>)
#endif
			this);
		Features.Add((IFeature)feaObj);
	}
	public void Add<TFeature>(Action<TFeature>? initializeAction = null) 
		where TFeature : IFeature, new()
	{
		if (Has<TFeature>()) throw new FeatureAlreadyExistException($"'{GetType().GetSignature()}' object already has '{typeof(TFeature).Name}' feature");
		TFeature fea = new();
		((IFeature<TFeaturizable>)fea).OnAttach((TFeaturizable)
#if NETSTANDARD2_0 || NET462
			(IFeaturizable<TFeaturizable>)
#endif
			this);
		initializeAction?.Invoke(fea);
		Features.Add(fea);
	}
	public bool Remove<TFeature>(bool exceptionIfNotFound = true)
		where TFeature : IFeature
	{
		if (!Has<TFeature>() && exceptionIfNotFound) throw new FeatureNotFoundException($"Feature {typeof(TFeature).GetSignature()} was not found");
		((IFeature<TFeaturizable>)Features.Get<TFeature>()).OnDetach((TFeaturizable)
#if NETSTANDARD2_0 || NET462
			(IFeaturizable<TFeaturizable>)
#endif
			this);
		return Features.Remove(typeof(TFeature).GetUriKey());
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
	internal bool Has(UriKey key);
	internal bool Has<TFeature>() where TFeature : IFeature;
	internal void Add(IFeature feature);
	internal bool Remove(UriKey key);
	internal TFeature Get<TFeature>() where TFeature : IFeature;
	internal bool TryGet<TFeature>([NotNullWhen(true)] out TFeature? feature) where TFeature : IFeature;
	internal dynamic Get(UriKey key);
	internal IEnumerable<TFeature> All<TFeature>() where TFeature : IFeature;
#if !NETSTANDARD2_0 && !NET462
	public static IFeatureCollection<TFeaturizable> Create() => new FeatureCollection<TFeaturizable>();
#endif
}
internal class FeatureCollection<TFeaturizable> : IFeatureCollection<TFeaturizable>
	where TFeaturizable : IFeaturizable<TFeaturizable>
{
	Dictionary<UriKey, IFeature> Features { get; } = new();
	public bool Has(UriKey key)
		=> Features.ContainsKey(key);
	public bool Has<TFeature>()
		where TFeature : IFeature
		=> Features.ContainsKey(typeof(TFeature).GetUriKey());
	public void Add(IFeature feature)
		=> Features.Add(feature.GetType().GetUriKey(), feature);
	public bool Remove(UriKey key) => Features.Remove(key);
	public TFeature Get<TFeature>()
		where TFeature : IFeature
		=> ((IFeatureCollection<TFeaturizable>)this).Has<TFeature>() 
			? (TFeature)Features[typeof(TFeature).GetUriKey()]
			: throw new FeatureNotFoundException($"Feature of type '{typeof(TFeature).GetSignature()}' not found");
	
	public bool TryGet<TFeature>([NotNullWhen(true)] out TFeature? feature)
		where TFeature : IFeature
	{
		if (((IFeatureCollection<TFeaturizable>)this).Has<TFeature>())
		{
			feature = (TFeature)Features[typeof(TFeature).GetUriKey()];
			return true;
		}
		feature = default;
		return false;
	}
	public dynamic Get(UriKey key)
	{
		if (!Features.ContainsKey(key)) throw new FeatureNotFoundException($"Feature with {nameof(UriKey)} '{key}' was not found");
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
public class FeatureAlreadyExistException(string message) : FuxionException(message) { }
public class FeatureNotFoundException(string message) : FuxionException(message) { }