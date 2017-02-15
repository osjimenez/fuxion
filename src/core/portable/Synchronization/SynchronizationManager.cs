using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    public class SynchronizationManager
    {
        ICollection<SessionRunner> runners = new List<SessionRunner>();
        public Task<SessionPreview> PreviewAsync(SessionDefinition definition)
        {
            var res = new SessionRunner(definition);
            runners.Add(res);
            return res.PreviewAsync();
        }
        public Task RunAsync(SessionPreview preview)
        {
            return runners.Single(r => r.Id == preview.Id).RunAsync(preview);
        }
    }
}
