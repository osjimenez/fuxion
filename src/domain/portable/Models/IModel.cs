using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Domain.Models
{
    public interface IModel
    {
        Guid Id { get; set; }
        string Name { get; set; }
    }
}
