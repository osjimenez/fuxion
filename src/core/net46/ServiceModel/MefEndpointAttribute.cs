using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace Fuxion.ServiceModel
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public abstract class MefEndpointAttribute : MefServiceAttribute
	{
		protected MefEndpointAttribute(int port)
		{
			Port = port;
			CloseTimeout = 60;
			OpenTimeout = 60;
			ReceiveTimeout = 600;
			SendTimeout = 60;
		}
		public string Path { get; set; }
		public int Port { get; set; }
		//Todos los valores de Timeout estan en segundos
		public uint CloseTimeout { get; set; }
		public uint OpenTimeout { get; set; }
		public uint ReceiveTimeout { get; set; }
		public uint SendTimeout { get; set; }

		protected internal virtual ServiceEndpoint CreateEndpoint(ContractDescription description, IMefServiceMetadata meta)
		{
			var bin = CreateBinding(meta);
			bin.CloseTimeout = CloseTimeout == 0 ? TimeSpan.MaxValue : TimeSpan.FromSeconds(CloseTimeout);
			bin.OpenTimeout = OpenTimeout == 0 ? TimeSpan.MaxValue : TimeSpan.FromSeconds(OpenTimeout);
			bin.ReceiveTimeout = ReceiveTimeout == 0 ? TimeSpan.MaxValue : TimeSpan.FromSeconds(ReceiveTimeout);
			bin.SendTimeout = SendTimeout == 0 ? TimeSpan.MaxValue : TimeSpan.FromSeconds(SendTimeout);
			var uri = CreateUri(bin.Scheme, meta);
			var address = new EndpointAddress(uri);
			return new ServiceEndpoint(description, bin, address);
		}
		protected abstract Binding CreateBinding(IMefServiceMetadata meta);
		protected virtual Uri CreateUri(string scheme, IMefServiceMetadata meta)
		{
			var builder = new UriBuilder(scheme, "localhost", Port, Path ?? meta.Name);
			return builder.Uri;
		}
	}
}
