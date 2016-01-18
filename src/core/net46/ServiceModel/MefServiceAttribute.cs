using Fuxion.Factories;
using System;
using System.ComponentModel.Composition.Hosting;

namespace Fuxion.ServiceModel
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public abstract class MefServiceAttribute : Attribute
	{
        //internal protected CompositionContainer Container { get { return Singleton.Get<CompositionContainer>(); } }
        // TODO - Oscar - Comprobar que la factoria funciona aqui
        internal protected CompositionContainer Container { get { return Factory.Create<CompositionContainer>(); } }
	}
}
