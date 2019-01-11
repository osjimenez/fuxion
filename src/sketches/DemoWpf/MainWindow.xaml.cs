using DemoWpf;
using Fuxion.ComponentModel;
using Fuxion.Configuration;
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
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.ServiceModel.Security;
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
using System.Xml.Linq;

namespace DemoWpf
{
    public class ConfigurationMock : ConfigurationItem<ConfigurationMock>
    {
        public override Guid ConfigurationItemId { get { return Guid.Parse("{7476B8AD-E6EB-4379-89D3-A5777013E9D1}"); } }
        public string Args { get { return GetValue(() => "DefaultArgs"); } set { SetValue(value); } }
        public string Args2 { get { return GetValue(() => "DefaultArgs2"); } set { SetValue(value); } }
        public int Vamos { get { return GetValue(() => 10); } set { SetValue(value); } }
    }
    public partial class MainWindow : Window, IFactoryServiceCallback
    {
        public MainWindow()
        {
            InitializeComponent();

            //Factory.AddInjector(new InstanceInjector<OverlayManager>(new OverlayManager(OverlayControl)));
            //var man = Factory.Get<OverlayManager>();
            //man.ShowOverlay(OverlayData<MockControl>.CreateDefault(new MockControl()));
            //man.ShowOverlay(new OverlayDialogData
            //{
            //    //Content = "Contenido",
            //    Title = "Titulo",
            //    Buttons = new[]
            //    {
            //        new OverlayDialogButton
            //        {
            //            Text = "SI",
            //            //OnCommand = data=> { data.}
            //        },
            //        new OverlayDialogButton
            //        {
            //            Text = "NO"
            //        }
            //    }
            //});


            //XmlFileConfiguration f = new XmlFileConfiguration {
            //    Path = "config2.xml"
            //};
            //f.Clear();
            //f.Reset<ConfigurationMock>();
            //f.Save();
            //var cm = f.Get<ConfigurationMock>();
            //cm.Args = "Demo";
            //cm.Args2 = "Demo2";
            //cm.Vamos = 123;
            //f.Save();


            ConfigureFactory();
            Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId} - Window created");
        }
        private void ConfigureFactory()
        {
            Container con = new Container();
            con.Register<ToInject>();
            Factory.AddInjector(new SimpleInjectorFactoryInjector(con));
        }
        //class ServiceValidator : UserNamePasswordValidator
        //{
        //    public override void Validate(string userName, string password)
        //    {
        //    }
        //}
        //class CertificateValidator : X509CertificateValidator
        //{
        //    public override void Validate(X509Certificate2 certificate) {

