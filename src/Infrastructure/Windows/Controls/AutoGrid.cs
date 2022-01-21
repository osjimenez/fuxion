namespace Fuxion.Windows.Controls;

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

public class AutoGrid : Grid
{
	private bool mustRelayout = true;
	[Category("Layout"), Description("Define height for all rows of the grid")]
	public GridLength RowHeight
	{
		get => (GridLength)GetValue(RowHeightProperty);
		set => SetValue(RowHeightProperty, value);
	}
	public static readonly DependencyProperty RowHeightProperty =
			DependencyProperty.Register(nameof(RowHeight), typeof(GridLength), typeof(AutoGrid),
				new FrameworkPropertyMetadata(default(GridLength),
					FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
					new PropertyChangedCallback((d, e) =>
					{
						if (d is AutoGrid ag) ag.PerformLayout(true);
					})));
	private int GetChildColumnIndex(UIElement child) => Children.IndexOf(child) % ColumnDefinitions.Count;
	private int GetChildRowIndex(UIElement child) => Children.IndexOf(child) / ColumnDefinitions.Count;
	internal void PerformLayout(bool force = false)
	{
		if (mustRelayout || force)
		{
			mustRelayout = false;
			RowDefinitions.Clear();
			if (ColumnDefinitions.Count == 0) return;
			for (var i = 0; i < System.Math.Ceiling((decimal)Children.Count / ColumnDefinitions.Count); i++)
			{
				RowDefinitions.Add(new RowDefinition { Height = RowHeight });
			}
			foreach (UIElement? child in Children)
			{
				if (child != null)
				{
					var colIndex = GetChildColumnIndex(child);
					child.SetValue(Grid.ColumnProperty, colIndex);
					child.SetValue(Grid.RowProperty, GetChildRowIndex(child));
					if (ColumnDefinitions[colIndex] is AutoColumnDefinition acd)
					{
						child.SetValue(FrameworkElement.HorizontalAlignmentProperty, acd.HorizontalContentAlignment);
						child.SetValue(FrameworkElement.VerticalAlignmentProperty, acd.VerticalContentAlignment);
						child.SetValue(FrameworkElement.MarginProperty, acd.ContentMargin);
					}
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
		mustRelayout = true;
		base.OnVisualChildrenChanged(visualAdded, visualRemoved);
	}
	#endregion Overrides
}
public class AutoColumnDefinition : ColumnDefinition
{
	[Category("Layout"), Description("Define horizontal alignment for all elements of this column")]
	public HorizontalAlignment HorizontalContentAlignment
	{
		get => (HorizontalAlignment)GetValue(HorizontalContentAlignmentProperty);
		set => SetValue(HorizontalContentAlignmentProperty, value);
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
		get => (VerticalAlignment)GetValue(VerticalContentAlignmentProperty);
		set => SetValue(VerticalContentAlignmentProperty, value);
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
		get => (Thickness)GetValue(ContentMarginProperty);
		set => SetValue(ContentMarginProperty, value);
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