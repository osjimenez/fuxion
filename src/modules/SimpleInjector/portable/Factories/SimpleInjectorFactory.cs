using System;
using System.Collections.Generic;
using SimpleInjector;
using System.Reflection;
using Fuxion.Notifications;
using System.Linq;

namespace Fuxion.Factories
{
	public class SimpleInjectorFactory : IFactory
	{
		public SimpleInjectorFactory(Container container) { this._container = container; }
		readonly Container _container;
		public object Create(Type type)
		{
			return _container.GetInstance(type);
		}
        public IEnumerable<object> GetAllInstances(Type type)
        {
            //if (type.GetTypeInfo().IsGenericTypeDefinition)
            //{
            //    var res = new List<object>();
            //    foreach (var reg in _container.GetCurrentRegistrations())
            //    {
            //        if (reg.ServiceType.GetTypeInfo().IsGenericType)
            //        {
            //            if (reg.ServiceType.GetGenericTypeDefinition() == type)
            //            {
            //                res.Add(reg.GetInstance());
            //            }
            //        }
            //    }
            //    return res;
            //}
            //else return _container.GetAllInstances(type);
            return type.GetTypeInfo().IsGenericTypeDefinition
                ? _container.GetCurrentRegistrations()
                    .Where(r => r.ServiceType.GetTypeInfo().IsGenericType && r.ServiceType.GetGenericTypeDefinition() == type)
                    .Select(r => r.GetInstance())
                : _container.GetAllInstances(type);
        }
	}
}
