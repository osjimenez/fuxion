using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Ordinem.Shell.Xamarin.Forms.Models;
using Ordinem.Shell.Xamarin.Forms.ViewModels;

namespace Ordinem.Shell.Xamarin.Forms.Views
{
	public partial class ItemDetailPage : ContentPage
	{
		ItemDetailViewModel viewModel;

		public ItemDetailPage(ItemDetailViewModel viewModel)
		{
			InitializeComponent();

			BindingContext = this.viewModel = viewModel;
		}

		public ItemDetailPage()
		{
			InitializeComponent();

			var item = new Item
			{
				Text = "Item 1",
				Description = "This is an item description."
			};

			viewModel = new ItemDetailViewModel(item);
			BindingContext = viewModel;
		}
	}
}