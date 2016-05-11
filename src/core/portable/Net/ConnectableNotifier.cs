using System.Threading.Tasks;
using System.Linq.Expressions;
using System.ComponentModel;
using Fuxion.Logging;
using Fuxion.ComponentModel;
using System;
using Fuxion.Threading.Tasks;

namespace Fuxion.Net
{
	public abstract class ConnectableNotifier<TConnectableNotifier> : Notifier<TConnectableNotifier>, IConnectableNotifier<TConnectableNotifier>
        where TConnectableNotifier : class, IConnectableNotifier<TConnectableNotifier>
	{
	    protected ConnectableNotifier()
		{
		    PropertyChanged += (s, e) =>
		    {
                e.Case(() => KeepAliveInterval, p =>
                {
                    //Si KeepAliveInterval es menor que 1 lanzo una ArgumentException
                    if (p.ActualValue.TotalMilliseconds < 1)
                        throw new ArgumentException(
                            "El valor de '" + s.GetMemberName(() => s.KeepAliveInterval) + "' no puede ser cero o negativo, para deshabilitar el KeepAlive establezca la propiedad '" + s.GetMemberName(() => s.IsKeepAliveEnable) + "' en False.",
                            s.GetMemberName(() => s.KeepAliveInterval));
                });
                e.Case(() => ConnectionMode, p =>
                {
                    //Si cambia el modo de conexión a automática y no estoy conectado o conectando debo llamar a Connect
                    if (p.ActualValue == ConnectionMode.Automatic && State != ConnectionState.Opened && State != ConnectionState.Opening)
                        Connect();
                });
                e.Case(() => State, p =>
                {
                    //Si cambia el estado hacia o desde Opened abrá que disparar el cambio de la propiedad IsConnected y el evento IsConnectedChanged
                    if (p.PreviousValue == ConnectionState.Opened || p.ActualValue == ConnectionState.Opened)
                    {
                        RaisePropertyChanged(() => IsConnected, p.PreviousValue == ConnectionState.Opened, p.ActualValue == ConnectionState.Opened);
                        if (Synchronizer != null)
                            Synchronizer.Invoke(actualValue => IsConnectedChanged?.Invoke(this, new EventArgs<bool>(actualValue == ConnectionState.Opened)), p.ActualValue);
                        else
                            IsConnectedChanged?.Invoke(this, new EventArgs<bool>(p.ActualValue == ConnectionState.Opened));
                    }
                });
		    };
		}
	    private ILog log = LogManager.Create<ConnectableNotifier<TConnectableNotifier>>();
		protected void ConnectionPropertyChanged()
		{
			switch (State)
			{
				case ConnectionState.Created: //Si esta creado, en error o cerrado no hay que hacer nada
				case ConnectionState.Faulted:
				case ConnectionState.Closed:
					//log.Verbose("SE HA CAMBIADO UNA PROPIEDAD DE CONEXION SIN CONSECUENCIAS");
					break;
				case ConnectionState.Opening: //Si esta conectando, conectado o cerrando, hay que desconectar y volver a conectar
				case ConnectionState.Opened:
				case ConnectionState.Closing:
					//log.Verbose("SE HA CAMBIADO UNA PROPIEDAD DE CONEXION Y SE DEBE VOLVER A CONECTAR");
			        var actualConnectMode = ConnectionMode;
                    Disconnect().ContinueWith((task) =>
                    {
                        if (actualConnectMode == ConnectionMode.Automatic)
                            ConnectionMode = ConnectionMode.Automatic;
                        else
                            Connect();
                    });
					break;
			}
		}
        public TimeSpan AutomaticConnectionModeRetryInterval
        {
            get { return GetValue<TimeSpan>(() => TimeSpan.FromSeconds(5)); }
            set { SetValue(value); }
        }
		public ConnectionMode ConnectionMode
		{
            get { return GetLockedValue(() => ConnectionMode.Manual); }
			set { SetLockedValue(value); }
		}
	    public ConnectionState State
	    {
	        get { return GetLockedValue(() => ConnectionState.Created); }
	        private set { SetLockedValue(value); }
	    }
	    #region Connect
        public string LastConnectionAttemptErrorMessage
        {
            get { return GetValue<string>( ); }
            set { SetValue(value); }
        }
        protected bool IsConnectCancellationRequested
        {
            get { return GetValue<bool>( ); }
            set { SetValue(value); }
        }
		public event EventHandler<EventArgs<bool>> IsConnectedChanged;
		public bool IsConnected { get { return State == ConnectionState.Opened; } }
		Task connectionTask;
		protected abstract Task OnConnect();
		public async Task Connect()
		{
			//log.Verbose(string.Format("({0}) LLAMANDO CONNECT con estado {1}", Thread.CurrentThread.ManagedThreadId, State));
			switch (State)
			{
				case ConnectionState.Opened: //Si esta conectado lanzo una excepción
					throw new InvalidOperationException("No se puede conectar porque la conexión ya esta activa.");
				case ConnectionState.Created: //Si esta creado o en error creo una nueva tarea de conexión
				case ConnectionState.Faulted:
				case ConnectionState.Closed:
					LastConnectionAttemptErrorMessage = null;
					State = ConnectionState.Opening;
					connectionTask = TaskManager.Create(async () =>
					{
						IsConnectCancellationRequested = false;						
						//log.Verbose(string.Format("({0}) NUEVA TAREA DE CONEXION ... ", Thread.CurrentThread.ManagedThreadId));
						while (!IsConnectCancellationRequested)
						{
							//log.Verbose(string.Format("({0}) INTENTANDO CONECTAR ... ", Thread.CurrentThread.ManagedThreadId));
							try
							{
								await OnConnect();
								LastConnectionAttemptErrorMessage = null;
								StartKeepAlive();
								State = ConnectionState.Opened;
								break;
							} catch (Exception ex)
							{
                                log.Error("Error '" + ex.GetType().Name + "' en el método 'OnConnect': " + ex.Message, ex);
								LastConnectionAttemptErrorMessage = ex.Message;
								//log.Verbose(string.Format("({0}) CONNECTION ERROR '{1}': {2}", Thread.CurrentThread.ManagedThreadId, ex.GetType().Name, ex.Message));
								if (ConnectionMode == ConnectionMode.Manual)
								{
									State = ConnectionState.Faulted;
									break;
								}
								connectionTask.Sleep(AutomaticConnectionModeRetryInterval);
							}
						}
						//log.Verbose(string.Format("({0}) FIN DE TAREA DE CONEXION ... ", Thread.CurrentThread.ManagedThreadId));
					});

                    connectionTask.OnCancel(() => { IsConnectCancellationRequested = true; });
			        connectionTask.Start();
					await connectionTask;
					break;
				case ConnectionState.Opening:
					await connectionTask;
					break;
				case ConnectionState.Closing:
					await disconnectionTask.ContinueWith((task) => Connect());
					break;
				default:
					throw new NotImplementedException(string.Format("El estado '{0}' no ha sido implementado en la operación de conexión.", State));
			}
		}
		#endregion
		#region Disconnect
	    protected bool IsDisconnectCancellationRequested { get { return GetValue<bool>(); } set { SetValue(value); } }
	    Task disconnectionTask;
		protected abstract Task OnDisconnect();
		public async Task Disconnect() { await Disconnect(false); }
		protected async Task Disconnect(bool isFaultedConnection)
		{
			switch (State)
			{
				case ConnectionState.Created:
				case ConnectionState.Faulted:
				case ConnectionState.Closed:
					throw new InvalidOperationException("No se puede desconectar porque la conexión no esta activa.");
				case ConnectionState.Closing:
					await disconnectionTask;
					break;
				case ConnectionState.Opening:
				case ConnectionState.Opened:
					//disconnectionToken = new CancellationTokenSource();
					disconnectionTask = TaskManager.Create(async () =>
					{
						IsDisconnectCancellationRequested = false;						
						//log.Verbose(string.Format("({0}) NUEVA TARE DE DESCONEXION ... ", Thread.CurrentThread.ManagedThreadId));
						State = ConnectionState.Closing;
                        //log.Verbose(string.Format("({0}) ESPERANDO CANCELACION DE CONEXION ... ", Thread.CurrentThread.ManagedThreadId));
                        new[] { connectionTask, keepAliveTask }.CancelAndWait(throwExceptionIfNotRunning: false);
					    try
					    {
					        await OnDisconnect();
					    }
					    catch(Exception ex) {
					        log.Error("Error '" + ex.GetType().Name + "' en el método 'OnDisconnect': " + ex.Message, ex);
					    }
					    finally
					    {
					        if (isFaultedConnection)
					        {
					            State = ConnectionState.Faulted;
					            if (ConnectionMode == ConnectionMode.Automatic)
					                Connect();
					        }
					        else
					        {
					            ConnectionMode = ConnectionMode.Manual;
					            State = ConnectionState.Closed;
					        }
					    }
					    //log.Verbose(string.Format("({0}) FIN DE TAREA DE DESCONEXION ... ", Thread.CurrentThread.ManagedThreadId));
					});
                    disconnectionTask.OnCancel(() => { IsDisconnectCancellationRequested = true; });
			        disconnectionTask.Start();
					await disconnectionTask;
					break;
				default:
					throw new NotImplementedException(string.Format("El estado '{0}' no ha sido implementado en la operación de desconexión.", State));
			}
		}
		#endregion
		#region KeepAlive
        protected bool IsKeepAliveCancellationRequested
        {
            get { return GetValue<bool>( ); }
            set { SetValue(value); }
        }
        public bool IsKeepAliveEnable
        {
            get { return GetValue<bool>( ); }
            set { SetValue(value); }
        }
	    public TimeSpan KeepAliveInterval
	    {
	        get { return GetValue<TimeSpan>(() => TimeSpan.FromSeconds(60)); }
	        set { SetValue(value); }
	    }
		Task keepAliveTask;
		protected virtual Task OnKeepAlive() { return Task.FromResult(0); }
        private void StartKeepAlive()
		{
			//Comprobar si el keep alive esta habilitado
			if (!IsKeepAliveEnable) return;
			//Comprobar si la tarea esta ya en ejecución
            if (keepAliveTask != null && keepAliveTask.Status == TaskStatus.Running) return; //await keepAliveTask;
            
			keepAliveTask = TaskManager.Create(async () =>
			{
				IsKeepAliveCancellationRequested = false;
				
				while (!IsKeepAliveCancellationRequested)
				{
					keepAliveTask.Sleep(KeepAliveInterval);
					if (IsKeepAliveCancellationRequested) break;
				    try { await OnKeepAlive(); }
				    catch (Exception ex) {
				        log.Error("Error '" + ex.GetType().Name + "' en el método 'OnKeepAlive': " + ex.Message, ex);
				        Disconnect(true);
				    }
				}
			});

            keepAliveTask.OnCancel(() => { IsKeepAliveCancellationRequested = true; });
            keepAliveTask.Start();
            //await keepAliveTask;
		}
		#endregion
	}
}
