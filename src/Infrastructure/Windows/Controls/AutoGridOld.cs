using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Fuxion.Windows.Controls
{
	/// <summary>
	/// Defines a flexible grid area that consists of columns and rows.
	/// Depending on the orientation, either the rows or the columns are auto-generated,
	/// and the children's position is set according to their index.
	///
	/// Partially based on work at http://rachel53461.wordpress.com/2011/09/17/wpf-grids-rowcolumn-count-properties/
	/// </summary>
	public class AutoGridOld : Grid
	{
		private int GetChildColumnIndex(UIElement child)
		{
			var index = Children.IndexOf(child);
			int colIndex = 0;
			if (Orientation == Orientation.Horizontal)
				colIndex = index % ColumnDefinitions.Count;
			else
				colIndex = (int)System.Math.Floor((decimal)index / RowDefinitions.Count);
			return colIndex;
		}
		#region ColumnHorizontalContentAlignments
		[Category("Layout"), Description("Define all columns horizontal content alignments use dot-comma separated notation. If only one, it will be use for all columns")]
		public string ColumnHorizontalContentAlignments
		{
			get { return (string)GetValue(ColumnHorizontalContentAlignmentsProperty); }
			set { SetValue(ColumnHorizontalContentAlignmentsProperty, value); }
		}
		public static readonly DependencyProperty ColumnHorizontalContentAlignmentsProperty =
			DependencyProperty.Register(nameof(ColumnHorizontalContentAlignments), typeof(string), typeof(AutoGridOld),
				new FrameworkPropertyMetadata(null,
					FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
					new PropertyChangedCallback((d, e) =>
					{
						var grid = d as AutoGridOld;
						foreach (UIElement child in grid.Children)
							child.SetValue(HorizontalAlignmentProperty, grid.SearchHorizontalContentAlignmentOfChild(child));
					})));
		private object SearchHorizontalContentAlignmentOfChild(UIElement child)
		{
			if (string.IsNullOrWhiteSpace(ColumnHorizontalContentAlignments))
				return DependencyProperty.UnsetValue;
			var splits = ColumnHorizontalContentAlignments.Split(';')
				.Select(str => (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), str))
				.ToArray();
			if (splits.Length == 1)
				return splits[0];
			if (ColumnDefinitions.Count != splits.Length) throw new ArgumentException($"{nameof(ColumnHorizontalContentAlignments)} must have one element or same number of elements than number of columns");
			return splits[GetChildColumnIndex(child)];
		}
		#endregion
		#region ColumnVerticalContentAlignments
		[Category("Layout"), Description("Define all columns vertical content alignments use dot-comma separated notation. If only one, it will be use for all columns")]
		public string ColumnVerticalContentAlignments
		{
			get { return (string)GetValue(ColumnVerticalContentAlignmentsProperty); }
			set { SetValue(ColumnVerticalContentAlignmentsProperty, value); }
		}
		public static readonly DependencyProperty ColumnVerticalContentAlignmentsProperty =
			DependencyProperty.Register(nameof(ColumnVerticalContentAlignments), typeof(string), typeof(AutoGridOld),
				new FrameworkPropertyMetadata(null,
					FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
					new PropertyChangedCallback((d, e) =>
					{
						var grid = d as AutoGridOld;
						foreach (UIElement child in grid.Children)
							child.SetValue(VerticalAlignmentProperty, grid.SearchVerticalContentAlignmentOfChild(child));
					})));
		private object SearchVerticalContentAlignmentOfChild(UIElement child)
		{
			if (string.IsNullOrWhiteSpace(ColumnVerticalContentAlignments))
				return DependencyProperty.UnsetValue;
			var splits = ColumnVerticalContentAlignments.Split(';')
				.Select(str => (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), str))
				.ToArray();
			if (splits.Length == 1)
				return splits[0];
			if (ColumnDefinitions.Count != splits.Length) throw new ArgumentException($"{nameof(ColumnVerticalContentAlignments)} must have one element or same number of elements than number of columns");
			return splits[GetChildColumnIndex(child)];
		}
		#endregion
		#region ColumnContentMargins
		[Category("Layout"), Description("Define all columns content margins use dot-comma Thickness separated notation. If only one, it will be use for all columns")]
		public string ColumnContentMargins
		{
			get { return (string)GetValue(ColumnContentMarginsProperty); }
			set { SetValue(ColumnContentMarginsProperty, value); }
		}
		public static readonly DependencyProperty ColumnContentMarginsProperty =
			DependencyProperty.Register(nameof(ColumnContentMargins), typeof(string), typeof(AutoGridOld),
				new FrameworkPropertyMetadata(null,
					FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
					new PropertyChangedCallback((d, e) =>
					{
						var grid = d as AutoGridOld;
						foreach (UIElement child in grid.Children)
							child.SetValue(MarginProperty, grid.SearchContentMarginOfChild(child));
					})));
		private object SearchContentMarginOfChild(UIElement child)
		{
			if (string.IsNullOrWhiteSpace(ColumnContentMargins))
				return DependencyProperty.UnsetValue;
			var splits = ColumnContentMargins.Split(';')
				.Select(str =>
				{
					if (!str.Contains(',') && double.TryParse(str, out double val))
						return new Thickness(val);
					else
					{
						var sp = str.Split(',');
						if (sp.Length == 2
							&& double.TryParse(sp[0], out double value1)
							&& double.TryParse(sp[1], out double value2))
						{
							return new Thickness(value1, value2, value1, value2);
						}
						else if (sp.Length == 4
						   && double.TryParse(sp[0], out double val1)
						   && double.TryParse(sp[1], out double val2)
						   && double.TryParse(sp[2], out double val3)
						   && double.TryParse(sp[3], out double val4))
						{
							return new Thickness(val1, val2, val3, val4);
						}
						throw new ArgumentException($"Value '{str}' is not valid for {nameof(ColumnContentMargins)}");
					}
				})
				.ToArray();
			if (splits.Length == 1)
				return splits[0];
			if (ColumnDefinitions.Count != splits.Length) throw new ArgumentException($"{nameof(ColumnContentMargins)} must have one element or same number of elements than number of columns");
			return splits[GetChildColumnIndex(child)];
		}
		#endregion
		#region Columns
		[Category("Layout"), Description("Defines all columns using comma separated grid length notation")]
		public string Columns
		{
			get { return (string)GetValue(ColumnsProperty); }
			set { SetValue(ColumnsProperty, value); }
		}
		public static readonly DependencyProperty ColumnsProperty =
			DependencyProperty.RegisterAttached(nameof(Columns), typeof(string), typeof(AutoGridOld),
				new FrameworkPropertyMetadata("",
					FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
					new PropertyChangedCallback(ColumnsChanged)));
		public static void ColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if ((string)e.NewValue == string.Empty)
				return;

			var grid = d as AutoGridOld;
			grid.ColumnDefinitions.Clear();

			var lens = ParseGridLengths((string)e.NewValue);
			foreach (var len in lens)
				grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = len });
		}
		public static GridLength[] ParseGridLengths(string text)
			=> text.Split(';')
				.Select(str =>
				{
					// ratio
					if (str.Contains('*'))
					{
						var strVal = str.Replace("*", "");
						if (string.IsNullOrWhiteSpace(strVal))
							strVal = "1";
						if (!double.TryParse(strVal, out double val))
							throw new ArgumentException($"Value '{str}' cannot be parsed as valid GridLength");
						return new GridLength(val, GridUnitType.Star);
					}
					// auto
					if (str.ToLower().Trim() == "auto")
						return GridLength.Auto;
					// pixels
					if (double.TryParse(str, out double value))
						return new GridLength(value);
					else
						throw new ArgumentException($"Value '{str}' cannot be parsed as valid GridLength");
				})
				.ToArray();
		#endregion
		#region ColumnSharedSizeGroups
		[Category("Layout"), Description("Define all columns shared size groups use dot-comma separated notation. If only one, it will be use for all columns")]
		public string ColumnSharedSizeGroups
		{
			get { return (string)GetValue(ColumnSharedSizeGroupsProperty); }
			set { SetValue(ColumnSharedSizeGroupsProperty, value); }
		}
		public static readonly DependencyProperty ColumnSharedSizeGroupsProperty =
			DependencyProperty.Register(nameof(ColumnSharedSizeGroups), typeof(string), typeof(AutoGridOld),
				new FrameworkPropertyMetadata(null,
					FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
					new PropertyChangedCallback((d, e) =>
					{
						var grid = d as AutoGridOld;
						if(e.NewValue != null)
						{
							if (!string.IsNullOrWhiteSpace(e.NewValue.ToString()))
							{
								var splits = e.NewValue.ToString().Split(';');
								if(splits.Length != grid.ColumnDefinitions.Count) throw new ArgumentException($"{nameof(ColumnSharedSizeGroups)} must have same number of elements than number of columns");
								for(int i = 0; i < grid.ColumnDefinitions.Count; i++)
								{
									if (!string.IsNullOrWhiteSpace(splits[i].Trim()))
										grid.ColumnDefinitions[i].SetValue(ColumnDefinition.SharedSizeGroupProperty, splits[i].Trim());
								}
							}
						}
					})));
		#endregion

		// Pending cleanup 

		#region ColumnWidth
		/// <summary>
		/// Gets or sets the fixed column width
		/// </summary>
		[Category("Layout"), Description("Presets the width of all columns set using the ColumnCount property")]
		public GridLength ColumnWidth
		{
			get { return (GridLength)GetValue(ColumnWidthProperty); }
			set { SetValue(ColumnWidthProperty, value); }
		}
		public static readonly DependencyProperty ColumnWidthProperty =
			DependencyProperty.RegisterAttached("ColumnWidth", typeof(GridLength), typeof(AutoGridOld),
				new FrameworkPropertyMetadata(GridLength.Auto,
					FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
					new PropertyChangedCallback(FixedColumnWidthChanged)));
		/// <summary>
		/// Handle the fixed column width changed event
		/// </summary>
		public static void FixedColumnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var grid = d as AutoGridOld;

			// add a default column if missing
			if (grid.ColumnDefinitions.Count == 0)
				grid.ColumnDefinitions.Add(new ColumnDefinition());

			// set all existing columns to this width
			for (int i = 0; i < grid.ColumnDefinitions.Count; i++)
				grid.ColumnDefinitions[i].Width = (GridLength)e.NewValue;
		}
		#endregion
		/// <summary>
		/// Gets or sets a value indicating whether the children are automatically indexed.
		/// <remarks>
		/// The default is <c>true</c>.
		/// Note that if children are already indexed, setting this property to <c>false</c> will not remove their indices.
		/// </remarks>
		/// </summary>
		[Category("Layout"), Description("Set to false to disable the auto layout functionality")]
		public bool IsAutoIndexing
		{
			get { return (bool)GetValue(IsAutoIndexingProperty); }
			set { SetValue(IsAutoIndexingProperty, value); }
		}

		/// <summary>
		/// Gets or sets the orientation.
		/// <remarks>The default is Vertical.</remarks>
		/// </summary>
		/// <value>The orientation.</value>
		[Category("Layout"),
		 Description(
			 "Defines the directionality of the autolayout. Use vertical for a column first layout, horizontal for a row first layout."
			 )]
		public Orientation Orientation
		{
			get { return (Orientation)GetValue(OrientationProperty); }
			set { SetValue(OrientationProperty, value); }
		}

		/// <summary>
		/// Gets or sets the fixed row height
		/// </summary>
		[Category("Layout"), Description("Presets the height of all rows set using the RowCount property")]
		public GridLength RowHeight
		{
			get { return (GridLength)GetValue(RowHeightProperty); }
			set { SetValue(RowHeightProperty, value); }
		}

		/// <summary>
		/// Gets or sets the rows
		/// </summary>
		[Category("Layout"), Description("Defines all rows using comma separated grid length notation")]
		public string Rows
		{
			get { return (string)GetValue(RowsProperty); }
			set { SetValue(RowsProperty, value); }
		}

		// AutoIndex attached property

		public static readonly DependencyProperty AutoIndexProperty = DependencyProperty.RegisterAttached(
			"AutoIndex", typeof(bool), typeof(AutoGridOld), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

		public static void SetAutoIndex(DependencyObject element, bool value)
		{
			element.SetValue(AutoIndexProperty, value);
		}

		public static bool GetAutoIndex(DependencyObject element)
		{
			return (bool)element.GetValue(AutoIndexProperty);
		}

		// RowHeight override attached property

		public static readonly DependencyProperty RowHeightOverrideProperty = DependencyProperty.RegisterAttached(
			"RowHeightOverride", typeof(GridLength?), typeof(AutoGridOld),
			new FrameworkPropertyMetadata(default(GridLength?), FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

		public static void SetRowHeightOverride(DependencyObject element, GridLength? value)
		{
			element.SetValue(RowHeightOverrideProperty, value);
		}

		public static GridLength? GetRowHeightOverride(DependencyObject element)
		{
			return (GridLength?)element.GetValue(RowHeightOverrideProperty);
		}

		public static readonly DependencyProperty ColumnWidthOverrideProperty = DependencyProperty.RegisterAttached(
			"ColumnWidthOverride", typeof(GridLength?), typeof(AutoGridOld),
			new FrameworkPropertyMetadata(default(GridLength?), FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

		public static void SetColumnWidthOverride(DependencyObject element, GridLength? value)
		{
			element.SetValue(ColumnWidthOverrideProperty, value);
		}

		public static GridLength? GetColumnWidthOverride(DependencyObject element)
		{
			return (GridLength?)element.GetValue(ColumnWidthOverrideProperty);
		}

		/// <summary>
		/// Handles the column count changed event
		/// </summary>
		public static void ColumnCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if ((int)e.NewValue < 0)
				return;

			var grid = d as AutoGridOld;

			// look for an existing column definition for the height
			var width = GridLength.Auto;
			if (grid.ColumnDefinitions.Count > 0)
				width = grid.ColumnDefinitions[0].Width;

			// clear and rebuild
			grid.ColumnDefinitions.Clear();
			for (int i = 0; i < (int)e.NewValue; i++)
				grid.ColumnDefinitions.Add(
					new ColumnDefinition() { Width = width });
		}
		/// <summary>
		/// Handle the fixed row height changed event
		/// </summary>
		public static void FixedRowHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var grid = d as AutoGridOld;

			// add a default row if missing
			if (grid.RowDefinitions.Count == 0)
				grid.RowDefinitions.Add(new RowDefinition());

			// set all existing rows to this height
			for (int i = 0; i < grid.RowDefinitions.Count; i++)
				grid.RowDefinitions[i].Height = (GridLength)e.NewValue;
		}
		/// <summary>
		/// Handles the row count changed event
		/// </summary>
		public static void RowCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if ((int)e.NewValue < 0)
				return;

			var grid = d as AutoGridOld;

			// look for an existing row to get the height
			var height = GridLength.Auto;
			if (grid.RowDefinitions.Count > 0)
				height = grid.RowDefinitions[0].Height;

			// clear and rebuild
			grid.RowDefinitions.Clear();
			for (int i = 0; i < (int)e.NewValue; i++)
				grid.RowDefinitions.Add(
					new RowDefinition() { Height = height });
		}

		/// <summary>
		/// Handle the rows changed event
		/// </summary>
		public static void RowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if ((string)e.NewValue == string.Empty)
				return;

			var grid = d as AutoGridOld;
			grid.RowDefinitions.Clear();

			var defs = ParseGridLengths((string)e.NewValue);
			foreach (var def in defs)
				grid.RowDefinitions.Add(new RowDefinition() { Height = def });
		}







		/// <summary>
		/// Handled the redraw properties changed event
		/// </summary>
		static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((AutoGridOld)d)._shouldReindex = true;
		}

		/// <summary>
		/// Apply child margins and layout effects such as alignment
		/// </summary>
		void ApplyChildLayout(UIElement child)
		{
			var margin = SearchContentMarginOfChild(child);
			if (margin != null && margin != DependencyProperty.UnsetValue)
				child.SetIfDefault(MarginProperty, (Thickness)margin);
			var horizontalAlignment = SearchHorizontalContentAlignmentOfChild(child);
			if (horizontalAlignment != null && horizontalAlignment != DependencyProperty.UnsetValue)
				child.SetIfDefault(HorizontalAlignmentProperty, (HorizontalAlignment)horizontalAlignment);
			var verticalAlignment = SearchVerticalContentAlignmentOfChild(child);
			if (verticalAlignment != null && verticalAlignment != DependencyProperty.UnsetValue)
				child.SetIfDefault(VerticalAlignmentProperty, (VerticalAlignment)verticalAlignment);
		}

		/// <summary>
		/// Clamp a value to its maximum.
		/// </summary>
		int Clamp(int value, int max)
		{
			return (value > max) ? max : value;
		}

		public void PerformLayout()
		{
			bool isVertical = Orientation == Orientation.Vertical;

			if (_shouldReindex || (IsAutoIndexing &&
				((isVertical && _rowOrColumnCount != ColumnDefinitions.Count) ||
				(!isVertical && _rowOrColumnCount != RowDefinitions.Count))))
			{
				_shouldReindex = false;

				if (IsAutoIndexing)
				{
					_rowOrColumnCount = (ColumnDefinitions.Count != 0) ? ColumnDefinitions.Count : RowDefinitions.Count;
					if (_rowOrColumnCount == 0) _rowOrColumnCount = 1;

					int cellCount = 0;
					foreach (UIElement child in Children)
					{
						if (GetAutoIndex(child) == false)
						{
							continue;
						}
						cellCount += (ColumnDefinitions.Count != 0) ? Grid.GetColumnSpan(child) : Grid.GetRowSpan(child);
					}

					//  Update the number of rows/columns
					if (ColumnDefinitions.Count != 0)
					{
						var newRowCount = (int)System.Math.Ceiling((double)cellCount / (double)_rowOrColumnCount);
						while (RowDefinitions.Count < newRowCount)
						{
							var rowDefinition = new RowDefinition();
							rowDefinition.Height = RowHeight;
							RowDefinitions.Add(rowDefinition);
						}
						if (RowDefinitions.Count > newRowCount)
						{
							RowDefinitions.RemoveRange(newRowCount, RowDefinitions.Count - newRowCount);
						}
					}
					else // rows defined
					{
						var newColumnCount = (int)System.Math.Ceiling((double)cellCount / (double)_rowOrColumnCount);
						while (ColumnDefinitions.Count < newColumnCount)
						{
							var columnDefinition = new ColumnDefinition();
							columnDefinition.Width = ColumnWidth;
							ColumnDefinitions.Add(columnDefinition);
						}
						if (ColumnDefinitions.Count > newColumnCount)
						{
							ColumnDefinitions.RemoveRange(newColumnCount, ColumnDefinitions.Count - newColumnCount);
						}
					}
				}

				//  Update children indices
				int cellPosition = 0;
				var cellsToSkip = new Queue<int>();
				foreach (UIElement child in Children)
				{
					if (IsAutoIndexing && GetAutoIndex(child) == true)
					{
						if (cellsToSkip.Any() && cellsToSkip.Peek() == cellPosition)
						{
							cellsToSkip.Dequeue();
							cellPosition += 1;
						}

						if (!isVertical) // horizontal (default)
						{
							var rowIndex = cellPosition / ColumnDefinitions.Count;
							Grid.SetRow(child, rowIndex);

							var columnIndex = cellPosition % ColumnDefinitions.Count;
							Grid.SetColumn(child, columnIndex);

							var rowSpan = Grid.GetRowSpan(child);
							if (rowSpan > 1)
							{
								Enumerable.Range(1, rowSpan).ToList()
									.ForEach(x => cellsToSkip.Enqueue(cellPosition + ColumnDefinitions.Count * x));
							}

							var overrideRowHeight = AutoGridOld.GetRowHeightOverride(child);
							if (overrideRowHeight != null)
							{
								RowDefinitions[rowIndex].Height = overrideRowHeight.Value;
							}

							var overrideColumnWidth = AutoGridOld.GetColumnWidthOverride(child);
							if (overrideColumnWidth != null)
							{
								ColumnDefinitions[columnIndex].Width = overrideColumnWidth.Value;
							}

							cellPosition += Grid.GetColumnSpan(child);
						}
						else
						{
							var rowIndex = cellPosition % RowDefinitions.Count;
							Grid.SetRow(child, rowIndex);

							var columnIndex = cellPosition / RowDefinitions.Count;
							Grid.SetColumn(child, columnIndex);

							var columnSpan = Grid.GetColumnSpan(child);
							if (columnSpan > 1)
							{
								Enumerable.Range(1, columnSpan).ToList()
									.ForEach(x => cellsToSkip.Enqueue(cellPosition + RowDefinitions.Count * x));
							}

							var overrideRowHeight = AutoGridOld.GetRowHeightOverride(child);
							if (overrideRowHeight != null)
							{
								RowDefinitions[rowIndex].Height = overrideRowHeight.Value;
							}

							var overrideColumnWidth = AutoGridOld.GetColumnWidthOverride(child);
							if (overrideColumnWidth != null)
							{
								ColumnDefinitions[columnIndex].Width = overrideColumnWidth.Value;
							}

							cellPosition += Grid.GetRowSpan(child);
						}
					}

					// Set margin and alignments
					var margin = SearchContentMarginOfChild(child);
					if (margin != null && margin != DependencyProperty.UnsetValue)
						DependencyHelpers.SetIfDefault(child, MarginProperty, (Thickness)margin);
					var horizontalAlignment = SearchHorizontalContentAlignmentOfChild(child);
					if (horizontalAlignment != null && horizontalAlignment != DependencyProperty.UnsetValue)
						DependencyHelpers.SetIfDefault(child, HorizontalAlignmentProperty, (HorizontalAlignment)horizontalAlignment);
					var verticalAlignment = SearchVerticalContentAlignmentOfChild(child);
					if (verticalAlignment != null && verticalAlignment != DependencyProperty.UnsetValue)
						DependencyHelpers.SetIfDefault(child, VerticalAlignmentProperty, (VerticalAlignment)verticalAlignment);
				}
			}
		}











		public static readonly DependencyProperty IsAutoIndexingProperty =
			DependencyProperty.Register("IsAutoIndexing", typeof(bool), typeof(AutoGridOld),
				new FrameworkPropertyMetadata(true,
					FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
					new PropertyChangedCallback(OnPropertyChanged)));

		public static readonly DependencyProperty OrientationProperty =
			DependencyProperty.Register("Orientation", typeof(Orientation), typeof(AutoGridOld),
				new FrameworkPropertyMetadata(Orientation.Horizontal,
					FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
					new PropertyChangedCallback(OnPropertyChanged)));

		public static readonly DependencyProperty RowHeightProperty =
			DependencyProperty.RegisterAttached("RowHeight", typeof(GridLength), typeof(AutoGridOld),
				new FrameworkPropertyMetadata(GridLength.Auto,
					FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
					new PropertyChangedCallback(FixedRowHeightChanged)));

		public static readonly DependencyProperty RowsProperty =
			DependencyProperty.RegisterAttached("Rows", typeof(string), typeof(AutoGridOld),
				new FrameworkPropertyMetadata("",
					FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
					new PropertyChangedCallback(RowsChanged)));

		bool _shouldReindex = true;
		int _rowOrColumnCount;

		#region Overrides

		/// <summary>
		/// Measures the children of a <see cref="T:System.Windows.Controls.Grid"/> in anticipation of arranging them during the <see cref="M:ArrangeOverride"/> pass.
		/// </summary>
		/// <param name="constraint">Indicates an upper limit size that should not be exceeded.</param>
		/// <returns>
		/// 	<see cref="Size"/> that represents the required size to arrange child content.
		/// </returns>
		protected override Size MeasureOverride(Size constraint)
		{
			this.PerformLayout();
			return base.MeasureOverride(constraint);
		}

		/// <summary>
		/// Called when the visual children of a <see cref="Grid"/> element change.
		/// <remarks>Used to mark that the grid children have changed.</remarks>
		/// </summary>
		/// <param name="visualAdded">Identifies the visual child that's added.</param>
		/// <param name="visualRemoved">Identifies the visual child that's removed.</param>
		protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
		{
			_shouldReindex = true;
			base.OnVisualChildrenChanged(visualAdded, visualRemoved);
		}

		#endregion Overrides
	}
	/// <summary>
	/// Encapsulates methods for dealing with dependency objects and properties.
	/// </summary>
	public static class DependencyHelpers
	{
		/// <summary>
		/// Gets the dependency property according to its name.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns></returns>
		public static DependencyProperty GetDependencyProperty(Type type, string propertyName)
		{
			DependencyProperty prop = null;

			if (type != null)
			{
				FieldInfo fieldInfo = type.GetField(propertyName + "Property", BindingFlags.Static | BindingFlags.Public);

				if (fieldInfo != null)
				{
					prop = fieldInfo.GetValue(null) as DependencyProperty;
				}
			}

			return prop;
		}

		/// <summary>
		/// Retrieves a <see cref="DependencyProperty"/> using reflection.
		/// </summary>
		/// <param name="o"></param>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public static DependencyProperty GetDependencyProperty(this DependencyObject o, string propertyName)
		{
			DependencyProperty prop = null;

			if (o != null)
			{
				prop = GetDependencyProperty(o.GetType(), propertyName);
			}

			return prop;
		}

		/// <summary>
		/// Sets the value of the <paramref name="property"/> only if it hasn't been explicitely set.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="o">The object.</param>
		/// <param name="property">The property.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static bool SetIfDefault<T>(this DependencyObject o, DependencyProperty property, T value)
		{
			if (o == null) throw new ArgumentNullException("o", "DependencyObject cannot be null");
			if (property == null) throw new ArgumentNullException("property", "DependencyProperty cannot be null");

			if (!property.PropertyType.IsAssignableFrom(typeof(T)))
			{
				throw new ArgumentException(
					string.Format("Expected {0} to be of type {1} but was {2}",
						property.Name, typeof(T).Name, property.PropertyType));
			}

			if (DependencyPropertyHelper.GetValueSource(o, property).BaseValueSource == BaseValueSource.Default)
			{
				o.SetValue(property, value);

				return true;
			}

			return false;
		}
	}
}
