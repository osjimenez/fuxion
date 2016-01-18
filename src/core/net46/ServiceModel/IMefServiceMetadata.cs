using System;
using System.Collections.Generic;

namespace Fuxion.ServiceModel
{
	public interface IMefServiceMetadata
	{
		string Name { get; }
		Type ServiceType { get; }
		IEnumerable<Type> ServiceContracts { get; }
	}
}
