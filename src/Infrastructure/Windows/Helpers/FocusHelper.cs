using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Fuxion.Windows.Helpers;

public static class FocusHelper
{
	public static readonly DependencyProperty SetKeyboardFocusOnLoadProperty =
		DependencyProperty.RegisterAttached("SetKeyboardFocusOnLoad", typeof(bool), typeof(FocusHelper), new(false, SetKeyboardFocusOnLoadChanged));
	public static bool GetSetKeyboardFocusOnLoad(DependencyObject obj) => (bool)obj.GetValue(SetKeyboardFocusOnLoadProperty);
	public static void SetSetKeyboardFocusOnLoad(DependencyObject obj, bool value) => obj.SetValue(SetKeyboardFocusOnLoadProperty, value);
	static void SetKeyboardFocusOnLoadChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if ((bool)e.NewValue)
			if (sender is UIElement ui)
				ui.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(delegate { Keyboard.Focus(ui); }));
	}
}