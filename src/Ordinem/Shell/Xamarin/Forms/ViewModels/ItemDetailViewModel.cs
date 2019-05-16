using System;

using Ordinem.Shell.Xamarin.Forms.Models;

namespace Ordinem.Shell.Xamarin.Forms.ViewModels
{
	public class ItemDetailViewModel : BaseViewModel
	{
		public Item Item { get; set; }
		public ItemDetailViewModel(Item item = null)
		{
			Title = item?.Text;
			Item = item;
		}
	}
}
