using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Net
{
    public enum ConnectionState
    {
        Created,
        Opening,
        Opened,
        Closing,
        Closed,
        Faulted
    }
}
