using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Fuxion.Windows.Controls
{
	public class AutoGrid : Grid
	{
		bool _shouldReindex = true;
		private int GetChildColumnIndex(UIElement child)
		{
			var index = Children.IndexOf(child);
			int colIndex = 0;
			//if (Orientation == Orientation.Horizontal)
				colIndex = index % ColumnDefinitions.Count;
			//else
			//	colIndex = (int)System.Math.Floor((decimal)index / RowDefinitions.Count);
			return colIndex;
		}
		private int GetChildRowIndex(UIElement child)
		{
			var index = Children.IndexOf(child);
			int rowIndex = 0;
			//if (Orientation == Orientation.Horizontal)
			rowIndex = index / ColumnDefinitions.Count;
			//else
			//	colIndex = (int)System.Math.Floor((decimal)index / RowDefinitions.Count);
			return rowIndex;
		}
		internal void PerformLayout(bool force = false)
		{
			Debug.WriteLine("PerformLayout");
			//bool isVertical = Orientation == Orientation.Vertical;
			if (_shouldReindex || force)
			{
				_shouldReindex = false;
				Debug.WriteLine("_shouldReindex = false");
				RowDefinitions.Clear();
				for(int i = 0; i < System.Math.Ceiling((decimal)Children.Count / ColumnDefinitions.Count); i++)
				{
					RowDefinitions.Add(new RowDefinition());
				}
				foreach(UIElement child in Children)
				{
					var colIndex = GetChildColumnIndex(child);
					child.SetValue(Grid.ColumnProperty, colIndex);
					child.SetValue(Grid.RowProperty, GetChildRowIndex(child));
					if(ColumnDefinitions[colIndex] is AutoColumnDefinition acd)
					{
						child.SetValue(FrameworkElement.HorizontalAlignmentProperty, acd.HorizontalContentAlignment);
						child.SetValue(FrameworkElement.VerticalAlignmentProperty, acd.VerticalContentAlignment);
						child.SetValue(FrameworkElement.MarginProperty, acd.ContentMargin);
					}
				}
			}
		}
		#region Overrides
		protected override Size MeasureOverride(Size constraint)
		{
			PerformLayout();
			return base.MeasureOverride(constraint);
		}
		protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
		{
			Debug.WriteLine("_shouldReindex = true");
			_shouldReindex = true;
			base.OnVisualChildrenChanged(visualAdded, visualRemoved);
		}
		#endregion Overrides
	}
	public class AutoColumnDefinition : ColumnDefinition
	{
		[Category("Layout"), Description("Define horizontal alignment for all elements of this column")]
		public HorizontalAlignment HorizontalContentAlignment
		{
			get { return (HorizontalAlignment)GetValue(HorizontalContentAlignmentProperty); }
			set { SetValue(HorizontalContentAlignmentProperty, value); }
		}
		public static readonly DependencyProperty HorizontalContentAlignmentProperty =
			DependencyProperty.Register(nameof(HorizontalContentAlignment), typeof(HorizontalAlignment), typeof(AutoColumnDefinition),
				new FrameworkPropertyMetadata(HorizontalAlignment.Stretch,
					FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
					new PropertyChangedCallback((d, e) =>
					{
						if (d is AutoColumnDefinition acd && acd.Parent is AutoGrid ag) ag.PerformLayout(true);
					})));

		[Category("Layout"), Description("Define vertical alignment for all elements of this column")]
		public VerticalAlignment VerticalContentAlignment
		{
			get { return (VerticalAlignment)GetValue(VerticalContentAlignmentProperty); }
			set { SetValue(VerticalContentAlignmentProperty, value); }
		}
		public static readonly DependencyProperty VerticalContentAlignmentProperty =
			DependencyProperty.Register(nameof(VerticalContentAlignment), typeof(VerticalAlignment), typeof(AutoColumnDefinition),
				new FrameworkPropertyMetadata(VerticalAlignment.Stretch,
					FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
					new PropertyChangedCallback((d, e) =>
					{
						if (d is AutoColumnDefinition acd && acd.Parent is AutoGrid ag) ag.PerformLayout(true);
					})));

		[Category("Layout"), Description("Define vertical alignment for all elements of this column")]
		public Thickness ContentMargin
		{
			get { return (Thickness)GetValue(ContentMarginProperty); }
			set { SetValue(ContentMarginProperty, value); }
		}
		public static readonly DependencyProperty ContentMarginProperty =
			DependencyProperty.Register(nameof(ContentMargin), typeof(Thickness), typeof(AutoColumnDefinition),
				new FrameworkPropertyMetadata(default(Thickness),
					FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
					new PropertyChangedCallback((d, e) =>
					{
						if (d is AutoColumnDefinition acd && acd.Parent is AutoGrid ag) ag.PerformLayout(true);
					})));
	}
}
