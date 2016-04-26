using Fuxion.ComponentModel;
using Fuxion.Threading.Tasks;
using Fuxion.Windows.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace DemoWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public async void Synchronizer_Click(object sender, RoutedEventArgs args)
        {
            Debug.WriteLine($"Test started in Thread '{Thread.CurrentThread.ManagedThreadId}'");
            var not = new SynchronizableNotifierMock();
            not.Integer = 1;
            not.PropertyChanged += (s, e) =>
            {
                e.Case(() => s.Integer, a =>
                {
                    Debug.WriteLine($"Integer changed to '{a.ActualValue}' in Thread '{Thread.CurrentThread.ManagedThreadId}'");
                });
            };
            not.Integer = 2;
            var task = TaskManager.StartNew(() => {
                Debug.WriteLine($"Task started in Thread '{Thread.CurrentThread.ManagedThreadId}'");
                not.Integer = 3;
            });
            await task;
            not.SetSynchronizer(new DispatcherSynchronizer());
            task = TaskManager.StartNew(() => {
                Debug.WriteLine($"Task started in Thread '{Thread.CurrentThread.ManagedThreadId}'");
                not.Integer = 4;
            });
            not.Integer = 5;
            await task;
            not.Integer = 6;
            //DispatcherSynchronizer sync = new DispatcherSynchronizer();
        }
    }
    public class SynchronizableNotifierMock : Notifier<SynchronizableNotifierMock>
    {
        internal void SetSynchronizer(INotifierSynchronizer value) { Synchronizer = value; }
        public int Integer { get { return GetValue(() => 0); } set { SetValue(value); } }
    }
}
