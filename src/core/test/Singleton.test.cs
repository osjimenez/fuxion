using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Fuxion.Test
{
    public class SingletonTest
    {
		[Fact(DisplayName = "Singleton - Add & Get")]
		public void Singleton_AddAndGet()
        {
            var id = Guid.NewGuid();
            Singleton.Add(id);
            var res = Singleton.Get<Guid>();
            Assert.Equal(res, id);
        }
		[Fact(DisplayName = "Singleton - And & Get with Key")]
		public void Singleton_AddAndGetWithKey()
        {
            var id = Guid.NewGuid();
            Singleton.Add("oka", id);
            var res = Singleton.Get<string>(id);
            Assert.Equal("oka", res);
        }
		[Fact(DisplayName = "Singleton - Constants")]
		public void Singleton_Constants()
		{
			SingletonConstants.Run();
			var ip = Singleton.Constants.IpAddress();
			var id = Singleton.Constants.DefaultId();
			Assert.Equal("127.0.0.1", ip);
			Assert.Equal(Guid.Parse("{760B9485-B3A3-477F-B393-8927FAAA0C56}"), id);
		}
    }
	static class SingletonConstants
	{
		public static void Run()
		{
			Singleton.Add("127.0.0.1", nameof(IpAddress));
			Singleton.Add(Guid.Parse("{760B9485-B3A3-477F-B393-8927FAAA0C56}"), nameof(DefaultId));
		}
		internal static string IpAddress(this ISingletonConstants _) => Singleton.Get<string>(nameof(IpAddress));
		internal static Guid DefaultId(this ISingletonConstants _) => Singleton.Get<Guid>(nameof(DefaultId));
	}
}
