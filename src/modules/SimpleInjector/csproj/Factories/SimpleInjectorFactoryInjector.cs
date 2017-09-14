using System;
using System.Collections.Generic;
using SimpleInjector;
using System.Reflection;
using System.Linq;

namespace Fuxion.Factories
{
	public class SimpleInjectorFactoryInjector : IFactoryInjector
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
	}
}
