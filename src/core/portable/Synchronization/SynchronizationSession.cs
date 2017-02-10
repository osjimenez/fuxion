using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    public class SynchronizationSession
    {
        internal Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; }
        public ICollection<SynchronizationWork> Works { get; set; } = new List<SynchronizationWork>();
        public Task<SynchronizationSessionPreview> PreviewAsync()
        {
            return Printer.IndentAsync($"Previewing synchronization session '{Name}'",
                async () =>
            {
                var res = new SynchronizationSessionPreview(Id);
                var tasks = Works.Select(w => w.PreviewAsync());
                var resTask = await Task.WhenAll(tasks);
                res.Works = resTask;
                return res;
            });
        }
        public async Task RunAsync(SynchronizationSessionPreview preview)
        {
            var actions = new List<Action>();
            await Printer.ForeachAsync($"Running synchronization session '{Name}'", Works,
                async work =>
            {
                var workPre = preview.Works.FirstOrDefault(w => w.Id == work.Id);
                var act = new Action(() => { });
                await work.RunAsync(workPre, act);
            });
            foreach (var act in actions)
                act();
        }
    }
}
