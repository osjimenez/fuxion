using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Domain.Commands
{
    public interface ICommand
    {
		Guid TargetId { get; set; }
        // TODO - Oscar - SourceId was created to Orleans. It's for detect if a command must be launched with GrainClient or not
        // Remove SourceId and create a envelope message to trasport Orleans events/commands in wich SourceId will be.
        Guid SourceId { get; set; }
        // TODO - Oscar - Make the correlation function to trace a process beetwen multiple commands, think about naming and purpose of this feature
        Guid? SagaId { get; set; }
    }
}
