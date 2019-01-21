using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Fuxion.Windows.Helpers
{
    public static class FocusHelper
    {
        public static bool GetSetKeyboardFocusOnLoad(DependencyObject obj)
        {
            return (bool)obj.GetValue(SetKeyboardFocusOnLoadProperty);
        }
        public static void SetSetKeyboardFocusOnLoad(DependencyObject obj, bool value)
        {
            obj.SetValue(SetKeyboardFocusOnLoadProperty, value);
        }

        public static readonly DependencyProperty SetKeyboardFocusOnLoadProperty =
         DependencyProperty.RegisterAttached("SetKeyboardFocusOnLoad", typeof(bool), typeof(FocusHelper),
         new PropertyMetadata(false, SetKeyboardFocusOnLoadChanged));

        private static void SetKeyboardFocusOnLoadChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                UIElement ui = sender as UIElement;
                if (ui != null)
                {
                    ui.Dispatcher.BeginInvoke(DispatcherPriority.Input, new System.Threading.ThreadStart(delegate ()
                    {
                        Keyboard.Focus(ui);
                    }));
                }
            }
        }
    }
}
