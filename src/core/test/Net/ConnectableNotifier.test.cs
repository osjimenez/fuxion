using Fuxion.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test.Net
{
    public class ConnectableNotifierTest
    {
        public ConnectableNotifierTest(ITestOutputHelper output)
        {
            this.output = output;
        }
        ITestOutputHelper output;
        [Fact]
        public void Connectable_NestedTask()
        {
            var con = new ConnectableNotifierMock(output);
            con.ConnectionMode = ConnectionMode.Automatic;

			con.IsKeepAliveEnabled = false;
			con.KeepAliveInterval = TimeSpan.FromSeconds(30);

            while (!con.IsConnected) { }
            Assert.Equal(1, con.Counter);
        }
    }
    public class ConnectableNotifierMock : ConnectableNotifier<ConnectableNotifierMock>
    {
        public ConnectableNotifierMock(ITestOutputHelper output)
        {
            this.output = output;
			base.KeepAliveInterval = TimeSpan.FromMilliseconds(1);
        }
        ITestOutputHelper output;
        public int Counter { get; set; }
        protected override Task OnConnect()
        {
            output.WriteLine($"Enter OnConnect() with Counter={Counter}");
            if (Counter < 1)
            {
                Counter++;
                output.WriteLine($"Throwing exception with Counter={Counter}");
                throw new NotImplementedException();
            }
            output.WriteLine($"Return OnConnect()");
            return Task.CompletedTask;
        }
        protected override Task OnDisconnect()
        {
            return Task.CompletedTask;
        }

		public int MyProperty { get; set; }

		public override bool IsKeepAliveEnabled { get => true; set { throw new InvalidOperationException("KeepAlive cannot be disabled"); } }
		public override TimeSpan KeepAliveInterval { get => TimeSpan.FromMilliseconds(1); set { throw new InvalidOperationException("KeepAlive cannot be disabled"); } }

	}
}
