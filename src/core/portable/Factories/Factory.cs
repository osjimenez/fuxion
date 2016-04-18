using Fuxion.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
namespace Fuxion.Factories
{
    public static class Factory
    {
        private static ImmutableList<IFactory> _pipe = new IFactory[] { }.ToImmutableList();
        public static bool ReturnDefaultValueIfCanNotBeCreated { get; set; } = true;
        public static void AddToPipe(IFactory factory) { _pipe = _pipe.Add(factory); }
        public static void RemoveFromPipe(IFactory factory) { _pipe = _pipe.Remove(factory); }
        public static void InsertToPipe(int index, IFactory factory) { _pipe = _pipe.Insert(index, factory); }
        public static void ClearPipe() { _pipe = _pipe.Clear(); }
        public static T Get<T>(bool createDefaultInstanceIfAllFactoriesFail = true) { return (T)Get(typeof(T), createDefaultInstanceIfAllFactoriesFail); }
        public static object Get(Type type, bool createDefaultInstanceIfAllFactoriesFail = true)
        {
            object res = null;
            // TODO - Oscar - Collect all factory exceptions and if no factory can create the instance, can return a good documented aggregateexception or similar
            foreach (var fac in _pipe)
            {
                try
                {
                    return fac.Get(type);
                }
                catch { }
            }

            var ti = type.GetTypeInfo();
            if (ti.IsInterface)
            {
                var att = ti.GetCustomAttribute<FactoryDefaultImplementationAttribute>(false, false);
                if (att != null)
                    try
                    {
                        return Activator.CreateInstance(att.Type);
                    }
                    catch { }
            }
            if (createDefaultInstanceIfAllFactoriesFail)
                try
                {
                    return Activator.CreateInstance(type);
                }
                catch { }
            if (res == null)
                throw new FactoryCreationException($"Cannot create instance of type '{type.Name}'");
            return res;
        }
        public static IEnumerable<T> GetMany<T>() { return _pipe.SelectMany(fac => fac.GetMany(typeof(T)).Cast<T>()); }
        public static IEnumerable<object> GetMany(Type type) { return _pipe.SelectMany(fac => fac.GetMany(type)); }
	}
}
