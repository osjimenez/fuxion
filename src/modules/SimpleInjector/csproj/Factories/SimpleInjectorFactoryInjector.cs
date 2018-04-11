using System;
using System.Collections.Generic;
using SimpleInjector;
using System.Reflection;
using System.Linq;

namespace Fuxion.Factories
{
	public class SimpleInjectorFactoryInjector : IFactoryInjector, ICheckableInjector
	{
		public SimpleInjectorFactoryInjector(Container container) { this._container = container; }
		readonly Container _container;
		public object Get(Type type)
		{
			return _container.GetInstance(type);
		}
        public IEnumerable<object> GetMany(Type type)
        {
            return type.GetTypeInfo().IsGenericTypeDefinition
                ? _container.GetCurrentRegistrations()
                    .Where(r => r.ServiceType.GetTypeInfo().IsGenericType && r.ServiceType.GetGenericTypeDefinition() == type)
                    .Select(r => r.GetInstance())
                : _container.GetAllInstances(type);
        }

		public bool CheckGet(Type type)
		{
			return _container.GetRegistration(type) != null;
		}

		public bool CheckGetMany(Type type)
		{
			return _container.GetRegistration(typeof(IEnumerable<>).MakeGenericType(type)) != null;
		}
	}
}
