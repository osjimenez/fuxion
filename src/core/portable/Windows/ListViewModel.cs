//using Fuxion;
//using Fuxion.Commands;
//using Fuxion.Entities;
//using Fuxion.Events;
//using Fuxion.Factories;
//using Fuxion.Logging;
//using Fuxion.Notifications;
//using Fuxion.Repositories;
//using Fuxion.Windows;
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Collections.Specialized;
//using System.ComponentModel;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using System.Threading.Tasks;

//using System.Diagnostics;
//using System.Windows.Threading;
//using AutoMapper;
//using AutoMapper.Mappers;
//using AutoMapper.QueryableExtensions;
//using Orleans;

//namespace Fuxion.Windows
//{
// TODO - Oscar - Think about Automapper and Dispatcher before implement this class on PCL library
//    public abstract class ListViewModel<TEntity, TViewModel, TCreateCommand, TCreatedNotification, TDeleteCommand, TDeletedNotification> : INotifyPropertyChanged
//        // TODO - Oscar - Remove INotifyPropertyChanged constraint from TEntity
//        where TEntity : class, IEntity, new()
//        where TViewModel : class, IViewModel, new()
//        where TCreateCommand : ICommand
//        where TCreatedNotification : INotification
//        where TDeleteCommand : ICommand
//        where TDeletedNotification : INotification
//    {
//        public ListViewModel()
//        {
//            Debug.WriteLine("============================ CREANDO LIST-VIEWMODEL: " + GetType().Name);
//            SubscribeToRefreshAsync<TCreatedNotification>(n => List.Add(new TViewModel { Id = n.SourceId }));
//            SubscribeToRefreshAsync<TDeletedNotification>(n => List.Remove(List.Single(i => i.Id == n.SourceId)));
//            AddCommand = new DelegateCommand(
//                async () => await ApplicationManager.DoAsync(GetCreateCommand()));
//            DeleteCommand = new DelegateCommand(
//                async () => await ApplicationManager.DoAsync(GetDeleteCommand())
//                , () => Selected != null);
//            RefreshCommand = new DelegateCommand(() => Load());

//            List.CollectionChanged += (s, e) =>
//            {
//                if (e.Action == NotifyCollectionChangedAction.Add)
//                    foreach (var item in e.NewItems)
//                        ((INotifyPropertyChanged)item).PropertyChanged += ViewModel_PropertyChanged;
//                else if (e.Action == NotifyCollectionChangedAction.Remove)
//                    foreach (var item in e.OldItems)
//                        ((INotifyPropertyChanged)item).PropertyChanged -= ViewModel_PropertyChanged;
//                else if (e.Action == NotifyCollectionChangedAction.Reset)
//                    foreach (var item in List)
//                        ((INotifyPropertyChanged)item).PropertyChanged -= ViewModel_PropertyChanged;
//                else throw new Exception();
//            };

//            Load();
//        }
        
//        protected bool Refreshing { get; set; }
//        public event PropertyChangedEventHandler PropertyChanged;
//        public ObservableCollection<TViewModel> List { get; set; } = new ObservableCollection<TViewModel>();
//        public DelegateCommand AddCommand { get; set; }
//        public DelegateCommand DeleteCommand { get; set; }
//        public DelegateCommand RefreshCommand { get; set; }
//        private TViewModel _selected;
//        public TViewModel Selected
//        {
//            get { return _selected; }
//            set
//            {
//                if (_selected != value)
//                {
//                    _selected = value;
//                    DeleteCommand.RaiseCanExecuteChanged();
//                    if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(nameof(Selected)));
//                }
//            }
//        }
//        // TODO - Oscar - Restore PostSharp
//        //[Log]
//        private void Load()
//        {
//            // Remove previous delegates
//            foreach (var item in List)
//                ((INotifyPropertyChanged)item).PropertyChanged -= ViewModel_PropertyChanged;
//            // Clear collection
//            List.Clear();
//            // Relink delegates

//            var oo = OnLoad().Project().To<TViewModel>(membersToExpand: MembersToExpand);
//            foreach (var item in oo)
//                List.Add(item);
//        }
//        protected virtual Expression<Func<TViewModel,object>>[] MembersToExpand { get { return new Expression<Func<TViewModel, object>>[] { }; } }
//        //protected abstract TViewModel Map(TEntity entity);
//        protected virtual IQueryable<TEntity> OnLoad() { return Factory.Create<IQueryableRepository<TEntity>>().Query(); }
//        protected Task<IDisposable> SubscribeToRefreshAsync<TNotification>(Action<TNotification> action)
//            where TNotification : INotification
//        {
//            return ApplicationManager.SubscribeNotificationAsync(new Action<TNotification>(n =>
//            {
//                Refreshing = true;
//                if (System.Windows.Application.Current.Dispatcher.CheckAccess())
//                    action(n);
//                else
//                    System.Windows.Application.Current.Dispatcher.Invoke(action, n);
//                Refreshing = false;
//            }));
//        }
//        protected abstract TCreateCommand GetCreateCommand();
//        protected abstract TDeleteCommand GetDeleteCommand();
//        protected virtual Task OnViewModelPropertyChanged(TViewModel viewModel, string propertyName) { return Task.CompletedTask; }
//        private async void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
//        {
//            if (Refreshing) return; // Because this won't be changes
//            await OnViewModelPropertyChanged((TViewModel)sender, e.PropertyName);
//        }
//    }
//}
