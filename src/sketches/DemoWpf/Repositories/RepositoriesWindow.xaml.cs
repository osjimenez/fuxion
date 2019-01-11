using Fuxion;
using Fuxion.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
namespace DemoWpf.Repositories
{
	public partial class RepositoriesWindow : Window
	{
		public RepositoriesWindow()
		{
			InitializeComponent();
			var rep = new Repository(ser);
			DataContext = new ViewModel(rep);
		}
		Service ser = new Service();
		ViewModel ViewModel => DataContext as ViewModel;
		private void ChangeViewModel_Click(object sender, RoutedEventArgs e)
		{
			var first = ViewModel.Items.First();
			ser.UpdateItem(first.Id, first.CloneWithJson().Transform<ItemDto>(i => i.Name = "oka"));
		}
	}
	public class ViewModel : Notifier<ViewModel>
	{
		public ViewModel(Repository rep)
		{
			this.rep = rep;
		}
		Repository rep;
		public ObservableCollection<ItemDto> Items => rep.Items;
	}
	[DataContract]
	public class ItemDto : Notifier<ItemDto>
	{
		[DataMember]
		public Guid Id
		{
			get => GetValue(() => Guid.NewGuid());
			set => SetValue(value);
		}
		[DataMember]
		public string Name
		{
			get => GetValue<string>();
			set => SetValue(value);
		}
	}
	public class Repository : Notifier<Repository>
	{
		public Repository(Service ser)
		{
			foreach (var item in ser.GetItems())
				Items.Add(item);
			ser.ItemUpdated += (s, e) =>
			{
				var item = Items.FirstOrDefault(i => i.Id == e.Value.Id);
				var index = Items.IndexOf(item);
				Items.Remove(item);
				Items.Insert(index, e.Value);
			};
		}
		public ObservableCollection<ItemDto> Items => GetValue(() => new ObservableCollection<ItemDto>());
	}
	public class Service
	{
		public Service()
		{
			Items.Add(new ItemDto { Name = "One" });
			Items.Add(new ItemDto { Name = "Two" });
		}
		List<ItemDto> Items = new List<ItemDto>();
		public ICollection<ItemDto> GetItems() => Items.CloneWithJson();
		public event EventHandler<EventArgs<ItemDto>> ItemUpdated;
		public void UpdateItem(Guid itemId, ItemDto dto)
		{
			var item = Items.FirstOrDefault(i => i.Id == itemId);
			Items.Remove(item);
			Items.Add(dto);
			ItemUpdated?.Invoke(this, new EventArgs<ItemDto>(dto));
		}
	}
}