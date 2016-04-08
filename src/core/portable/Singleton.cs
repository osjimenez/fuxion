using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion
{
    public class Singleton
    {
        #region Singleton patern
        private static Singleton _instance;
        private static readonly object lockObject = new object();
        static Singleton Instance
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
        readonly Dictionary<SingletonKey, object> objects = new Dictionary<SingletonKey, object>();
        #region Add
        public static void Add<T>() where T : new() { Add(new T(), SingletonKey.GetKey<T>()); }
        public static void Add<T>(T objectInstance) { Add(objectInstance, SingletonKey.GetKey<T>()); }
        public static void Add<T>(T objectInstance, object key) { Add(objectInstance, SingletonKey.GetKey<T>(key)); }
        private static void Add<T>(T objectInstance, SingletonKey key)
        {
            if (Instance.objects.ContainsKey(key)) throw new ArgumentException("No se puede agregar el objeto porque la combinación clave/tipo esta en uso");
            Instance.objects.Add(key, objectInstance);
            foreach (var sub in Instance.subscriptions)
                if (sub.Type == ((object)objectInstance).GetType() && sub.Key == key) sub.Invoke(default(T), objectInstance, SingletonAction.Add);
        }
        #endregion
        #region Remove
        public static bool Remove<T>() { return Remove<T>(SingletonKey.GetKey<T>()); }
        public static bool Remove<T>(object key) { return Remove<T>(SingletonKey.GetKey<T>(key)); }
        private static bool Remove<T>(SingletonKey key)
        {
            if (!Instance.objects.ContainsKey(key)) return false;
            var obj = Instance.objects[key];
            var res = Instance.objects.Remove(key);
            if (!res) return false;
            foreach (var sub in Instance.subscriptions.Where(sub => sub.Type == obj.GetType() && sub.Key == key))
                sub.Invoke(obj, default(T), SingletonAction.Remove);
            return true;
        }
        #endregion
        #region Contains
        public static bool Contains<T>() { return Contains<T>(SingletonKey.GetKey<T>()); }
        public static bool Contains<T>(object key) { return Contains<T>(SingletonKey.GetKey<T>(key)); }
        private static bool Contains<T>(SingletonKey key) { return Instance.objects.ContainsKey(key); }
        #endregion
        #region Get
        public static T Get<T>() { return Get<T>(null); }
        public static T Get<T>(object key, bool throwExceptionIfNotFind = false)
        {
            var res = Get(SingletonKey.GetKey<T>(key));
            if (res != null) return (T)res;
            if (throwExceptionIfNotFind) throw new KeyNotFoundException("No se ha encontrado el objecto de tipo '" + typeof(T).Name + " con la clave '" + (key ?? "null") + "'.");
            return default(T);
        }
        private static object Get(SingletonKey key)
        {
            return Instance.objects.ContainsKey(key) ? Instance.objects[key] : null;
        }
        #endregion
        #region Set
        public static bool Set<T>(T substitute) { return Set<T>(SingletonKey.GetKey<T>(), substitute); }
        public static bool Set<T>(T substitute, object key) { return Set<T>(SingletonKey.GetKey<T>(key), substitute); }
        //public static bool Set(Type type, object key, object substitute) { return Set(SingletonKey.GetKey(type, key), substitute); }
        private static bool Set<T>(SingletonKey key, T substitute)
        {
            if (Instance.objects.ContainsKey(key))
            {
                var previous = (T)Instance.objects[key];
                Instance.objects[key] = substitute;
                foreach (var sub in Instance.subscriptions)
                    if (sub.Type == substitute.GetType() && sub.Key == key) sub.Invoke(previous, substitute, SingletonAction.Set);
                return true;
            }
            return false;
        }
        #endregion
        #region Subscriptions
        readonly List<SubscriptionItem> subscriptions = new List<SubscriptionItem>();
        //public static void Subscribe<T>(Action<T> changeAction) { Subscribe<T>(changeAction, SingletonKey.GetKey<T>()); }
        //public static void Subscribe<T>(Action<T> changeAction, object key) { Subscribe<T>(changeAction, SingletonKey.GetKey<T>(key)); }
        //private static void Subscribe<T>(Action<T> changeAction, SingletonKey key)
        //{
        //    Instance.subscriptions.Add(new SubscriptionItem { Type = typeof(T), Key = key, Action = changeAction });
        //    if (Instance.objects.ContainsKey(key)) changeAction((T)Instance.objects[key]);
        //}

        public static void Subscribe<T>(Action<SingletonSubscriptionArgs<T>> changeAction, bool raiseAddIfAlreadyAdded = true) { Subscribe<T>(changeAction, SingletonKey.GetKey<T>()); }
        public static void Subscribe<T>(Action<SingletonSubscriptionArgs<T>> changeAction, object key, bool raiseAddIfAlreadyAdded = true) { Subscribe<T>(changeAction, SingletonKey.GetKey<T>(key)); }
        private static void Subscribe<T>(Action<SingletonSubscriptionArgs<T>> changeAction, SingletonKey key, bool raiseAddIfAlreadyAdded = true)
        {
            Instance.subscriptions.Add(new SubscriptionItem { Type = typeof(T), Key = key, Action = changeAction });
            if (raiseAddIfAlreadyAdded && Instance.objects.ContainsKey(key)) changeAction(new SingletonSubscriptionArgs<T>(default(T), (T)Instance.objects[key], SingletonAction.Add));
        }
        #endregion
        class SingletonKey
        {
            private SingletonKey(Type type, object key)
            {
                if (type == null) throw new ArgumentNullException("type", "El tipo no puede ser null");
                Type = type; Key = key;
            }

            public Type Type { get; set; }
            public object Key { get; set; }

            public override int GetHashCode()
            {
                if (Key == null) return Type.GetHashCode();
                return Type.GetHashCode() | Key.GetHashCode();
            }
            public override bool Equals(object obj)
            {
                if (obj is SingletonKey)
                {
                    var key = obj as SingletonKey;
                    return key.Type == Type && key.Key == Key;
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
            public static bool operator ==(SingletonKey key1, SingletonKey key2) { return Compare(key1, key2); }
            public static bool operator !=(SingletonKey key1, SingletonKey key2) { return !Compare(key1, key2); }

            public static SingletonKey GetKey<T>(object key) { return GetKey(typeof(T), key); }
            public static SingletonKey GetKey<T>() { return GetKey(typeof(T), null); }
            public static SingletonKey GetKey(Type type, object key) { return new SingletonKey(type, key); }
        }
        class SubscriptionItem
        {
            public Type Type { get; set; }
            public SingletonKey Key { get; set; }
            public object Action { get; set; }
            public void Invoke<T>(T previousValue, T actualValue, SingletonAction action)
            {
                ((Delegate)Action).DynamicInvoke(new SingletonSubscriptionArgs<T>(previousValue, actualValue, action));
            }
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
}