        //    }
        //}
        public async void HostService_Click(object sender, RoutedEventArgs args)
        {
            //await CreateService();
            //return;
            await ServiceBuilder.Host<FactoryService>()
                .DefaultTcpSecurizedHost<IFactoryService>(
                    6666, "FactoryService",
                    new X509Certificate2(Properties.Resources.Fuxion, "fuxion"),
                    (username, password) =>
                    {
                        if (username != "username" || password != "password")
                        {
                            throw new AuthenticationException("Invalid username or password");
                        }
                    })
                .MakeDiscoverable()
                .OpenAsync(afterOpenAction: _ => Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId} - Service host opened"));
        }
        class ServiceValidator : UserNamePasswordValidator
        {
            public ServiceValidator(Action<string, string> userNameValidationAction)
            {
                this.userNameValidationAction = userNameValidationAction;
            }
            Action<string, string> userNameValidationAction;
            public override void Validate(string userName, string password)
            {
                userNameValidationAction(userName, password);
            }
        }
        private async Task CreateService()
        {
            await TaskManager.StartNew(() =>
            {
                var host = new ServiceHost(typeof(FactoryService));

                var cert = new X509Certificate2(Properties.Resources.Fuxion, "fuxion");
                host.Credentials.ServiceCertificate.Certificate = cert;
                host.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.Custom;
                host.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = new ServiceValidator((_, __) => { });

                var bin = new NetTcpBinding(SecurityMode.Message)
                {
                    MaxReceivedMessageSize = int.MaxValue,
                    MaxBufferSize = int.MaxValue
                };
                bin.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
                bin.ReceiveTimeout = TimeSpan.MaxValue;
                bin.SendTimeout = TimeSpan.MaxValue;

                var cusBin = new CustomBinding(bin);
                var security = cusBin.Elements.Find<SecurityBindingElement>();
                if (security != null)
                {
                    security.LocalServiceSettings.MaxClockSkew = TimeSpan.FromDays(1);
                    security.LocalClientSettings.MaxClockSkew = TimeSpan.FromDays(1);
                }

                var se = new ServiceEndpoint(
                    ContractDescription.GetContract(typeof(IFactoryService)),
                    cusBin,
                    new EndpointAddress(
                        new Uri("net.tcp://localhost:6666/FactoryService")
                        ));
                host.AddServiceEndpoint(se);
                var edb = new EndpointDiscoveryBehavior();
                edb.Extensions.Add(new XElement("NetBiosName", Environment.MachineName));
                var dns = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in dns.AddressList)
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) edb.Extensions.Add(new XElement("IpAddress", ip.ToString()));
                se.Behaviors.Add(edb);
                host.Description.Behaviors.Add(new ServiceDiscoveryBehavior());
                host.AddServiceEndpoint(new UdpDiscoveryEndpoint());

                host.Open();
            });
        }
        public void SearchService_Click(object sender, RoutedEventArgs args)
        {
            ServiceBuilder.DiscoverServices<IFactoryService>()
                .OnFind(res =>
                {
                    Debug.WriteLine(res.NetBiosName);
                    res.Manager.Stop();
                })
                .Start();
            //var dis = new DiscoveryClient(new UdpDiscoveryEndpoint());
            //dis.FindProgressChanged += (s, e) =>
            //{
            //    //if (!e.EndpointDiscoveryMetadata.Extensions.Any(ex => ex.Name == "NetBiosName")) return;
            //    //string netbios = e.EndpointDiscoveryMetadata.Extensions.First(ex => ex.Name == "NetBiosName").Value;
            //    //int port = e.EndpointDiscoveryMetadata.Address.Uri.Port;
            //    //IEnumerable<string> ipList = e.EndpointDiscoveryMetadata.Extensions.Where(ex => ex.Name == "IpAddress").Select(ex => ex.Value);
            //    //if (FoundServices.Any(fs => fs.NetBiosName == netbios))
            //    //{
            //    //    FoundService ser = FoundServices.First(fs => fs.NetBiosName == netbios);
            //    //    foreach (string ip in ipList.Intersect(ser.IpAddressList))
            //    //        ser.IpAddressList.Add(ip);
            //    //}
            //    //else
            //    //    FoundServices.Add(new FoundService
            //    //    {
            //    //        IpAddressList = new List<string>(ipList),
            //    //        NetBiosName = netbios,
            //    //        Port = port
            //    //    });
            //    Debug.WriteLine("");
            //};
            //dis.FindCompleted += (s, e) =>
            //{
            //    dis.FindAsync(new FindCriteria(typeof(IFactoryService)));
            //};
            //dis.FindAsync(new FindCriteria(typeof(IFactoryService)));
        }
        public void ProxyService_Click(object sender, RoutedEventArgs args)
        {
            //var proxy = await ServiceBuilder.Proxy<IFactoryService>(this)
            //var proxy = await ServiceBuilder.Proxy(new CustomDuplexChannelFactory<IFactoryService>(this))

            //var proxy = await ServiceBuilder.Proxy((endpoint) => new CustomDuplexChannelFactory<IFactoryService>(this, endpoint))
            //var proxy = ServiceBuilder.Proxy(this, (instance, endpoint) => new CustomDuplexChannelFactory<IFactoryService>(instance, endpoint))
            var proxy = ServiceBuilder.Proxy<IFactoryService>(this)
                .DefaultTcpSecurizedProxy("localhost", 6666, "FactoryService", "fuxion.demo", "username", "password", "0DFF4D21124E15D8B3365B03E65AE4B9F4A52FF3")
                .Create();
            //.OpenAsync(afterOpenAction: _ => Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId} - Proxy opened"));

            Stopwatch sw = new Stopwatch();
            sw.Start();
            ((ICommunicationObject)proxy).Open();
            Debug.WriteLine("Open WATCH: " + sw.ElapsedMilliseconds);

            sw.Restart();
            proxy.Ping();
            Debug.WriteLine("Ping WATCH: " + sw.ElapsedMilliseconds);

            Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId} - Proxy ping call completed");

            /*
             * To allow the self signed certificate must add this to app.config or web.config
            <configuration>
                <runtime>
                    <AppContextSwitchOverrides value="Switch.System.IdentityModel.DisableMultipleDNSEntriesInSANCertificate=true" /> 
                </runtime>
            </configuration>
            */

            return;
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
            //not.Synchronizer = new DispatcherSynchronizer();
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
        private void Button_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
    public class SynchronizableNotifierMock : Notifier<SynchronizableNotifierMock>
    {
        public int Integer { get { return GetValue(() => 0); } set { SetValue(value); } }
    }
}
