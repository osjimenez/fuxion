using DemoWpf.Properties;
using Fuxion.ServiceModel;
using Fuxion.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DemoWpf.ServiceModel
{
	public class Service : IContract
	{
		public void Test()
		{
			MessageBox.Show("TEST");
		}

		public int Port { get; set; } = 12345;

		public async void Publish()
		{
			await TaskManager.StartNew(() =>
			{
				var cert = new X509Certificate2(Resources.Fuxion, "fuxion");
				var host = ServiceBuilder.Host<Service>()
					.DefaultTcpSecurizedHost<IContract>(Port, "TestPath", cert, (u, p) => { }, bin => bin
						.LocalClientMaxClockSkew(TimeSpan.FromDays(10))
						.LocalServiceMaxClockSkew(TimeSpan.FromDays(10)))
					.Open();
			});
			//TaskManager.StartNew(() => {
			//	while (true)
			//	{
			//		Thread.Sleep(2000);

			//	}
			//});
		}
	}
}
