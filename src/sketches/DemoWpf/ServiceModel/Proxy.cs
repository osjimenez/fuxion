using DemoWpf.Properties;
using Fuxion.ServiceModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DemoWpf.ServiceModel
{
	public class Proxy : ICallback
	{
		IContract service;

		public string Host { get; set; } = "172.17.0.19";
		public int Port { get; set; } = 12345;
		public string Username { get; set; } = "fuxion";
		public string Password { get; set; } = "fuxion";

		public void Connect()
		{
			var cert = new X509Certificate2(Resources.Fuxion, "fuxion");
			service = ServiceBuilder.Proxy<IContract>(this)
				.DefaultTcpSecurizedProxy(Host, Port, "TestPath", "Fuxion.local", Username, Password, cert.Thumbprint, bin => bin
						 .LocalClientMaxClockSkew(TimeSpan.FromDays(10))
						 .LocalServiceMaxClockSkew(TimeSpan.FromDays(10)))
				.Open();
		}

		internal void Test()
		{
			service.Test();
		}

		void ICallback.CallbackTest()
		{
			MessageBox.Show("CALLBACK");
		}
	}
}
