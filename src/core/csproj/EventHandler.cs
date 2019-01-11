using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion
{
    public delegate void EventHandler<TSource, TEventArgs>(TSource source, TEventArgs args) where TEventArgs : EventArgs;
}
