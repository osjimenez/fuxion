using Fuxion;
using Fuxion.Licensing;
using Fuxion.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DemoWpf
{
    public partial class TimeProvider : Window
    {
        public TimeProvider()
        {
            InitializeComponent();
        }
        string[] WebServersAddresses { get; } = new[]
{
            "http://www.google.com",
            "http://www.google.es",
            "http://www.youtube.com",
            "http://www.microsoft.com",
            "http://www.yahoo.com",
            "http://www.amazon.com",
            "http://www.facebook.com",
            "http://www.twitter.com",
        };

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //var atp = new AverageTimeProvider
                //{
                //    MaxFailsPerTry = 1,
                //    RandomizedProvidersPerTry = WebServersAddresses.Length,
                //    Log = LogManager.Create<AverageTimeProvider>(),
                //};
                //foreach (var pro in WebServersAddresses.Select(address => new InternetTimeProvider
                //{
                //    ServerAddress = address,
                //    ServerType = InternetTimeServerType.Web,
                //    Timeout = TimeSpan.FromSeconds(5),
                //})) atp.AddProvider(pro, true, true);
                var attp = new AntiTamperedTimeProvider(
                    new AverageTimeProvider()
                    .Transform(p=>
                    {
                        p.Log = LogManager.Create<AverageTimeProvider>();
                        p.AddProvider(new DefaultTimeProvider());
                        return p;
                    }),
                    new AntiBackTimeProvider(new RegistryStorageTimeProvider())
                    {
                        Log = LogManager.Create<AntiBackTimeProvider>(),
                        TimeProvider = new MockTimeProvider().Transform(p2 =>
                        {
                            p2.SetOffset(TimeSpan.FromDays(int.Parse(OffsetTextBox.Text)));
                            return p2;
                        })
                    })
                {
                    Log = LogManager.Create<AntiTamperedTimeProvider>(),
                };
                var res = attp.Now();
                MessageBox.Show("res: " + res);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Error '{ex.GetType().Name}': {ex.Message}");
            }
        }
    }
}
