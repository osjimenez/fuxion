using Fuxion.Domain.Models;
using Fuxion.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fuxion.Domain.Events;

namespace Fuxion.Domain.Notifications
{
    public interface INotification
    {
        Guid SourceId { get; set; }
    }
}
