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
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
						Connect();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
					break;
				case ConnectionState.Opening: //Si esta conectando, conectado o cerrando, hay que desconectar y volver a conectar
				case ConnectionState.Opened:
				case ConnectionState.Closing:
					var actualConnectMode = ConnectionMode;
					Disconnect().ContinueWith((task) =>
					{
						if (actualConnectMode == ConnectionMode.Automatic)
							ConnectionMode = ConnectionMode.Automatic;
						else
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
							Connect();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
					});
					break;
			}
		}
		public TimeSpan AutomaticConnectionModeRetryInterval
		{
			get => GetValue<TimeSpan>(() => TimeSpan.FromSeconds(5));
			set => SetValue(value);
		}
		public ConnectionMode ConnectionMode
		{
			get => GetLockedValue(() => ConnectionMode.Manual);
			set => SetLockedValue(value);
		}
		public ConnectionState State
		{
			get => GetLockedValue(() => ConnectionState.Created);
			private set => SetLockedValue(value);
		}
		#region Connect
		public string LastConnectionAttemptErrorMessage
		{
			get => GetValue<string>();
			set => SetValue(value);
		}
		protected bool IsConnectCancellationRequested
		{
			get => GetValue<bool>();
			set => SetValue(value);
		}
		public event EventHandler<EventArgs<bool>> IsConnectedChanged;
		public bool IsConnected => State == ConnectionState.Opened;
		Task connectionTask;
		protected abstract Task OnConnect();
		public async Task Connect()
		{
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
						while (!IsConnectCancellationRequested)
						{
							try
							{
								await OnConnect();
								LastConnectionAttemptErrorMessage = null;
								StartKeepAlive();
								State = ConnectionState.Opened;
								break;
							}
							catch (Exception ex)
							{
								log.Error("Error '" + ex.GetType().Name + "' en el método 'OnConnect': " + ex.Message, ex);
								LastConnectionAttemptErrorMessage = ex.Message;
								if (ConnectionMode == ConnectionMode.Manual)
								{
									State = ConnectionState.Faulted;
									break;
								}
								connectionTask.Sleep(AutomaticConnectionModeRetryInterval);
							}
						}
					});
					connectionTask.OnCancelRequested(() => IsConnectCancellationRequested = true);
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
					throw new NotImplementedException($"El estado '{State}' no ha sido implementado en la operación de conexión.");
			}
		}
		#endregion
		#region Disconnect
		protected bool IsDisconnectCancellationRequested
		{
			get => GetValue<bool>();
			private set => SetValue(value);
		}
		Task<bool> disconnectionTask;
		protected abstract Task OnDisconnect();
		public async Task Disconnect()
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
					disconnectionTask = TaskManager.Create<bool>(async () =>
					{
						IsDisconnectCancellationRequested = false;
						State = ConnectionState.Closing;
						new[] { connectionTask, keepAliveTask }.CancelAndWait(throwExceptionIfNotRunning: false);
						try
						{
							await OnDisconnect();
						}
						catch (Exception ex)
						{
							log.Error("Error '" + ex.GetType().Name + "' en el método 'OnDisconnect': " + ex.Message, ex);
						}
						ConnectionMode = ConnectionMode.Manual;
						State = ConnectionState.Closed;
						return false;
					});
					disconnectionTask.OnCancelRequested(() => IsDisconnectCancellationRequested = true);
					disconnectionTask.Start();
					await disconnectionTask;
					break;
				default:
					throw new NotImplementedException($"El estado '{State}' no ha sido implementado en la operación de desconexión.");
			}
		}
		protected async Task<bool> ReconnectOnFailure()
		{
			State = ConnectionState.Faulted;
			if (ConnectionMode == ConnectionMode.Automatic)
			{
				await Disconnect();
				await Connect();
				return true;
			}
			return false;
		}
		#endregion
		#region KeepAlive
		protected bool IsKeepAliveCancellationRequested
		{
			get => GetValue<bool>();
			set => SetValue(value);
		}
		public bool IsKeepAliveEnable
		{
			get => GetValue<bool>();
			set => SetValue(value);
		}
		public TimeSpan KeepAliveInterval
		{
			get => GetValue(() => TimeSpan.FromSeconds(60));
			set => SetValue(value);
		}
		Task keepAliveTask;
		protected virtual Task OnKeepAlive() => Task.CompletedTask;
		private void StartKeepAlive()
		{
			//Comprobar si el keep alive esta habilitado
			if (!IsKeepAliveEnable) return;
			//Comprobar si la tarea esta ya en ejecución
			if (keepAliveTask != null && keepAliveTask.Status == TaskStatus.Running) return;

			keepAliveTask = TaskManager.Create(async () =>
			{
				IsKeepAliveCancellationRequested = false;
				while (!IsKeepAliveCancellationRequested)
				{
					keepAliveTask.Sleep(KeepAliveInterval);
					if (IsKeepAliveCancellationRequested) break;
					try { await OnKeepAlive(); }
					catch (Exception ex)
					{
						log.Error("Error '" + ex.GetType().Name + "' en el método 'OnKeepAlive': " + ex.Message, ex);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
						ReconnectOnFailure();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
						break;
					}
				}
			});
			keepAliveTask.OnCancelRequested(() => IsKeepAliveCancellationRequested = true);
			keepAliveTask.Start();
		}
		#endregion
	}
}
