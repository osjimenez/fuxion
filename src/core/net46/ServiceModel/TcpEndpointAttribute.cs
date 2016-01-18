using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Fuxion.ServiceModel
{
    public class TcpEndpointAttribute : MefEndpointAttribute
    {
        public TcpEndpointAttribute(int port)
            : base(port)
        {
            //Seteo los valores por defecto
            MaxBufferPoolSize = 524288;
            MaxBufferSize = 65536;
            MaxConnections = 10;
            MaxReceivedMessageSize = int.MaxValue;//65536;
            
            SecurityMode = SecurityMode.None;
            ClientCredentialType = MessageCredentialType.None;
            LocalServiceMaxClockSkew = 300;
            LocalClientMaxClockSkew = 300;

            ReaderQuotas_MaxStringContentLength = 8192;
            ReaderQuotas_MaxArrayLength = 16384;
        }

        public long MaxBufferPoolSize { get; set; }
        public int MaxBufferSize { get; set; }
        public int MaxConnections { get; set; }
        public long MaxReceivedMessageSize { get; set; }
        public SecurityMode SecurityMode { get; set; }
        public MessageCredentialType ClientCredentialType { get; set; }
        public uint LocalServiceMaxClockSkew { get; set; }
        public uint LocalClientMaxClockSkew { get; set; }
        public TransferMode TransferMode { get; set; }
        public int ReaderQuotas_MaxStringContentLength { get; set; }
        public int ReaderQuotas_MaxArrayLength { get; set; }
        protected override Binding CreateBinding(IMefServiceMetadata meta)
        {
            Binding res;
            var bin = new NetTcpBinding();
            bin.MaxBufferPoolSize = MaxBufferPoolSize;
            bin.MaxBufferSize = MaxBufferSize;
            bin.MaxConnections = MaxConnections;
            bin.MaxReceivedMessageSize = MaxReceivedMessageSize;
            bin.ReaderQuotas.MaxBytesPerRead = int.MaxValue;
            bin.ReaderQuotas.MaxArrayLength = int.MaxValue;
            bin.Security.Mode = SecurityMode;
            bin.Security.Message.ClientCredentialType = ClientCredentialType;
            bin.TransferMode = TransferMode;
            // ReaderQuotas
            bin.ReaderQuotas.MaxStringContentLength = ReaderQuotas_MaxStringContentLength;
            bin.ReaderQuotas.MaxArrayLength = ReaderQuotas_MaxArrayLength;

            res = bin;
            //Compruebo si los ClockSkew tienen los valores por defecto (300 segundos)
            if (LocalClientMaxClockSkew != 300 || LocalServiceMaxClockSkew != 300)
            {
                CustomBinding cusBin = new CustomBinding(bin);
                SecurityBindingElement security = cusBin.Elements.Find<SecurityBindingElement>();
                if (security != null)
                {
                    security.LocalServiceSettings.MaxClockSkew = LocalServiceMaxClockSkew == 0 ? TimeSpan.MaxValue : TimeSpan.FromSeconds(LocalServiceMaxClockSkew);
                    security.LocalClientSettings.MaxClockSkew = LocalClientMaxClockSkew == 0 ? TimeSpan.MaxValue : TimeSpan.FromSeconds(LocalClientMaxClockSkew);
                }
                res = cusBin;
            }
            return res;
        }
    }
}
