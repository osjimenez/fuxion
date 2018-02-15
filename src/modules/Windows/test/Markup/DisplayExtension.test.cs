using Fuxion.ComponentModel;
using Fuxion.Test;
using Fuxion.Windows.Markup;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Markup;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Windows.Test.Markup
{
	public class DisplayExtensionTest : BaseTest
	{
		public DisplayExtensionTest(ITestOutputHelper output) : base(output) { }
		public static Task StartSTATask(Action action)
		{
			var tcs = new TaskCompletionSource<object>();
			var thread = new Thread(() =>
			{
				try
				{
					action();
					tcs.SetResult(new object());
				}
				catch (Exception e)
				{
					tcs.SetException(e);
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			return tcs.Task;
		}
		[Fact(DisplayName = "DisplayMarkupExtension - Create chain")]
		public async Task DisplayMarkupExtension_CreateChain()
		{
			await StartSTATask(() =>
			{
				Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

				var ext = new DisplayExtension("Dto.SubDto.Value")
				{
					Printer = Printer.Default
				};
				var dtoLink = ext.chain.First();
				var subDtoLink = ext.chain.Skip(1).First();
				var valueLink = ext.chain.Skip(2).First();
				// Check property names
				Assert.Equal("Dto", dtoLink.PropertyName);
				Assert.Equal("SubDto", subDtoLink.PropertyName);
				Assert.Equal("Value", valueLink.PropertyName);
				// Check NextLink
				Assert.True(dtoLink.NextLink == subDtoLink);
				Assert.True(subDtoLink.NextLink == valueLink);
				Assert.Null(valueLink.NextLink);
				// Check PreviousLink
				Assert.Null(dtoLink.PreviousLink);
				Assert.True(subDtoLink.PreviousLink == dtoLink);
				Assert.True(valueLink.PreviousLink == subDtoLink);
			});
		}
		[Fact(DisplayName = "DisplayMarkupExtension - Direct property")]
		public async Task DisplayMarkupExtension_DirectProperty()
		{
			await StartSTATask(() =>
			{
				Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

				var ext = new DisplayExtension("Dto")
				{
					Printer = Printer.Default
				};
				var ser = new ServiceProviderMock();
				var provider = ser.GetService(null) as ProvideValueTargetMock;
				ext.ProvideValue(ser);

				var expectedValue = ViewModelMock.DtoDisplayName;

				Assert.NotEqual(expectedValue, provider.textBlock.Text);
				provider.textBlock.DataContext = new ViewModelMock();
				Assert.Equal(expectedValue, provider.textBlock.Text);
			});
		}
		[Fact(DisplayName = "DisplayMarkupExtension - Two level property")]
		public async Task DisplayMarkupExtension_TwoLevelProperty()
		{
			await StartSTATask(() =>
			{
				Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

				var ext = new DisplayExtension("Dto.Dto2")
				{
					Printer = Printer.Default
				};
				var ser = new ServiceProviderMock();
				var provider = ser.GetService(null) as ProvideValueTargetMock;
				ext.ProvideValue(ser);

				var expectedValue = DtoMock.Dto2DisplayName;

				Assert.NotEqual(expectedValue, provider.textBlock.Text);
				var viewModel = new ViewModelMock();
				provider.textBlock.DataContext = viewModel;
				Assert.NotEqual(expectedValue, provider.textBlock.Text);
				viewModel.Dto = new DtoMock();
				Assert.Equal(expectedValue, provider.textBlock.Text);
			});
		}
		[Fact(DisplayName = "DisplayMarkupExtension - Three level property")]
		public async Task DisplayMarkupExtension_ThreeLevelProperty()
		{
			await StartSTATask(() =>
			{
				Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

				var ext = new DisplayExtension("Dto.Dto2.Dto3")
				{
					Printer = Printer.Default
				};
				var ser = new ServiceProviderMock();
				var provider = ser.GetService(null) as ProvideValueTargetMock;
				ext.ProvideValue(ser);

				var expectedValue = Dto2Mock.Dto3DisplayName;

				Assert.NotEqual(expectedValue, provider.textBlock.Text);
				var viewModel = new ViewModelMock();
				provider.textBlock.DataContext = viewModel;
				Assert.NotEqual(expectedValue, provider.textBlock.Text);
				var dto = new DtoMock();
				viewModel.Dto = dto;
				Assert.NotEqual(expectedValue, provider.textBlock.Text);
				dto.Dto2 = new Dto2Mock();
				Assert.Equal(expectedValue, provider.textBlock.Text);

				dto.Dto2 = null;
				Assert.NotEqual(expectedValue, provider.textBlock.Text);
			});
		}
		[Fact(DisplayName = "DisplayMarkupExtension - Three level property without DisplayAttribute")]
		public async Task DisplayMarkupExtension_ThreeLevelProperty2()
		{
			await StartSTATask(() =>
			{
				Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

				var ext = new DisplayExtension("Dto.Dto2.Dto3WithoutDisplayAttribute")
				{
					Printer = Printer.Default
				};
				var ser = new ServiceProviderMock();
				var provider = ser.GetService(null) as ProvideValueTargetMock;
				ext.ProvideValue(ser);

				string expectedValue = "Dto3WithoutDisplayAttribute";

				Assert.NotEqual(expectedValue, provider.textBlock.Text);
				var viewModel = new ViewModelMock
				{
					Dto = new DtoMock
					{
						Dto2 = new Dto2Mock()
					}
				};
				provider.textBlock.DataContext = viewModel;
				Assert.Equal(expectedValue, provider.textBlock.Text);
			});
		}
		[Fact(DisplayName = "DisplayMarkupExtension - Four level property")]
		public async Task DisplayMarkupExtension_FourLevelProperty()
		{
			await StartSTATask(() =>
			{
				Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

				var ext = new DisplayExtension("Dto.Dto2.Dto3.Value")
				{
					Printer = Printer.Default
				};
				var ser = new ServiceProviderMock();
				var provider = ser.GetService(null) as ProvideValueTargetMock;
				ext.ProvideValue(ser);

				var expectedValue = Dto3Mock.ValueDisplayName;

				Assert.NotEqual(expectedValue, provider.textBlock.Text);
				var viewModel = new ViewModelMock();
				provider.textBlock.DataContext = viewModel;
				Assert.NotEqual(expectedValue, provider.textBlock.Text);
				var dto = new DtoMock();
				viewModel.Dto = dto;
				Assert.NotEqual(expectedValue, provider.textBlock.Text);
				var dto2 = new Dto2Mock();
				dto.Dto2 = dto2;
				Assert.NotEqual(expectedValue, provider.textBlock.Text);
				dto2.Dto3 = new Dto3Mock();
				Assert.Equal(expectedValue, provider.textBlock.Text);

				dto2.Dto3 = null;
				Assert.NotEqual(expectedValue, provider.textBlock.Text);
			});
		}
	}
	public class ServiceProviderMock : IServiceProvider
	{
		ProvideValueTargetMock provideValueTargetMock = new ProvideValueTargetMock();
		public object GetService(Type serviceType) => provideValueTargetMock;
	}
	public class ProvideValueTargetMock : IProvideValueTarget
	{
		public TextBlock textBlock = new TextBlock();
		public object TargetObject => textBlock;
		public object TargetProperty => TextBlock.TextProperty;
	}
	public class ViewModelMock : Notifier<ViewModelMock>
	{
		public const string DtoDisplayName= nameof(DtoMock) + " display name";
		[Display(Name = DtoDisplayName)]
		public DtoMock Dto
		{
			get => GetValue<DtoMock>();
			set => SetValue(value);
		}
	}
	public class DtoMock : Notifier<DtoMock> {
		public const string Dto2DisplayName = nameof(Dto2Mock) + " display name";
		[Display(Name = Dto2DisplayName)]
		public Dto2Mock Dto2
		{
			get => GetValue<Dto2Mock>();
			set => SetValue(value);
		}
	}
	public class Dto2Mock : Notifier<Dto2Mock>
	{
		public const string Dto3DisplayName = nameof(Dto3Mock) + " display name";
		[Display(Name = Dto3DisplayName)]
		public Dto3Mock Dto3
		{
			get => GetValue<Dto3Mock>();
			set => SetValue(value);
		}
		public Dto3Mock Dto3WithoutDisplayAttribute
		{
			get => GetValue<Dto3Mock>();
			set => SetValue(value);
		}
	}
	public class Dto3Mock : Notifier<Dto3Mock>
	{
		public const string ValueDisplayName = nameof(Value) + " display name";
		[Display(Name = ValueDisplayName)]
		public string Value
		{
			get => GetValue<string>();
			set => SetValue(value);
		}
	}
}
