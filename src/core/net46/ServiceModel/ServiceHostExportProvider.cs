using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using Fuxion.ServiceModel.Description;
using Fuxion.Factories;

namespace Fuxion.ServiceModel
{
	public class MefServiceHostExportProvider : ExportProvider
	{
		private static readonly string MatchContractName = AttributedModelServices.GetContractName(typeof(MefServiceHost));
        //CompositionContainer Container { get { return Singleton.Get<CompositionContainer>(); } }
        // TODO - Oscar - Comprobar que la factoria funciona aqui
        CompositionContainer Container { get { return Factory.Get<CompositionContainer>(); } }

		protected override IEnumerable<Export> GetExportsCore(ImportDefinition importDefinition, AtomicComposition composition)
		{


			if (Container == null) return Enumerable.Empty<Export>();

			if (importDefinition.ContractName.Equals(MatchContractName))
			{
				var exports = Container.GetExports<IMefService, IMefServiceMetadata>();

				switch (importDefinition.Cardinality)
				{
					case ImportCardinality.ExactlyOne:
						{
							var export = exports.Single();
							return new[] { CreateExport(export.Metadata) };
						}
					case ImportCardinality.ZeroOrOne:
						{
							var export = exports.SingleOrDefault();
							return (export == null)
									   ? Enumerable.Empty<Export>()
									   : new[] { CreateExport(export.Metadata) };
						}
					case ImportCardinality.ZeroOrMore:
						{
							return exports.Select(e => CreateExport(e.Metadata));
						}
				}
			}
			return Enumerable.Empty<Export>();
		}
		private Export CreateExport(IMefServiceMetadata meta)
		{
			return new Export(MatchContractName,
				() =>
				{
					if (meta == null) throw new ArgumentNullException("meta");

					var host = new MefServiceHost(meta, new Uri[0]);
					host.Description.Behaviors.Add(new MefServiceBehavior(meta.Name));

					return host;
				});
		}
	}
}
