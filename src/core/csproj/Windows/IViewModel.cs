using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Windows
{
    public interface IViewModel : INotifyPropertyChanged
    {
        Guid Id { get; set; }
        string Name { get; }
    }
}
