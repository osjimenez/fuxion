using Fuxion.DynamicData;
using Fuxion.Shell.Resources;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Shell.Models
{
	public class BaseDvo : ReactiveObject, ICacheable
	{
		Guid _Id = Guid.NewGuid();
		[Display(Name = nameof(Strings.Id), GroupName = nameof(Strings.Ids), ResourceType = typeof(Strings))]
		public Guid Id
		{
			get => _Id;
			set => this.RaiseAndSetIfChanged(ref _Id, value);
		}
		string? _Name;
		[Display(Name = nameof(Strings.Name), GroupName = nameof(Strings.Names), ResourceType = typeof(Strings))]
		[Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = nameof(Strings.Required))]
		[StringLength(200, ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = nameof(Strings.StringLength))]
		public string? Name
		{
			get => _Name;
			set => this.RaiseAndSetIfChanged(ref _Name, value);
		}
	}
}
