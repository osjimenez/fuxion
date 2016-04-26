using Fuxion.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Net
{
    public interface IConnectableNotifier<TConnectableNotifier> : INotifier<TConnectableNotifier> where TConnectableNotifier : IConnectableNotifier<TConnectableNotifier>
    {
        TimeSpan AutomaticConnectionModeRetryInterval { get; set; }
        ConnectionMode ConnectMode { get; set; }
        ConnectionState State { get; }
        event EventHandler<EventArgs<bool>> IsConnectedChanged;
        bool IsConnected { get; }
        Task Connect();
        Task Disconnect();
        TimeSpan KeepAliveInterval { get; set; }
        bool IsKeepAliveEnable { get; set; }
    }
}
