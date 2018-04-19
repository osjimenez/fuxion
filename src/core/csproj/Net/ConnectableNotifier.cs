using System.Threading.Tasks;
using System.Linq.Expressions;
using System.ComponentModel;
using Fuxion.Logging;
using Fuxion.ComponentModel;
using System;
using Fuxion.Threading.Tasks;
using Fuxion.Windows.Threading;
using System.Threading;

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
						this.Invoke(actualValue => IsConnectedChanged?.Invoke(this, new EventArgs<bool>(actualValue == ConnectionState.Opened)), p.ActualValue);
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
		protected bool IsConnectCancellationRequested => connectionTask.IsCancellationRequested();
		public event EventHandler<EventArgs<bool>> IsConnectedChanged;
		public bool IsConnected => State == ConnectionState.Opened;
		Task connectionTask;
		protected abstract Task OnConnect();
		Task Connect(out Func<Task<bool>> firstTryResultFunc, TimeSpan? firstTryTimeout = null)
		{
			switch (State)
			{
				case ConnectionState.Opened: //Si esta conectado lanzo una excepción
					throw new InvalidOperationException("No se puede conectar porque la conexión ya esta activa.");
				case ConnectionState.Created: //Si esta creado o en error creo una nueva tarea de conexión
				case ConnectionState.Faulted:
				case ConnectionState.Closed:
					State = ConnectionState.Opening;
					var cts = new CancellationTokenSource();
					bool? firstTryResult = null;
					firstTryResultFunc = new Func<Task<bool>>(async ()=> {
						try
						{
							await Task.Delay(firstTryTimeout ?? TimeSpan.FromMinutes(1), cts.Token);
							return false;
						}
						catch (TaskCanceledException)
						{
							return firstTryResult ?? false;
						}
					});
					connectionTask = TaskManager.Create(async () =>
					{
						SetValue(IsConnectCancellationRequested, true, nameof(IsConnectCancellationRequested));
						bool firstTry = true;
						while (!IsConnectCancellationRequested)
						{
							try
							{
								await OnConnect();
								LastConnectionAttemptErrorMessage = null;
								StartKeepAlive();
								State = ConnectionState.Opened;
								if (firstTry)
								{
									firstTryResult = true;
									cts.Cancel();
								}
								break;
							}
							catch (Exception ex)
							{
								if (firstTry)
								{
									firstTryResult = false;
									cts.Cancel();
								}
								log.Error($"Error '{ex.GetType().Name}' en el método '{nameof(OnConnect)}' de la clase '{GetType().GetSignature(false)}' (*)\r\n{ex.Message}", ex);
								LastConnectionAttemptErrorMessage = ex.Message;
								if (ConnectionMode == ConnectionMode.Manual)
								{
									State = ConnectionState.Faulted;
									break;
								}
								connectionTask.Sleep(AutomaticConnectionModeRetryInterval);
							}
							finally
							{
								firstTry = false;
							}
						}
					});
					connectionTask.OnCancelRequested(() => SetValue(IsConnectCancellationRequested, true, nameof(IsConnectCancellationRequested)));
					connectionTask.Start();
					return connectionTask;
					break;
				case ConnectionState.Opening:
					firstTryResultFunc = new Func<Task<bool>>(() => Task.FromResult(false));
					return connectionTask;
					break;
				case ConnectionState.Closing:
					firstTryResultFunc = new Func<Task<bool>>(() => Task.FromResult(false));
					return disconnectionTask.ContinueWith((task) => Connect());
					break;
				default:
					throw new NotImplementedException($"El estado '{State}' no ha sido implementado en la operación de conexión.");
			}
		}
		public Task Connect()
		{
			return Connect(out var _);
			//switch (State)
			//{
			//	case ConnectionState.Opened: //Si esta conectado lanzo una excepción
			//		throw new InvalidOperationException("No se puede conectar porque la conexión ya esta activa.");
			//	case ConnectionState.Created: //Si esta creado o en error creo una nueva tarea de conexión
			//	case ConnectionState.Faulted:
			//	case ConnectionState.Closed:
			//		State = ConnectionState.Opening;
			//		connectionTask = TaskManager.Create(async () =>
			//		{
			//			SetValue(IsConnectCancellationRequested, true, nameof(IsConnectCancellationRequested));
			//			while (!IsConnectCancellationRequested)
			//			{
			//				try
			//				{
			//					await OnConnect();
			//					LastConnectionAttemptErrorMessage = null;
			//					StartKeepAlive();
			//					State = ConnectionState.Opened;
			//					break;
			//				}
			//				catch (Exception ex)
			//				{
			//					log.Error($"Error '{ex.GetType().Name}' en el método '{nameof(OnConnect)}' de la clase '{GetType().GetSignature(false)}' (*)\r\n{ex.Message}", ex);
			//					LastConnectionAttemptErrorMessage = ex.Message;
			//					if (ConnectionMode == ConnectionMode.Manual)
			//					{
			//						State = ConnectionState.Faulted;
			//						break;
			//					}
			//					connectionTask.Sleep(AutomaticConnectionModeRetryInterval);
			//				}
			//			}
			//		});
			//		connectionTask.OnCancelRequested(() => SetValue(IsConnectCancellationRequested, true, nameof(IsConnectCancellationRequested)));
			//		connectionTask.Start();
			//		await connectionTask;
			//		break;
			//	case ConnectionState.Opening:
			//		await connectionTask;
			//		break;
			//	case ConnectionState.Closing:
			//		await disconnectionTask.ContinueWith((task) => Connect());
			//		break;
			//	default:
			//		throw new NotImplementedException($"El estado '{State}' no ha sido implementado en la operación de conexión.");
			//}
		}
		#endregion
		#region Disconnect
		protected bool IsDisconnectCancellationRequested => disconnectionTask.IsCancellationRequested();
		Task disconnectionTask;
		protected abstract Task OnDisconnect();
		async Task Disconnect(bool mustClose)
		{
			switch (State)
			{
				case ConnectionState.Created:
				case ConnectionState.Closed:
					throw new InvalidOperationException("No se puede desconectar porque la conexión no esta activa.");
				case ConnectionState.Closing:
					await disconnectionTask;
					break;
				case ConnectionState.Faulted:
				case ConnectionState.Opening:
				case ConnectionState.Opened:
					disconnectionTask = TaskManager.Create(async () =>
					{
						SetValue(IsDisconnectCancellationRequested, true, nameof(IsDisconnectCancellationRequested));
						if (mustClose)
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
						if (mustClose)
						{
							ConnectionMode = ConnectionMode.Manual;
							State = ConnectionState.Closed;
						}
					});
					disconnectionTask.OnCancelRequested(() => SetValue(IsDisconnectCancellationRequested, true, nameof(IsDisconnectCancellationRequested)));
					disconnectionTask.Start();
					await disconnectionTask;
					break;
				default:
					throw new NotImplementedException($"El estado '{State}' no ha sido implementado en la operación de desconexión.");
			}
		}
		public Task Disconnect() => Disconnect(true);
		protected async Task<bool> ReconnectOnFailure(TimeSpan? timeout = null)
		{
			State = ConnectionState.Faulted;
			if (ConnectionMode == ConnectionMode.Automatic)
			{
				await Disconnect(false);
				//var f = new Func<Task<bool>>(() => Task.FromResult(true));
				Connect(out var f, timeout);
				return await f();
				//return true;
			}
			return false;
		}
		#endregion
		#region KeepAlive
		protected bool IsKeepAliveCancellationRequested => keepAliveTask.IsCancellationRequested();
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
				SetValue(IsKeepAliveCancellationRequested, true, nameof(IsKeepAliveCancellationRequested));
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
			keepAliveTask.OnCancelRequested(() => SetValue(IsKeepAliveCancellationRequested, true, nameof(IsKeepAliveCancellationRequested)));
			keepAliveTask.Start();
		}
		#endregion
	}
}
