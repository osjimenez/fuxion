using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Fuxion.ServiceModel
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true), MetadataAttribute]
	public class ExportServiceAttribute : ExportAttribute, IMefServiceMetadata
	{
		public ExportServiceAttribute(string name, Type serviceType, params Type[] serviceContracts)
			: base(typeof(IMefService))
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("El parámetro 'name' no puede ser null, vacio o espacios en blanco.", "name");

			if (serviceType == null)
				throw new ArgumentNullException("serviceType");

			if (serviceContracts == null && serviceContracts.Length > 0)
				throw new ArgumentException("El parámetro 'serviceContracts' no puede ser null ni vacio. Especifique al menos un contrato para el servicio.", "serviceContracts");

			Name = name;
			ServiceType = serviceType;
			ServiceContracts = serviceContracts;
		}

		public string Name { get; private set; }
		public Type ServiceType { get; private set; }
		public IEnumerable<Type> ServiceContracts { get; private set; }
	}
}
