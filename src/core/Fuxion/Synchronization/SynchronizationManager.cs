using Fuxion.Threading;
using Fuxion.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
	public class SynchronizationManager : IDisposable
	{
		public SynchronizationManager()
		{
			cleanEntriesTask = TaskManager.StartNew(async () =>
			{
				// Este primer delay es para que se cree la Task adecuadamente
				await Task.Delay(TimeSpan.FromSeconds(5));
				while (!cleanEntriesTask.IsCancellationRequested())
				{
					await Task.Delay(CheckOutdatedSessionsInterval, cleanEntriesTask.GetCancellationToken(true));
					runners.Write(l =>
					{
						foreach (var entry in l.Where(e => e.CreationTime.Add(SessionOutdateLimit) < DateTime.Now).ToList())
						{
							l.Remove(entry);
							Logger?.LogInformation($"La sesión de sincronización '{entry.Runner.Session.Name}' ha caducado y se ha eliminado de la caché");
						}
					});
				}
			});
		}
		public ILogger? Logger { get; set; }

		private readonly Task cleanEntriesTask;
		private readonly Locker<ICollection<SessionEntry>> runners = new Locker<ICollection<SessionEntry>>(new List<SessionEntry>());
		public IPrinter Printer { get; set; } = Fuxion.Printer.Default;
		public TimeSpan CheckOutdatedSessionsInterval { get; set; } = TimeSpan.FromMinutes(1);
		public TimeSpan SessionOutdateLimit { get; set; } = TimeSpan.FromMinutes(60);
		public async Task<SessionPreview> PreviewAsync(Session definition, bool includeNoneActionItems = false)
		{
			var runner = new SessionRunner(definition, Printer);
			var preview = await runner.PreviewAsync(includeNoneActionItems, Printer);
			runners.Write(l => l.Add(new SessionEntry(runner)));
			return preview;
		}
		public async Task RunAsync(SessionPreview preview)
		{
			var entry = runners.Read(l => l.Single(r => r.Runner.Id == preview.Id));
			await entry.Runner.RunAsync(preview, Printer);
			runners.Write(l => l.Remove(entry));
		}
		public void Dispose()
		{
			if (cleanEntriesTask != null && cleanEntriesTask.Status == TaskStatus.Running)
				cleanEntriesTask.Cancel();
		}
	}
	internal class SessionEntry
	{
		public SessionEntry(SessionRunner runner)
		{
			Runner = runner;
			CreationTime = DateTime.Now;
		}
		public SessionRunner Runner { get; set; }
		public DateTime CreationTime { get; set; }
	}
}
