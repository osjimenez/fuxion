//using DynamicData;
//using Fuxion.Shell.Models;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Fuxion.Shell
//{
//	public class Cache
//	{
//		readonly ConcurrentDictionary<Type, (object SourceCache, object Comparer)> caches = new ConcurrentDictionary<Type, (object SourceCache, object Comparer)>();

//		public void Add<T>(IEqualityComparer<T> comparer) where T : ICacheable => caches.GetOrAdd(typeof(T), (new SourceCache<T, Guid>(_ => _.Id), comparer));
//		public SourceCache<T, Guid> Get<T>() where T : ICacheable => (SourceCache<T, Guid>)caches[typeof(T)].SourceCache;

//		public void EditDiff<T>(params T[] items) => EditDiff(items.AsEnumerable());
//		public void EditDiff<T>(IEnumerable<T> alltems) => caches[typeof(T)].Transform(t => ((SourceCache<T, Guid>)t.SourceCache).EditDiff(alltems, ((IEqualityComparer<T>)t.Comparer)));

//		public void AddOrUpdate<T>(T item) => ((SourceCache<T, Guid>)caches[typeof(T)].SourceCache).AddOrUpdate(item);
//	}
//	public interface ICacheable
//	{
//		Guid Id { get; }
//	}
//}
