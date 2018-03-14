#if (NET471)
using Fuxion.Logging;

namespace Fuxion.Logging
{
    public interface ILog4netLog : log4net.ILog, ILog { }
}
#endif