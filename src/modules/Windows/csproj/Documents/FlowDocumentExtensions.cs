using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Fuxion.Windows.Documents
{
    public static class FlowDocumentExtensions
    {
		static bool IsBasicType(Type type)
		{
			return
				type == typeof(short) ||
				type == typeof(ushort) ||
				type == typeof(int) ||
				type == typeof(uint) ||
				type == typeof(long) ||
				type == typeof(ulong) ||
				type == typeof(float) ||
				type == typeof(double) ||
				type == typeof(decimal) ||

				type == typeof(bool) ||

				type == typeof(char) ||
				type == typeof(string) ||

				type == typeof(byte) ||
				type == typeof(sbyte) ||

				type == typeof(DateTime) ||
				type == typeof(TimeSpan) ||
				type == typeof(Guid);
		}
		public static IEnumerable<Block> ToBlocks(this object obj)
		{
			if (obj == null) throw new ArgumentNullException(nameof(obj));
			foreach (var pro in obj.GetType().GetProperties().Where(p => !p.GetIndexParameters().Any()).OrderBy(p => p.Name))
				yield return ProcessProperty(pro.GetValue(obj), pro.Name, pro.PropertyType);
		}
		internal static Block ProcessProperty(this object obj, string name, Type type)
		{
			var res = new List<Block>();
			var objType = obj?.GetType() ?? type;
			return IsBasicType(objType)
				? (Block)new Paragraph().Transform(p =>
				{
					p.Inlines.Add(new Bold(new Run($"{name} = ")));
					var valStr = obj?.ToString();
					if (valStr?.Contains('\r') ?? false)
						p.Inlines.Add(new LineBreak());
					p.Inlines.Add(new Run(obj?.ToString() ?? "null"));
				})
				: new CollapsibleSection(obj, name, type, false);
		}
	}
	internal class CollapsibleSection : Section
	{
		public CollapsibleSection(object obj, string name, Type type, bool expandEnumerable)
		{
			var objType = obj?.GetType() ?? type;
			var isEnumerable = obj is IEnumerable || typeof(IEnumerable).IsAssignableFrom(objType);
			int enumerableCount = 0;
			if (isEnumerable)
				foreach (var item in obj as IEnumerable)
					enumerableCount++;

			var button = new ToggleButton();
			button.Click += (s, e) =>
			{
				try
				{
					var but = s as ToggleButton;
					if (but.IsChecked ?? false)
					{
						button.Content = "▲";
						if (expandEnumerable)
						{
							int counter = 0;
							foreach (var item in obj as IEnumerable)
							{
								var list = new List();
								list.ListItems.Add(new ListItem().Transform(i => i.Blocks.Add(item.ProcessProperty($"{name} [{counter++}]", item.GetType()))));
								Blocks.Add(list);
							}
						}
						else
						{
							var list = new List();
							foreach (var pro in obj.GetType().GetProperties().Where(p => !p.GetIndexParameters().Any()).OrderBy(p => p.Name))
							{
								try
								{
									list.ListItems.Add(new ListItem().Transform(item => item.Blocks.Add(FlowDocumentExtensions.ProcessProperty(pro.GetValue(obj), pro.Name, pro.PropertyType))));
								}
								catch (Exception ex)
								{
									list.ListItems.Add(new ListItem()
										.Transform(item => item.Blocks.Add(new Paragraph()
										.Transform(p => p.Inlines.Add(new Bold(new Run($"Error el desplegar el item:\r\n'{ex.GetType().Name}': {ex.Message}")
										.Transform<Run>(r => r.Foreground = Brushes.Red)))))));
								}
							}
							if (isEnumerable)
							{
								CollapsibleSection sec = new CollapsibleSection(obj, name, type, true);
								list.ListItems.Add(new ListItem().Transform(item => item.Blocks.Add(sec)));
							}
							Blocks.Add(list);
						}
					}
					else
					{
						button.Content = "▼";
						ResetBlocks();
					}
				}
				catch (Exception ex)
				{
					ResetBlocks();
					Blocks.Add(new Paragraph().Transform(p => p.Inlines.Add(new Bold(new Run($"Error el desplegar el item:\r\n'{ex.GetType().Name}': {ex.Message}")
						.Transform<Run>(r => r.Foreground = Brushes.Red)))));
				}
			};
			button.Content = obj != null ? "▼" : "◊"; // "■"; // "●";
			var inlineContainer = new InlineUIContainer(button);
			inlineContainer.BaselineAlignment = BaselineAlignment.Center;
			inlineContainer.Cursor = Cursors.Hand;
			var header = new Paragraph();
			header.Inlines.Add(inlineContainer);
			if (expandEnumerable)
			{
				header.Inlines.Add(new Bold(new Run("Listado ")));
				header.Foreground = Brushes.DarkBlue;
			}
			else
				header.Inlines.Add(new Bold(new Run(name)));
			if (isEnumerable)
				header.Inlines.Add(new Bold(new Run($" [{enumerableCount} elemento{(enumerableCount == 1 ? "" : "s")}]")));
			if (obj == null)
			{
				header.Inlines.Add(new Bold(new Run(" = ")));
				header.Inlines.Add(new Run("null"));
				button.IsHitTestVisible = false;
			}
			Blocks.Add(header);
		}
		void ResetBlocks()
		{
			var first = Blocks.First();
			Blocks.Clear();
			Blocks.Add(first);
		}
	}
}
