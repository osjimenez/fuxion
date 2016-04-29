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
            List<Exception> exceptions = new List<Exception>();
            // TODO - Oscar - Collect all factory exceptions and if no factory can create the instance, can return a good documented aggregateexception or similar
            foreach (var fac in _pipe)
            {
                try
                {
                    if (fac is ICheckableFactory)
                    {
                        if ((fac as ICheckableFactory).CheckGet(type))
                            return fac.Get(type);
                    }
                    else return fac.Get(type);
                }
                catch (Exception ex)
                {
                    exceptions.Add(new FactoryCreationException($"Error creating type '{type.GetSignature(false)}' in factory '{fac.GetType().GetSignature(false)}'", ex));
                }
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
                    catch (Exception ex)
                    {
                        exceptions.Add(new FactoryCreationException($"Error creating interface '{ti.GetType().GetSignature(false)}' using default implementation of type '{att.Type.GetSignature(false)}'", ex));
                    }
            }
            if (createDefaultInstanceIfAllFactoriesFail)
                try
                {
                    return Activator.CreateInstance(type);
                }
                catch (Exception ex)
                {
                    exceptions.Add(new FactoryCreationException($"Error creating default instance of type '{type.GetSignature(false)}'", ex));
                }
            if (res == null)
            {
                if (exceptions.Any())
                    throw new FactoryCreationException($"Factory cannot create instance of type '{type.Name}'", new AggregateException(exceptions));
                else
                    throw new FactoryCreationException($"Factory cannot create instance of type '{type.Name}'");
            }
            return res;
        }
        public static IEnumerable<T> GetMany<T>(bool concatAllPipeResults = false)
        {
            return _pipe.SelectMany(fac => fac.GetMany(typeof(T)).Cast<T>());
        }
        public static IEnumerable<object> GetMany(Type type, bool concatAllPipeResults = false)
        {
            List<IEnumerable<object>> res = new List<IEnumerable<object>>();
            List<Exception> exceptions = new List<Exception>();
            foreach (var fac in _pipe)
            {
                try
                {
                    if (fac is ICheckableFactory)
                    {
                        if ((fac as ICheckableFactory).CheckGet(type))
                            res.Add(fac.GetMany(type));
                    }
                    else res.Add(fac.GetMany(type));
                }
                catch (Exception ex)
                {
                    exceptions.Add(new FactoryCreationException($"Error creating many of type '{type.GetSignature(false)}' in factory '{fac.GetType().GetSignature(false)}'", ex));
                }
            }
            if (!res.Any())
            {
                if (exceptions.Any())
                    throw new FactoryCreationException($"Cannot create many instances of type '{type.Name}'", new AggregateException(exceptions));
                else
                    throw new FactoryCreationException($"Cannot create many instances of type '{type.Name}'");
            }
            return res.SelectMany(r => r);

        }
	}
}
