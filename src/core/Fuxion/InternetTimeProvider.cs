using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;

namespace Fuxion
{
	public enum InternetTimeServerType
	{
		Ntp,
		Web
	}
	public class InternetTimeProvider : ITimeProvider
	{
		public InternetTimeProvider() => Timeout = TimeSpan.FromSeconds(5);

		public InternetTimeServerType ServerType { get; set; } = InternetTimeServerType.Ntp;
		public string ServerAddress { get; set; } = "time.google.com";//= "time.nist.gov";

		public TimeSpan Timeout { get; set; }
		//private DateTime GetUtc()
		//{
		//	switch (ServerType)
		//	{
		//		case InternetTimeServerType.Ntp:
		//			return FromNtp();
		//		case InternetTimeServerType.Web:
		//			return FromWeb();
		//		default:
		//			throw new ArgumentException($"ServerType value not supported");
		//	}
		//}
		private DateTime GetUtc() => ServerType switch
		{
			InternetTimeServerType.Ntp => FromNtp(),
			InternetTimeServerType.Web => FromWeb(),
			{ } => throw new ArgumentException($"ServerType value not supported")
		};
		private DateTime FromWeb()
		{
			var res = WebRequest.Create(ServerAddress).Transform(r =>
			{
				r.Timeout = (int)Timeout.TotalMilliseconds;
				return r;
			}).GetResponse();
			var dt = DateTimeOffset.ParseExact(res.Headers["date"], "ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AssumeUniversal).DateTime;
			res.Close();
			return dt;
			//return
			//     DateTimeOffset.ParseExact(
			//        WebRequest.Create(ServerAddress).Transform(r =>
			//        {
			//            r.Timeout = (int)Timeout.TotalMilliseconds;
			//            return r;
			//        }).GetResponse().Headers["date"], 
			//        "ddd, dd MMM yyyy HH:mm:ss 'GMT'", 
			//        CultureInfo.InvariantCulture.DateTimeFormat,
			//        DateTimeStyles.AssumeUniversal)
			//        .DateTime;
		}
		private DateTime FromNtp()
		{
			//default Windows time server
			//const string ntpServer = "time.windows.com";

			// NTP message size - 16 bytes of the digest (RFC 2030)
			var ntpData = new byte[48];

			//Setting the Leap Indicator, Version Number and Mode values
			ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

			var addresses = Dns.GetHostEntry(ServerAddress).AddressList;

			//The UDP port number assigned to NTP is 123
			var ipEndPoint = new IPEndPoint(addresses[0], 123);
			//NTP uses UDP
			var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			socket.Connect(ipEndPoint);

			//Stops code hang if NTP is blocked
			socket.ReceiveTimeout = (int)Timeout.TotalMilliseconds;

			socket.Send(ntpData);
			socket.Receive(ntpData);
			socket.Close();

			//Offset to get to the "Transmit Timestamp" field (time at which the reply 
			//departed the server for the client, in 64-bit timestamp format."
			const byte serverReplyTime = 40;

			//Get the seconds part
			ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);

			//Get the seconds fraction
			ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

			//Convert From big-endian to little-endian
			intPart = SwapEndianness(intPart);
			fractPart = SwapEndianness(fractPart);

			var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

			//**UTC** time
			var networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);

			return networkDateTime;
			//return networkDateTime.ToLocalTime();
		}
		// stackoverflow.com/a/3294698/162671
		private uint SwapEndianness(ulong x)
		{
			return (uint)(((x & 0x000000ff) << 24) +
						   ((x & 0x0000ff00) << 8) +
						   ((x & 0x00ff0000) >> 8) +
						   ((x & 0xff000000) >> 24));
		}

		public DateTime Now() => GetUtc().ToLocalTime();
		public DateTimeOffset NowOffsetted() => GetUtc().ToLocalTime();
		public DateTime UtcNow() => GetUtc();

		public override string ToString() => ServerType + " - " + ServerAddress;
	}
}
