using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml.Serialization;
using Fuxion.ComponentModel;
using Fuxion.Logging;
using Fuxion.Threading.Tasks;
namespace Waf.Windows.Threading
{
	public interface IDispatcherNotifier<TDispatcherNotifier> : INotifier<TDispatcherNotifier>, IDisposable where TDispatcherNotifier : IDispatcherNotifier<TDispatcherNotifier>
	{
		Dispatcher Dispatcher { get; set; }
		bool UseDispatcherOnInvoke { get; set; }
	}
	//[DataContract(IsReference = true)]
	public class DispatcherNotifier<TDispatcherNotifier> : Notifier<TDispatcherNotifier>, IDispatcherNotifier<TDispatcherNotifier> where TDispatcherNotifier : class, IDispatcherNotifier<TDispatcherNotifier>
	{
		public DispatcherNotifier()
		{
			if (SynchronizationContext.Current != null)
				_dispatcher = Dispatcher.CurrentDispatcher;
		}
		public DispatcherNotifier(Dispatcher dispatcher) : this() { Dispatcher = dispatcher; }
	    private Dispatcher _dispatcher;
        [Browsable(false)]
        [XmlIgnore]
	    public Dispatcher Dispatcher { get { return GetField(ref _dispatcher); } set { SetField(ref _dispatcher, value); } }
	    //public Dispatcher Dispatcher
        //{
        //    get;// { return GetValue<Dispatcher>(); }
        //    set;// { SetValue(value); }
        //}
	    private bool _useDispatcherOnInvoke = true;
	    [Browsable(false)]
        [XmlIgnore]
        public bool UseDispatcherOnInvoke { get { return GetField(ref _useDispatcherOnInvoke); } set { SetField(ref _useDispatcherOnInvoke, value); } }
        //public bool UseDispatcherOnInvoke
        //{
        //    get { return GetValue<bool>(() => true); }
        //    set { SetValue(value); }
        //}
		protected override sealed void OnRaisePropertyChanged<T>(string propertyName, T oldValue, T newValue)
		{
            Invoke(base.OnRaisePropertyChanged, propertyName, oldValue, newValue);
		}
        //protected override bool OnRaisePropertyChanging<T>(string propertyName, T actualValue, T futureValue)
        //{
        //    return Invoke<string, T, T, bool>(base.OnRaisePropertyChanging, propertyName, actualValue, futureValue).WaitResult();
        //}
        //protected override T OnRaisePropertyRead<T>(string propertyName, T value)
        //{
        //    return Invoke<string, T, T>(base.OnRaisePropertyRead<T>, propertyName, value).WaitResult();
        //}
        //private Dispatcher GetCurrentDispatcher()
        //{
        //    //No podemos consultar las propiedades 'UseDispatcherOnInvoke' y 'Dispatcher' directamente porque se produciría un bucle
        //    //Pero tampoco podemos acceder a sus campos privados porque no se infromaría de la lectura a los posibles registrados a PropertyRead.
        //    bool useDispatcherOnInvoke = false;
        //    if (_dispatcher != null && _useDispatcherOnInvoke && _dispatcher.Thread.IsAlive && _dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
        //        useDispatcherOnInvoke = (bool)_dispatcher.Invoke(new Func<string, bool, bool>(base.OnRaisePropertyRead), "UseDispatcherOnInvoke", _useDispatcherOnInvoke);
        //    else
        //        useDispatcherOnInvoke = base.OnRaisePropertyRead("UseDispatcherOnInvoke", _useDispatcherOnInvoke);
        //    _useDispatcherOnInvoke = useDispatcherOnInvoke;
        //    Dispatcher dispatcher = null;
        //    if (useDispatcherOnInvoke && _dispatcher != null && _dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
        //        dispatcher = (Dispatcher)_dispatcher.Invoke(new Func<string, Dispatcher, Dispatcher>(base.OnRaisePropertyRead), "Dispatcher", _dispatcher);
        //    else
        //        dispatcher = base.OnRaisePropertyRead("Dispatcher", _dispatcher);

        //    if (dispatcher != null && useDispatcherOnInvoke && dispatcher.Thread.IsAlive && dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
        //        return dispatcher;
        //    return null;
        //}
        private Dispatcher GetCurrentDispatcher()
        {
            if (_dispatcher != null && _useDispatcherOnInvoke && _dispatcher.Thread.IsAlive && _dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
                return _dispatcher;
            return null;
        }
        private async Task<TResult> InvokeDelegate<TResult>(Delegate method, params object[] args)
	    {
	        var dis = GetCurrentDispatcher();
            if(dis != null)
                return await TaskManager.StartNew((d, a, m) =>
                {
                    try
                    {
                        return (TResult) d.Invoke(m, a);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }, dis, args, method);
            return (TResult)method.DynamicInvoke(args);
	    }
	    private async Task InvokeDelegate(Delegate method, params object[] args)
        {
	        var dis = GetCurrentDispatcher();
	        if(dis != null)
	            await TaskManager.StartNew((d, a) => d.Invoke(method, a), dis, args);
	        else
	            method.DynamicInvoke(args);
        }
	    protected async Task<TResult> Invoke<TResult>(Delegate method, params object[] args) { return await InvokeDelegate<TResult>(method, args); }
	    #region Invoke Actions
	    protected async Task Invoke(Action action) { await InvokeDelegate(action); }
	    protected async Task Invoke<T>(Action<T> action, T param) { await InvokeDelegate(action, param); }
	    protected async Task Invoke<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2) { await InvokeDelegate(action, param1, param2); }
	    protected async Task Invoke<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3) { await InvokeDelegate(action, param1, param2, param3); }
	    protected async Task Invoke<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4) { await InvokeDelegate(action, param1, param2, param3, param4); }
	    protected async Task Invoke<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5) { await InvokeDelegate(action, param1, param2, param3, param4, param5); }
	    protected async Task Invoke<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6) { await InvokeDelegate(action, param1, param2, param3, param4, param5, param6); }
	    #endregion
        #region Invoke Funcs
	    protected async Task<TResult> Invoke<TResult>(Func<TResult> func) { return await InvokeDelegate<TResult>(func); }
	    protected async Task<TResult> Invoke<T, TResult>(Func<T, TResult> func, T param) { return await InvokeDelegate<TResult>(func, param); }
	    protected async Task<TResult> Invoke<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 param1, T2 param2) { return await InvokeDelegate<TResult>(func, param1, param2); }
	    protected async Task<TResult> Invoke<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, T1 param1, T2 param2, T3 param3) { return await InvokeDelegate<TResult>(func, param1, param2, param3); }
	    protected async Task<TResult> Invoke<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4) { return await InvokeDelegate<TResult>(func, param1, param2, param3, param4); }
	    protected async Task<TResult> Invoke<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5) { return await InvokeDelegate<TResult>(func, param1, param2, param3, param4, param5); }
	    protected async Task<TResult> Invoke<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6) { return await InvokeDelegate<TResult>(func, param1, param2, param3, param4, param5, param6); }
	    #endregion
		#region IDisposable
		public event EventHandler Disposing;
		public event EventHandler Disposed;
		public void Dispose()
		{
			if (Disposing != null) Disposing(this, new EventArgs());
			Dispose(true);
			GC.SuppressFinalize(this);
			if (Disposed != null) Disposed(this, new EventArgs());
		}
		protected virtual void Dispose(bool disposing) { }
		#endregion
	}
}
