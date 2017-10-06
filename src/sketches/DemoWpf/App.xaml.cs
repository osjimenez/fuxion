using Fuxion;
using Fuxion.Factories;
using Fuxion.Licensing;
using Fuxion.Logging;
using Fuxion.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DemoWpf
{
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            Factory.AddInjector(new InstanceInjector<ILogFactory>(new Log4netFactory()));
            new MainWindow().Show();
            //new TimeProvider().Show();
        }
    }
}
