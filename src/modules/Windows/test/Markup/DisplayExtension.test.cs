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
using System.Windows;
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
		[Fact(DisplayName = "DisplayExtension - Create chain")]
		public async Task DisplayExtension_CreateChain()
		{
			await StartSTATask(() =>
			{
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
		private T RegisterFrameworkElement<T>(object property, string bindPath) where T : FrameworkElement, new()
		{
			var ext = new DisplayExtension(bindPath)
			{
				Printer = Printer.Default
			};
			var element = new T();
			var ser = new ServiceProviderMock(element, property);
			var provider = ser.GetService(null) as ProvideValueTargetMock;
			ext.ProvideValue(ser);
			return element;
		}
		private T RegisterFrameworkContentElement<T>(object property, string bindPath) where T : FrameworkContentElement, new()
		{
			var ext = new DisplayExtension(bindPath)
			{
				Printer = Printer.Default
			};
			var element = new T();
			var ser = new ServiceProviderMock(element, property);
			var provider = ser.GetService(null) as ProvideValueTargetMock;
			ext.ProvideValue(ser);
			return element;
		}
		[Fact(DisplayName = "DisplayExtension - Different values")]
		public async Task DifferentValues()
		{
			await StartSTATask(() =>
			{
				var textBlock = RegisterFrameworkElement<TextBlock>(TextBox.TextProperty, "[Name]Dto");
				var expectedValue = ViewModelMock.DtoDisplayName;
				textBlock.DataContext = new ViewModelMock();
				Assert.Equal(expectedValue, textBlock.Text);

				// ShortName
				textBlock = RegisterFrameworkElement<TextBlock>(TextBox.TextProperty, "[ShortName]Dto");
				expectedValue = ViewModelMock.DtoDisplayShortName;
				textBlock.DataContext = new ViewModelMock();
				Assert.Equal(expectedValue, textBlock.Text);

				// GroupName
				textBlock = RegisterFrameworkElement<TextBlock>(TextBox.TextProperty, "[GroupName]Dto");
				expectedValue = ViewModelMock.DtoDisplayGroupName;
				textBlock.DataContext = new ViewModelMock();
				Assert.Equal(expectedValue, textBlock.Text);

				// Description
				textBlock = RegisterFrameworkElement<TextBlock>(TextBox.TextProperty, "[Description]Dto");
				expectedValue = ViewModelMock.DtoDisplayDescription;
				textBlock.DataContext = new ViewModelMock();
				Assert.Equal(expectedValue, textBlock.Text);

				// Order
				textBlock = RegisterFrameworkElement<TextBlock>(TextBox.TextProperty, "[Order]Dto");
				expectedValue = ViewModelMock.DtoDisplayOrder.ToString();
				textBlock.DataContext = new ViewModelMock();
				Assert.Equal(expectedValue, textBlock.Text);

				// Prompt
				textBlock = RegisterFrameworkElement<TextBlock>(TextBox.TextProperty, "[Prompt]Dto");
				expectedValue = ViewModelMock.DtoDisplayPrompt;
				textBlock.DataContext = new ViewModelMock();
				Assert.Equal(expectedValue, textBlock.Text);
			});
		}
		[Fact(DisplayName = "DisplayExtension - Direct property")]
		public async Task DirectProperty()
		{
			await StartSTATask(() =>
			{
				var textBlock = RegisterFrameworkElement<TextBlock>(TextBox.TextProperty, "Dto");

				var expectedValue = ViewModelMock.DtoDisplayName;

				Assert.NotEqual(expectedValue, textBlock.Text);
				textBlock.DataContext = new ViewModelMock();
				Assert.Equal(expectedValue, textBlock.Text);
			});
		}
		[Fact(DisplayName = "DisplayExtension - Direct property FrameworkContentElement")]
		public async Task DirectPropertyFrameworkContentElement()
		{
			await StartSTATask(() =>
			{
				var contentElement = RegisterFrameworkContentElement<FrameworkContentElement>(FrameworkContentElement.TagProperty, "Dto");

				var expectedValue = ViewModelMock.DtoDisplayName;

				Assert.NotEqual(expectedValue, contentElement.Tag);
				contentElement.DataContext = new ViewModelMock();
				Assert.Equal(expectedValue, contentElement.Tag);
			});
		}
		[Fact(DisplayName = "DisplayExtension - Two level property")]
		public async Task TwoLevelProperty()
		{
			await StartSTATask(() =>
			{
				var textBlock = RegisterFrameworkElement<TextBlock>(TextBox.TextProperty, "Dto.Dto2");

				var expectedValue = DtoMock.Dto2DisplayName;

				Assert.NotEqual(expectedValue, textBlock.Text);
				var viewModel = new ViewModelMock();
				textBlock.DataContext = viewModel;
				Assert.NotEqual(expectedValue, textBlock.Text);
				viewModel.Dto = new DtoMock();
				Assert.Equal(expectedValue, textBlock.Text);
			});
		}
		[Fact(DisplayName = "DisplayExtension - Three level property")]
		public async Task ThreeLevelProperty()
		{
			await StartSTATask(() =>
			{
				var textBlock = RegisterFrameworkElement<TextBlock>(TextBox.TextProperty, "Dto.Dto2.Dto3");

				var expectedValue = Dto2Mock.Dto3DisplayName;

				Assert.NotEqual(expectedValue, textBlock.Text);
				var viewModel = new ViewModelMock();
				textBlock.DataContext = viewModel;
				Assert.NotEqual(expectedValue, textBlock.Text);
				var dto = new DtoMock();
				viewModel.Dto = dto;
				Assert.NotEqual(expectedValue, textBlock.Text);
				dto.Dto2 = new Dto2Mock();
				Assert.Equal(expectedValue, textBlock.Text);

				dto.Dto2 = null;
				Assert.NotEqual(expectedValue, textBlock.Text);
			});
		}
		[Fact(DisplayName = "DisplayExtension - Three level property without Display")]
		public async Task ThreeLevelPropertyWithoutDisplay()
		{
			await StartSTATask(() =>
			{
				DisplayExtension.NonAttrributePrefix = "prefix:";
				DisplayExtension.NonAttrributeSufix = ":sufix";
				var textBlock = RegisterFrameworkElement<TextBlock>(TextBox.TextProperty, "Dto.Dto2.Dto3WithoutDisplayAttribute");

				string expectedValue = DisplayExtension.NonAttrributePrefix + "Dto3WithoutDisplayAttribute" + DisplayExtension.NonAttrributeSufix;

				Assert.NotEqual(expectedValue, textBlock.Text);
				Assert.Equal("", textBlock.Text);
				var viewModel = new ViewModelMock
				{
					Dto = new DtoMock
					{
						Dto2 = new Dto2Mock()
					}
				};
				textBlock.DataContext = viewModel;
				Assert.Equal(expectedValue, textBlock.Text);
			});
		}
		[Fact(DisplayName = "DisplayExtension - Four level property")]
		public async Task FourLevelProperty()
		{
			await StartSTATask(() =>
			{
				var textBlock = RegisterFrameworkElement<TextBlock>(TextBox.TextProperty, "Dto.Dto2.Dto3.Value");

				var expectedValue = Dto3Mock.ValueDisplayName;

				Assert.NotEqual(expectedValue, textBlock.Text);
				var viewModel = new ViewModelMock();
				textBlock.DataContext = viewModel;
				Assert.NotEqual(expectedValue, textBlock.Text);
				var dto = new DtoMock();
				viewModel.Dto = dto;
				Assert.NotEqual(expectedValue, textBlock.Text);
				var dto2 = new Dto2Mock();
				dto.Dto2 = dto2;
				Assert.NotEqual(expectedValue, textBlock.Text);
				dto2.Dto3 = new Dto3Mock();
				Assert.Equal(expectedValue, textBlock.Text);

				dto2.Dto3 = null;
				Assert.NotEqual(expectedValue, textBlock.Text);
			});
		}
	}
	public class ServiceProviderMock : IServiceProvider
	{
		public ServiceProviderMock(object targetObject, object targetProperty)
		{
			provideValueTargetMock = new ProvideValueTargetMock(targetObject, targetProperty);
		}
		ProvideValueTargetMock provideValueTargetMock;
		public object GetService(Type serviceType) => provideValueTargetMock;
	}
	public class ProvideValueTargetMock : IProvideValueTarget
	{
		public ProvideValueTargetMock(object targetObject, object targetProperty)
		{
			TargetObject = targetObject;
			TargetProperty = targetProperty;
		}
		public TextBlock textBlock = new TextBlock();
		public object TargetObject { get; set; }
		public object TargetProperty { get; set; }
	}
	public class ViewModelMock : Notifier<ViewModelMock>
	{
		public const string DtoDisplayName= nameof(DtoMock) + " display name";
		public const string DtoDisplayShortName = nameof(DtoMock) + " display short name";
		public const string DtoDisplayGroupName = nameof(DtoMock) + " display group name";
		public const string DtoDisplayDescription = nameof(DtoMock) + " display description";
		public const int DtoDisplayOrder = 1;
		public const string DtoDisplayPrompt = nameof(DtoMock) + " display prompt";
		[Display(
			Name = DtoDisplayName, 
			GroupName = DtoDisplayGroupName, 
			Description = DtoDisplayDescription, 
			Order = DtoDisplayOrder, 
			Prompt = DtoDisplayPrompt, 
			ShortName = DtoDisplayShortName)]
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
