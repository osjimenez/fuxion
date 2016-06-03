using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace Fuxion.Windows.Controls
{
    public partial class Overlay : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public UIElement ActiveOverlayControl { get { return activeControl.Content as UIElement; } }
        public Overlay()
        {
            InitializeComponent();
            _Children.CollectionChanged += _Children_CollectionChanged;
        }
        private void GoToOverlayed()
        {
            VisualStateManager.GoToState(this, "Overlayed", true);
        }
        private void GoToNotOverlayed()
        {
            VisualStateManager.GoToState(this, "NotOverlayed", true);
        }
        ObservableCollection<UIElement> _Children = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> Children
        {
            get { return _Children; }
            set
            {
                _Children = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Children)));
            }
        }
        void _Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null && e.OldItems.Count > 0)
                foreach (var item in e.OldItems.Cast<UIElement>())
                {
                    if (activeControl.Content == item)
                    {
                        activeControl.Content = null;
                    }
                    if (inactiveControlsGrid.Children.Count > 0)
                    {
                        var ctrl = inactiveControlsGrid.Children[0];
                        inactiveControlsGrid.Children.RemoveAt(0);
                        activeControl.Content = ctrl;
                    }
                }
            if (e.NewItems != null && e.NewItems.Count > 0)
                foreach (var item in e.NewItems.Cast<UIElement>())
                {
                    if (activeControl.Content != null)
                    {
                        var ctrl = activeControl.Content as UIElement;
                        activeControl.Content = null;
                        inactiveControlsGrid.Children.Add(ctrl);
                    }
                    activeControl.Content = item;
                }
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                inactiveControlsGrid.Children.Clear();
                activeControl.Content = null;
            }
            if (activeControl.Content != null)
            {
                GoToOverlayed();
            }
            else
            {
                GoToNotOverlayed();
            }
        }
    }
}
