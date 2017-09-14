using Fuxion;
using Fuxion.Factories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DemoWpf_net47
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var tup = new ValueTupleForTest();
            if (tup.GetTuple().uno == 1)
            {

            }
            base.OnStartup(e);
        }
    }
}
