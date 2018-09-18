using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using log4net.Appender;
using log4net.Core;
using log4net.Util;

namespace Fuxion.Logging
{
	internal interface IInternalErrorHandler
	{
		void Error(string message, Exception ex);
	}
	internal class TcpClient
	{
		private readonly Socket _socket;

		private readonly IInternalErrorHandler _errorHandler;

		public TcpClient(Socket socket, IInternalErrorHandler errorHandler)
		{
			this._socket = socket;
			this._errorHandler = errorHandler;
		}

		public void Close()
		{
			this._socket.Shutdown(SocketShutdown.Both);
			this._socket.Disconnect(false);
			this._socket.Close();
		}

		internal bool SendBuffer(byte[] buffer)
		{
			bool length;
			if (buffer != null)
			{
				try
				{
					int num = this._socket.Send(buffer);
					length = num == (int)buffer.Length;
				} catch (Exception exception1)
				{
					Exception exception = exception1;
					this._errorHandler.Error(string.Format("Unable to send logging event to remote host {0}.", this._socket.RemoteEndPoint), exception);
					return false;
				}
				return length;
			}
			return false;
		}
	}
	public class TcpAppender : AppenderSkeleton
	{
		private TcpListener _listener;

		private int _localPort;

		private readonly Queue<LoggingEvent> _loggingEvents = new Queue<LoggingEvent>();

		private Encoding _mEncoding = Encoding.Default;

		private uint _maxQueueSize = 2000;

		private readonly Queue<TcpAppender.RenderedLoggingEvent> _recentEvents = new Queue<TcpAppender.RenderedLoggingEvent>();

		private readonly List<TcpClient> _clients = new List<TcpClient>();

		private bool _stopWorkerThread;

		private Thread _workerThread;

		private readonly AutoResetEvent _queueFullEvent = new AutoResetEvent(false);

		private bool _messagesLost;

		private DateTime _lastCleanupTime = DateTime.Now;

		private byte[] _heartbeat;

		private DateTime _lastSendTime = DateTime.Now;

		public Encoding Encoding
		{
			get
			{
				return this._mEncoding;
			}
			set
			{
				this._mEncoding = value;
			}
		}

		private bool IsActive
		{
			get
			{
				return this._localPort != -1;
			}
		}

		public int LocalPort
		{
			get
			{
				return this._localPort;
			}
			set
			{
				if (value == -1)
				{
					this._localPort = value;
					return;
				}
				if (value != 0 && (value < 0 || value > 65535))
				{
					object obj = value;
					string[] str = new string[] { "The value specified is less than ", null, null, null, null };
					str[1] = 0.ToString(NumberFormatInfo.InvariantInfo);
					str[2] = " or greater than ";
					str[3] = 65535.ToString(NumberFormatInfo.InvariantInfo);
					str[4] = ".";
					throw SystemInfo.CreateArgumentOutOfRangeException("value", obj, string.Concat(str));
				}
				this._localPort = value;
			}
		}

		public uint MaxQueueSize
		{
			get
			{
				return this._maxQueueSize;
			}
			set
			{
				this._maxQueueSize = value;
			}
		}

		protected override bool RequiresLayout
		{
			get
			{
				return true;
			}
		}

		public TcpAppender()
		{
		}

		private void AcceptPendingClients()
		{
			while (this._listener.Pending())
			{
				Socket socket = this._listener.AcceptSocket();
				this._clients.Add(new TcpClient(socket, new TcpAppender.Log4netErrorHandler(this.ErrorHandler)));
				this.SendRecentEvents(socket);
			}
		}

