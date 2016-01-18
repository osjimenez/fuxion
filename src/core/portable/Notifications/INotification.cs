using Fuxion.Models;
using Fuxion.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fuxion.Events;

namespace Fuxion.Notifications
{
    public interface INotification
    {
        Guid SourceId { get; set; }
    }
}
