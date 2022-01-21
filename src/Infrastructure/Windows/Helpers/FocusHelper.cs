namespace Fuxion.Windows.Helpers;

using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

public static class FocusHelper
{
	public static bool GetSetKeyboardFocusOnLoad(DependencyObject obj) => (bool)obj.GetValue(SetKeyboardFocusOnLoadProperty);
	public static void SetSetKeyboardFocusOnLoad(DependencyObject obj, bool value) => obj.SetValue(SetKeyboardFocusOnLoadProperty, value);

	public static readonly DependencyProperty SetKeyboardFocusOnLoadProperty =
	 DependencyProperty.RegisterAttached("SetKeyboardFocusOnLoad", typeof(bool), typeof(FocusHelper),
	 new PropertyMetadata(false, SetKeyboardFocusOnLoadChanged));

	private static void SetKeyboardFocusOnLoadChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if ((bool)e.NewValue)
		{
			if (sender is UIElement ui)
			{
				ui.Dispatcher.BeginInvoke(DispatcherPriority.Input, new System.Threading.ThreadStart(delegate ()
				{
					Keyboard.Focus(ui);
				}));
			}
		}
	}
}