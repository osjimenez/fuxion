using Fuxion.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;

namespace Fuxion.Windows.Controls
{
    public class OverlayManager
    {
        public OverlayManager(Overlay overlay) {
            this.overlay = overlay;
        }
        Overlay overlay;
        public void ShowOverlay(IOverlayData data)
        {
            overlay.Children.Add(data.OverlayControl);
        }
        public bool HideOverlay(IOverlayData data)
        {
            return overlay.Children.Remove(data.OverlayControl);
        }
        public void HideCurrentOverlay()
        {
            if (overlay.Children.Count > 0)
                overlay.Children.RemoveAt(0);
        }
    }
    public interface IOverlayData<TControl> : IOverlayData
        where TControl : UIElement
    {
        new TControl OverlayControl { get; }
    }
    public interface IOverlayData
    {
        UIElement OverlayControl { get; }
    }
    public abstract class OverlayData<TControl> : Notifier<OverlayData<TControl>>, IOverlayData<TControl>
        where TControl : FrameworkElement
    {
        public OverlayData(TControl control) { this.control = control; control.DataContext = this; }
        TControl control;
        UIElement IOverlayData.OverlayControl { get { return control; } }
        TControl IOverlayData<TControl>.OverlayControl { get { return control; } }
    }
    public enum OverlayDialogType
    {
        Normal,
        Validation,
        Error
    }
    public class OverlayDialogButton : Notifier<OverlayDialogButton>
    {
        public string Text { get { return GetValue<string>(); } set { SetValue(value); } }
        public bool IsCancel { get { return GetValue<bool>(); } set { SetValue(value); } }
        public bool IsDefault { get { return GetValue<bool>(); } set { SetValue(value); } }
        public bool IsEnabled { get { return GetValue(() => true); } set { SetValue(value); } }
        public bool IsVisible { get { return GetValue(() => true); } set { SetValue(value); } }
        public double? MinWidth { get { return GetValue<double?>(); } set { SetValue(value); } }
        public double? MaxWidth { get { return GetValue<double?>(); } set { SetValue(value); } }
        public double? Width { get { return GetValue<double?>(); } set { SetValue(value); } }
        public ICommand Command { get { return GetValue<ICommand>(); } set { SetValue(value); } }
        public Action<OverlayDialogData> OnCommand { get { return GetValue<Action<OverlayDialogData>>(); } set { SetValue(value); } }
    }
    public class OverlayDialogData : OverlayData<OverlayDialog>
    {
        public OverlayDialogData():base(new OverlayDialog()) { }
        //public override FrameworkElement ContentControl { get { return new OverlayDialog(this); } }
        public string Title { get { return GetValue<string>(); } set { SetValue(value); } }
        public OverlayDialogType Type { get { return GetValue<OverlayDialogType>(); } set { SetValue(value); } }

        public double? DialogMinWidth { get { return GetValue<double?>(); } set { SetValue(value); } }
        public double? DialogMaxWidth { get { return GetValue<double?>(); } set { SetValue(value); } }
        public double? DialogWidth { get { return GetValue<double?>(); } set { SetValue(value); } }
        public double? DialogMinHeight { get { return GetValue<double?>(); } set { SetValue(value); } }
        public double? DialogMaxHeight { get { return GetValue<double?>(); } set { SetValue(value); } }
        public double? DialogHeight { get { return GetValue<double?>(); } set { SetValue(value); } }
        public double? ButtonsMaxWidth { get { return GetValue<double?>(); } set { SetValue(value); } }
        public double? ButtonsMinWidth { get { return GetValue<double?>(); } set { SetValue(value); } }
        public double? ButtonsWidth { get { return GetValue<double?>(); } set { SetValue(value); } }
        public IEnumerable<OverlayDialogButton> Buttons { get { return GetValue(() => Enumerable.Empty<OverlayDialogButton>()); } set { SetValue(value); } }
        //public OverlayButtonDefaultDataTemplate OverlayButtonDataTemplate { get { return GetValue(() => Enumerable.Empty<OverlayDialogButton>()); } set { SetValue(value); } }
        //public FrameworkElement DialogControl { get { return GetValue<FrameworkElement>(); } set { SetValue(value); } }
    }
}
