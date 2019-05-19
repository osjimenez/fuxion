using Fuxion.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fuxion
{
	public class Singleton
	{
		#region Singleton patern
		private static Singleton _instance;
		private static readonly object lockObject = new object();

		private static Singleton Instance
		{
			get
			{
				lock (lockObject)
				{
					if (_instance == null) _instance = new Singleton();
					return _instance;
				}
			}
		}
		private Singleton() { }
		#endregion
		private readonly Locker<Dictionary<SingletonKey, object?>> objects = new Locker<Dictionary<SingletonKey, object?>>(new Dictionary<SingletonKey, object?>());
		#region Add
		public static void Add<T>() where T : new() => Add(new T(), SingletonKey.GetKey<T>());
		public static void Add<T>(T objectInstance) => Add(objectInstance, SingletonKey.GetKey<T>());
		public static void Add<T>(T objectInstance, object key) => Add(objectInstance, SingletonKey.GetKey<T>(key));
		private static void Add<T>(T objectInstance, SingletonKey key)
		{
			Instance.objects.Write(_ =>
			{
				if (_.ContainsKey(key)) throw new ArgumentException("No se puede agregar el objeto porque la combinación clave/tipo esta en uso");
				_.Add(key, objectInstance);
			});
			//if (Instance.objects.ContainsKey(key)) throw new ArgumentException("No se puede agregar el objeto porque la combinación clave/tipo esta en uso");
			//Instance.objects.Add(key, objectInstance);
			foreach (var sub in Instance.subscriptions.Where(sub => sub.Type == objectInstance?.GetType() && sub.Key == key))
				sub.Invoke(default!, objectInstance, SingletonAction.Add);
		}
		public static void AddOrSkip<T>() where T : new() => AddOrSkip(new T(), SingletonKey.GetKey<T>());
		public static void AddOrSkip<T>(T objectInstance) => AddOrSkip(objectInstance, SingletonKey.GetKey<T>());
		public static void AddOrSkip<T>(T objectInstance, object key) => AddOrSkip(objectInstance, SingletonKey.GetKey<T>(key));
		private static void AddOrSkip<T>(T objectInstance, SingletonKey key)
		{
			var added = Instance.objects.Write(_ =>
			{
				if (!_.ContainsKey(key))
				{
					_.Add(key, objectInstance);
					return true;
				}
				return false;
			});
			if (added)
				foreach (var sub in Instance.subscriptions.Where(sub => sub.Type == objectInstance?.GetType() && sub.Key == key))
					sub.Invoke(default!, objectInstance, SingletonAction.Add);
		}
		#endregion
		#region Remove
		public static bool Remove<T>() => Remove<T>(SingletonKey.GetKey<T>());
		public static bool Remove<T>(object key) => Remove<T>(SingletonKey.GetKey<T>(key));
		private static bool Remove<T>(SingletonKey key)
		{
			var (value, result) = Instance.objects.Write(_ =>
			{
				if (!_.ContainsKey(key)) return (null, false);
				return (_[key], _.Remove(key));
			});
			if (!result) return false;
			foreach (var sub in Instance.subscriptions.Where(sub => sub.Type == value?.GetType() && sub.Key == key))
				sub.Invoke(value, default, SingletonAction.Remove);
			return true;
		}
		#endregion
		#region Contains
		public static bool Contains<T>() => Contains<T>(SingletonKey.GetKey<T>());
		public static bool Contains<T>(object key) => Contains<T>(SingletonKey.GetKey<T>(key));
		private static bool Contains<T>(SingletonKey key) => Instance.objects.Read(_ => _.ContainsKey(key));
		#endregion
		#region Get
		public static T Get<T>() => Get<T>(null);
		public static T Get<T>(object? key, bool throwExceptionIfNotFound = false)
		{
			var res = Get(SingletonKey.GetKey<T>(key), typeof(T));
			if (res != null) return (T)res;
			if (throwExceptionIfNotFound) throw new KeyNotFoundException("No se ha encontrado el objecto de tipo '" + typeof(T).Name + " con la clave '" + (key ?? "null") + "'.");
			return default!;
		}
		private static object? Get(SingletonKey key, Type requestedType)
			=> Instance.objects.ReadUpgradeable(_ =>
			{
				if (_.ContainsKey(key))
					return _[key];
				var att = requestedType.GetCustomAttribute<DefaultSingletonInstanceAttribute>(true, false, false);
				if (att != null)
				{
					var ins = Activator.CreateInstance(att.Type);
					Add(ins, key);
					return ins;
				}
				return _[key];
			});
		
		#endregion
		#region Find
		public static T Find<T>() => Find<T>(null);
		public static T Find<T>(object? key, bool throwExceptionIfNotFind = false)
		{
			var res = Find(SingletonKey.GetKey<T>(key), typeof(T));
			if (res != null) return (T)res;
			if (throwExceptionIfNotFind) throw new KeyNotFoundException("No se ha encontrado el objecto de tipo '" + typeof(T).Name + " con la clave '" + (key ?? "null") + "'.");
			return default!;
		}
		private static object? Find(SingletonKey key, Type requestedType)
			//=> Instance.objects.Read(_ => _.ContainsKey(key) ? _[key] : null);
			=> Instance.objects.ReadUpgradeable(_ =>
			{
				if (_.ContainsKey(key))
					return _[key];
				var att = requestedType.GetCustomAttribute<DefaultSingletonInstanceAttribute>(true, false, false);
				if (att != null)
				{
					var ins = Activator.CreateInstance(att.Type);
					Add(ins, key);
					return ins;
				}
				return null;
			});
		#endregion
		#region Set
		public static bool Set<T>(T substitute) => Set<T>(SingletonKey.GetKey<T>(), substitute);
		public static bool Set<T>(T substitute, object key) => Set<T>(SingletonKey.GetKey<T>(key), substitute);
		private static bool Set<T>(SingletonKey key, T substitute)
		{
			var (previous, setted) = Instance.objects.Write(_ =>
			{
				if (_.ContainsKey(key))
				{
					var previous = (T)_[key]!;
					_[key] = substitute;
					return (previous, true);
				}
				return (default(T)!, false);
			});
			if (setted)
			{
				foreach (var sub in Instance.subscriptions)
					if (sub.Type == typeof(T) && sub.Key == key)
						sub.Invoke(previous, substitute, SingletonAction.Set);
			}
			return setted;
		}
		#endregion
		#region Subscriptions
		private readonly List<SubscriptionItem> subscriptions = new List<SubscriptionItem>();
		//public static void Subscribe<T>(Action<T> changeAction) { Subscribe<T>(changeAction, SingletonKey.GetKey<T>()); }
		//public static void Subscribe<T>(Action<T> changeAction, object key) { Subscribe<T>(changeAction, SingletonKey.GetKey<T>(key)); }
		//private static void Subscribe<T>(Action<T> changeAction, SingletonKey key)
		//{
		//    Instance.subscriptions.Add(new SubscriptionItem { Type = typeof(T), Key = key, Action = changeAction });
		//    if (Instance.objects.ContainsKey(key)) changeAction((T)Instance.objects[key]);
		//}

		public static void Subscribe<T>(Action<SingletonSubscriptionArgs<T>> changeAction, bool raiseAddIfAlreadyAdded = true) => Subscribe<T>(changeAction, SingletonKey.GetKey<T>(), raiseAddIfAlreadyAdded);
		public static void Subscribe<T>(Action<SingletonSubscriptionArgs<T>> changeAction, object key, bool raiseAddIfAlreadyAdded = true) => Subscribe<T>(changeAction, SingletonKey.GetKey<T>(key), raiseAddIfAlreadyAdded);
		private static void Subscribe<T>(Action<SingletonSubscriptionArgs<T>> changeAction, SingletonKey key, bool raiseAddIfAlreadyAdded = true)
		{
			Instance.subscriptions.Add(new SubscriptionItem(typeof(T), key, changeAction));
			Instance.objects.Read(_ =>
			{
				if (raiseAddIfAlreadyAdded && _.ContainsKey(key))
					changeAction(new SingletonSubscriptionArgs<T>(default!, (T)_[key]!, SingletonAction.Add));
			});
		}
		#endregion
		#region Constants
		public static ISingletonConstants Constants => default!;
		#endregion
		private class SingletonKey
		{
			private SingletonKey(Type type, object? key)
			{
				if (type == null) throw new ArgumentNullException("type", "El tipo no puede ser null");
				Type = type; Key = key;
			}

			public Type Type { get; set; }
			public object? Key { get; set; }

			public override int GetHashCode()
			{
				if (Key == null) return Type.GetHashCode();
				return Type.GetHashCode() | Key.GetHashCode();
			}
			public override bool Equals(object obj)
			{
				if (obj is SingletonKey key)
				{
					return key.Type == Type && (key.Key == null || (key.Key != null && key.Key.Equals(Key)));
				}
				return false;
			}
			private static bool Compare<T>(T t1, T t2)
			{
				return
					(t1 == null && t2 == null)
					||
					(t1 != null && t1.Equals(t2));
			}
			public static bool operator ==(SingletonKey key1, SingletonKey key2) => Compare(key1, key2);
			public static bool operator !=(SingletonKey key1, SingletonKey key2) => !Compare(key1, key2);

			public static SingletonKey GetKey<T>(object? key) => GetKey(typeof(T), key);
			public static SingletonKey GetKey<T>() => GetKey(typeof(T), null);
			public static SingletonKey GetKey(Type type, object? key) => new SingletonKey(type, key);
		}

		private class SubscriptionItem
		{
			public SubscriptionItem(Type type, SingletonKey key, Delegate action)
			{
				Type = type;
				Key = key;
				Action = action;
			}
			public Type Type { get; set; }
			public SingletonKey Key { get; set; }
			public Delegate Action { get; set; }
			public void Invoke<T>(T previousValue, T actualValue, SingletonAction action) => Action.DynamicInvoke(new SingletonSubscriptionArgs<T>(previousValue, actualValue, action));
		}
	}
	public enum SingletonAction
	{
		Add,
		Remove,
		Set
	}
	public class SingletonSubscriptionArgs<T>
	{
		internal SingletonSubscriptionArgs(T previousValue, T actualValue, SingletonAction action)
		{
			PreviousValue = previousValue;
			ActualValue = actualValue;
			Action = action;
		}
		public SingletonAction Action { get; private set; }
		public T PreviousValue { get; set; }
		public T ActualValue { get; set; }
	}
	public interface ISingletonConstants
	{
		void Repair();
	}
	public class DefaultSingletonInstanceAttribute : Attribute
	{
		public DefaultSingletonInstanceAttribute(Type type) => Type = type;
		public Type Type { get; set; }
	}
}