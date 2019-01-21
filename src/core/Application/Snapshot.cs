using Fuxion.Application.Snapshots;
using Fuxion.Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fuxion.Application
{
	public abstract class Snapshot<TAggregate> : Snapshot where TAggregate : Aggregate, new()
	{
		protected internal abstract void Hydrate(TAggregate aggregate);
		internal override void Load(Aggregate aggregate)
		{
			Load((TAggregate)aggregate);
		}
		protected internal abstract void Load(TAggregate aggregate);
	}
}