		public override void ActivateOptions()
		{
			base.ActivateOptions();
			if (this.LocalPort == -1)
			{
				return;
			}
			if (this.LocalPort == 0)
			{
				throw SystemInfo.CreateArgumentOutOfRangeException("this.LocalPort", this.LocalPort, "The LocalPort is zero.");
			}
			if (this.LocalPort != 0 && (this.LocalPort < 0 || this.LocalPort > 65535))
			{
				object localPort = this.LocalPort;
				string[] str = new string[] { "The LocalPort is less than ", null, null, null, null };
				str[1] = 0.ToString(NumberFormatInfo.InvariantInfo);
				str[2] = " or greater than ";
				str[3] = 65535.ToString(NumberFormatInfo.InvariantInfo);
				str[4] = ".";
				throw SystemInfo.CreateArgumentOutOfRangeException("this.LocalPort", localPort, string.Concat(str));
			}
			this._heartbeat = this.Encoding.GetBytes("\n\r");
			this.InitializeClientConnection();
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			if (!this.IsActive)
			{
				return;
			}
			if ((ulong)this._loggingEvents.Count < (ulong)this._maxQueueSize && !this._stopWorkerThread)
			{
				this.EnqueueLoggingEvent(loggingEvent);
				this._messagesLost = false;
				return;
			}
			if (!this._messagesLost)
			{
				this._messagesLost = true;
				LoggingEventData loggingEventDatum = new LoggingEventData()
				{
					Message = "Tcp-Appender dropped messages Messages due to buffer overrun",
					Level = Level.Warn,
					ThreadName = Thread.CurrentThread.Name,
                    TimeStampUtc = DateTime.UtcNow,
					//TimeStamp = DateTime.Now,
					LoggerName = "TcpAppender",
					Domain = AppDomain.CurrentDomain.FriendlyName
				};
				LoggingEventData loggingEventDatum1 = loggingEventDatum;
				lock (this)
				{
					this._loggingEvents.Enqueue(new LoggingEvent(loggingEventDatum1));
				}
				this._queueFullEvent.Set();
			}
		}

		protected override void Append(LoggingEvent[] logEvents)
		{
			if (!this.IsActive)
			{
				return;
			}
			if ((ulong)(this._loggingEvents.Count + (int)logEvents.Length) > (ulong)this._maxQueueSize || this._stopWorkerThread)
			{
				LoggingEvent[] loggingEventArray = logEvents;
				for (int i = 0; i < (int)loggingEventArray.Length; i++)
				{
					this.Append(loggingEventArray[i]);
				}
				return;
			}
			LoggingEvent[] loggingEventArray1 = logEvents;
			for (int j = 0; j < (int)loggingEventArray1.Length; j++)
			{
				this.EnqueueLoggingEvent(loggingEventArray1[j]);
			}
			this._messagesLost = false;
		}

		private void CleanupRecentEvents()
		{
			if (!this.IsActive)
			{
				return;
			}
			if (this._recentEvents.Count > 0 && (DateTime.Now - this._lastCleanupTime).TotalSeconds > 1)
			{
				this._lastCleanupTime = DateTime.Now;
				DateTime dateTime = this._lastCleanupTime.AddSeconds(-5);
				while (this._recentEvents.Count > 0)
				{
					TcpAppender.RenderedLoggingEvent renderedLoggingEvent = this._recentEvents.Peek();
					if (renderedLoggingEvent != null && dateTime < renderedLoggingEvent.TimeStamp)
					{
						return;
					}
					this._recentEvents.Dequeue();
				}
			}
		}

		private void EnqueueLoggingEvent(LoggingEvent loggingEvent)
		{
			string threadName = loggingEvent.ThreadName;
			loggingEvent.GetProperties();
			lock (this)
			{
				this._loggingEvents.Enqueue(loggingEvent);
			}
			this._queueFullEvent.Set();
		}

		protected virtual void InitializeClientConnection()
		{
			if (!this.IsActive)
			{
				return;
			}
			try
			{
				this._listener = new TcpListener(IPAddress.Any, this._localPort);
				this._listener.Start();
				this._stopWorkerThread = false;
				Thread thread = new Thread(new ThreadStart(this.SocketProc))
				{
					Name = "TcpAppender Thread",
					IsBackground = true
				};
				this._workerThread = thread;
				this._workerThread.Start();
			} catch (Exception exception1)
			{
				Exception exception = exception1;
				this.ErrorHandler.Error(string.Concat("Could not initialize the TcpListener on port ", this._localPort.ToString(NumberFormatInfo.InvariantInfo), "."), exception, ErrorCode.GenericFailure);
				this._listener = null;
			}
		}

