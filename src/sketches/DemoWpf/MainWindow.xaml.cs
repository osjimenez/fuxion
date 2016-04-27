using Fuxion.ComponentModel;
using Fuxion.Factories;
using Fuxion.ServiceModel;
using Fuxion.Threading.Tasks;
using Fuxion.Windows.Threading;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Selectors;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DemoWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IFactoryServiceCallback
    {
        public MainWindow()
        {
            InitializeComponent();
            ConfigureFactory();
        }
        private void ConfigureFactory()
        {
            Container con = new Container();
            con.Register<ToInject>();
            Factory.AddToPipe(new SimpleInjectorFactory(con));
        }
        class ServiceValidator : UserNamePasswordValidator
        {
            public override void Validate(string userName, string password)
            {
            }
        }
        class CertificateValidator : X509CertificateValidator
        {
            public override void Validate(X509Certificate2 certificate) { }
        }
        public async void HostService_Click(object sender, RoutedEventArgs args)
        {
            await TaskManager.StartNew(() =>
            {
                #region Create manually
                ServiceHost ho = new ServiceHost(typeof(FactoryService));
                System.ServiceModel.Channels.Binding bin;
                var tcpBin = new NetTcpBinding();
                tcpBin.MaxBufferPoolSize = 524288;
                tcpBin.MaxBufferSize = int.MaxValue;
                tcpBin.MaxConnections = 10;
                tcpBin.MaxReceivedMessageSize = int.MaxValue;
                tcpBin.ReaderQuotas.MaxBytesPerRead = int.MaxValue;
                tcpBin.ReaderQuotas.MaxArrayLength = int.MaxValue;
                tcpBin.Security.Mode = SecurityMode.Message;
                tcpBin.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
                tcpBin.TransferMode = TransferMode.Buffered;
                // ReaderQuotas
                tcpBin.ReaderQuotas.MaxStringContentLength = 8192;
                tcpBin.ReaderQuotas.MaxArrayLength = 16384;

                bin = tcpBin;
                var LocalClientMaxClockSkew = 86400;
                var LocalServiceMaxClockSkew = 86400;
                //Compruebo si los ClockSkew tienen los valores por defecto (300 segundos)
                if (LocalClientMaxClockSkew != 300 || LocalServiceMaxClockSkew != 300)
                {
                    CustomBinding cusBin = new CustomBinding(tcpBin);
                    SecurityBindingElement security = cusBin.Elements.Find<SecurityBindingElement>();
                    if (security != null)
                    {
                        security.LocalServiceSettings.MaxClockSkew = LocalServiceMaxClockSkew == 0 ? TimeSpan.MaxValue : TimeSpan.FromSeconds(LocalServiceMaxClockSkew);
                        security.LocalClientSettings.MaxClockSkew = LocalClientMaxClockSkew == 0 ? TimeSpan.MaxValue : TimeSpan.FromSeconds(LocalClientMaxClockSkew);
                    }
                    bin = cusBin;
                }
                bin.CloseTimeout = TimeSpan.FromSeconds(60);
                bin.OpenTimeout = TimeSpan.FromSeconds(60);
                bin.ReceiveTimeout = TimeSpan.FromSeconds(600);
                bin.SendTimeout = TimeSpan.FromSeconds(60);

                ho.AddServiceEndpoint(new ServiceEndpoint(ContractDescription.GetContract(typeof(IFactoryService)),
                    bin,
                    new EndpointAddress(new Uri("net.tcp://localhost:6666/FactoryService"), new DnsEndpointIdentity("CertificateName"))));

                var cert = new X509Certificate2(new byte[] { }, "password");
                ho.Credentials.ServiceCertificate.Certificate = cert;

                ho.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = System.ServiceModel.Security.UserNamePasswordValidationMode.Custom;
                ho.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = new ServiceValidator();

                ho.Open();

                #endregion
                var host = ServiceManager
                    .Host<FactoryService>()
                    .WithEndpoint(e => e
                        .WithContractOfType<IFactoryService>()
                        .WithTcpBinding(b => b
                            .SecurityMode(SecurityMode.None)
                            .OpenTimeout(TimeSpan.FromSeconds(10))
                            .MaxBufferSize(65536)) // 65536 is default value
                        .ConfigureEndpoint(se => { })
                        .WithAddress("net.tcp://localhost:6666/FactoryService"))
                    .ConfigureHost(hos => { })
                    .Create();
                host.Open();
                Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId} - Service host opened");
            });
        }
        public void ProxyService_Click(object sender, RoutedEventArgs args)
        {
            #region Create manually
            var bin = new NetTcpBinding(SecurityMode.Message)
            {
                MaxReceivedMessageSize = int.MaxValue,
                MaxBufferSize = int.MaxValue,
                MaxBufferPoolSize = int.MaxValue,
            };

            bin.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
            bin.ReceiveTimeout = TimeSpan.MaxValue;
            bin.ReliableSession.InactivityTimeout = TimeSpan.MaxValue; // añadido porque tras un rato de inactividad me da SessionKeyExpiredException, a ver si con esto conseguimos que no expiren las claves
            bin.SendTimeout = TimeSpan.MaxValue;
            bin.ReaderQuotas.MaxBytesPerRead = int.MaxValue;
            bin.ReaderQuotas.MaxArrayLength = int.MaxValue;
            bin.ReaderQuotas.MaxStringContentLength = int.MaxValue;
            var cusBin = new CustomBinding(bin);
            var security = cusBin.Elements.Find<SecurityBindingElement>();
            if (security != null)
            {
                security.LocalServiceSettings.MaxClockSkew = TimeSpan.FromDays(1);
                security.LocalClientSettings.MaxClockSkew = TimeSpan.FromDays(1);
            }
            var fac = new DuplexChannelFactory<IFactoryService>(
                new InstanceContext(this)
                , new ServiceEndpoint(
                    ContractDescription.GetContract(typeof(IFactoryService)),
                    cusBin,
                    new EndpointAddress(
                        new Uri("net.tcp://localhost:6666/FactoryService"),
                            new DnsEndpointIdentity("CertificateName"))));
            fac.Credentials.ServiceCertificate.Authentication.CertificateValidationMode =
                System.ServiceModel.Security.X509CertificateValidationMode.Custom;
            fac.Credentials.ServiceCertificate.Authentication.CustomCertificateValidator =
                new CertificateValidator();
            fac.Credentials.UserName.UserName = "username";
            fac.Credentials.UserName.Password = "password";
            fac.Endpoint.Binding.OpenTimeout = TimeSpan.FromSeconds(3);

            foreach (var op in fac.Endpoint.Contract.Operations)
            {
                var dataContractBehavior =
                    op.Behaviors.Find<DataContractSerializerOperationBehavior>() as
                        DataContractSerializerOperationBehavior;
                if (dataContractBehavior != null)
                {
                    dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
                }
            }
            var ch = fac.CreateChannel();
            #endregion
            var proxy = ServiceManager
                .Proxy<IFactoryService>(this)
                .WithTcpBinding(b => b
                    .SecurityMode(SecurityMode.None)
                    .OpenTimeout(TimeSpan.FromSeconds(10)))
                .WithAddress("net.tcp://localhost:6666/FactoryService")
                .Create();
            Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId} - Proxy created");
            proxy.Echo();
            proxy.Ping();
            Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId} - Proxy call completed");
        }
        public void Pong()
        {
            Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId} - PONG");
        }
        public async void Synchronizer_Click(object sender, RoutedEventArgs args)
        {
            Debug.WriteLine($"Test started in Thread '{Thread.CurrentThread.ManagedThreadId}'");
            var not = new SynchronizableNotifierMock();
            not.Integer = 1;
            not.PropertyChanged += (s, e) =>
            {
                e.Case(() => s.Integer, a =>
                {
                    Debug.WriteLine($"Integer changed to '{a.ActualValue}' in Thread '{Thread.CurrentThread.ManagedThreadId}'");
                });
            };
            not.Integer = 2;
            var task = TaskManager.StartNew(() =>
            {
                Debug.WriteLine($"Task started in Thread '{Thread.CurrentThread.ManagedThreadId}'");
                not.Integer = 3;
            });
            await task;
            not.Synchronizer = new DispatcherSynchronizer();
            task = TaskManager.StartNew(() =>
            {
                Debug.WriteLine($"Task started in Thread '{Thread.CurrentThread.ManagedThreadId}'");
                not.Integer = 4;
            });
            not.Integer = 5;
            await task;
            not.Integer = 6;
            //DispatcherSynchronizer sync = new DispatcherSynchronizer();
        }
    }
    public class SynchronizableNotifierMock : Notifier<SynchronizableNotifierMock>
    {
        public int Integer { get { return GetValue(() => 0); } set { SetValue(value); } }
    }
}
