using Fuxion.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion
{
    public interface IApplicationCommandManager
    {
        Task DoAsync<TCommand>(TCommand command) where TCommand : ICommand;
    }
}
