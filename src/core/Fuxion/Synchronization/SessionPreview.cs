using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Runtime.Serialization;

namespace Fuxion.Synchronization
{
	[DataContract(IsReference = true)]
	public class SessionPreview
	{
		internal SessionPreview() { }
		internal SessionPreview(Guid id) { Id = id; }
		[DataMember]
		public Guid Id { get; set; }
		[DataMember]
		public IList<WorkPreview> Works { get; set; } = new List<WorkPreview>();

		public static SessionPreview Empty = new SessionPreview(Guid.Empty);
		public ResourceManager? ResourceManager { get; set; }
		public int ChangesCount { get { return Works.Sum(w => w.ChangesCount); } }
		public void Print(IPrinter? printer = null)
		{
			printer ??= Printer.Default;
			printer.Foreach("Preview: ", Works, work => work.Print(printer!), false);
		}
		internal void CleanNoneActions()
		{
			foreach (var work in Works.ToList())
			{
				work.CleanNoneActions();
				if (!work.Items.Any())
					Works.Remove(work);
			}
		}
	}
}
