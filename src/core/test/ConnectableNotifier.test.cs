using Fuxion.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test
{
    public class ConnectableNotifierTest
    {
        public ConnectableNotifierTest(ITestOutputHelper output)
        {
            this.output = output;
        }
        ITestOutputHelper output;
        [Fact]
        public void Connectable_First() {
            var con = new ConnectableNotifierMock(output);
            con.ConnectMode = ConnectionMode.Automatic;
            while (!con.IsConnected) { }
            Assert.Equal(con.Counter, 1);
        }
        [Fact]
        public void Connectable_NestedTask()
        {
            var con = new ConnectableNotifierMock(output);
            con.ConnectMode = ConnectionMode.Automatic;
            while (!con.IsConnected) { }
            Assert.Equal(con.Counter, 1);
        }
    }
    public class ConnectableNotifierMock : ConnectableNotifier<ConnectableNotifierMock>
    {
        public ConnectableNotifierMock(ITestOutputHelper output)
        {
            this.output = output;
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
            return Task.FromResult(0);
        }
        protected override Task OnDisconnect()
        {
            return Task.FromResult(0);
        }
    }
}