		protected override void OnClose()
		{
			if (this.IsActive)
			{
				this._stopWorkerThread = true;
				this._queueFullEvent.Set();
				if (this._workerThread != null)
				{
					this._workerThread.Join(2000);
				}
			}
			base.OnClose();
		}

		private void Send(string renderedEvent)
		{
			this.SendBuffer(this.Encoding.GetBytes(renderedEvent));
		}

		private void SendBuffer(byte[] buffer)
		{
			for (int i = this._clients.Count - 1; i >= 0; i--)
			{
				lock (this._clients)
				{
					if (!this._clients[i].SendBuffer(buffer))
					{
						this._clients.RemoveAt(i);
					}
				}
			}
			this._lastSendTime = DateTime.Now;
		}

		private void SendHeartbeat()
		{
			if ((DateTime.Now - this._lastSendTime).TotalMilliseconds > 750)
			{
				this.SendBuffer(this._heartbeat);
			}
		}

		private void SendLogMessages()
		{
			LoggingEvent loggingEvent;
			string str;
			if (!this.IsActive)
			{
				return;
			}
			while (this._loggingEvents.Count > 0)
			{
				lock (this)
				{
					loggingEvent = this._loggingEvents.Dequeue();
					str = base.RenderLoggingEvent(loggingEvent);
				}
				if (this._clients.Count > 0)
				{
					this.Send(str);
				}
				this._recentEvents.Enqueue(new TcpAppender.RenderedLoggingEvent(str, loggingEvent.TimeStamp));
			}
		}

		private void SendRecentEvents(Socket socket)
		{
			Queue<TcpAppender.RenderedLoggingEvent> renderedLoggingEvents = new Queue<TcpAppender.RenderedLoggingEvent>(this._recentEvents);
			while (renderedLoggingEvents.Count > 0 && !this._stopWorkerThread)
			{
				try
				{
					TcpAppender.RenderedLoggingEvent renderedLoggingEvent = renderedLoggingEvents.Dequeue();
					byte[] bytes = this.Encoding.GetBytes(renderedLoggingEvent.RenderedEvent);
					socket.Send(bytes);
				} catch (Exception exception1)
				{
					Exception exception = exception1;
					this.ErrorHandler.Error(string.Format("Unable to send logging event to RemotingServerAppender at port {0}.", this._localPort), exception, ErrorCode.WriteFailure);
				}
			}
		}

		private void Shutdown()
		{
			if (this._clients.Count > 0)
			{
				this.SendLogMessages();
				foreach (TcpClient _client in this._clients)
				{
					_client.Close();
				}
			}
			if (this._listener != null)
			{
				this._listener.Stop();
				this._listener = null;
			}
			this._clients.Clear();
		}

		private void SocketProc()
		{
			do
			{
				this.AcceptPendingClients();
				this.SendLogMessages();
				this.SendHeartbeat();
				this.CleanupRecentEvents();
				Thread.Sleep(100);
				this._queueFullEvent.WaitOne(1000, false);
			}
			while (!this._stopWorkerThread);
			this.Shutdown();
		}
		
		private sealed class Log4netErrorHandler : IInternalErrorHandler
		{
			private readonly IErrorHandler _errorHandler;

			public Log4netErrorHandler(IErrorHandler errorHandler)
			{
				this._errorHandler = errorHandler;
			}

			public void Error(string message, Exception ex)
			{
				this._errorHandler.Error(message, ex);
			}
		}

		private sealed class RenderedLoggingEvent
		{
			private readonly string _renderedEvent;

			private readonly DateTime _eventTime;

			public string RenderedEvent
			{
				get
				{
					return this._renderedEvent;
				}
			}

			public DateTime TimeStamp
			{
				get
				{
					return this._eventTime;
				}
			}

			public RenderedLoggingEvent(string renderedEvent, DateTime time)
			{
				this._renderedEvent = renderedEvent;
				this._eventTime = time;
			}
		}
	}
}