using Fuxion.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Yort.Ntp;

namespace Fuxion.Licensing
{
    [FactoryDefaultImplementation(typeof(DefaultTimeProvider))]
    public interface ITimeProvider
    {
        DateTime GetUtcNow();
    }
    public class DefaultTimeProvider : ITimeProvider
    {
        public DateTime GetUtcNow() { return DateTime.UtcNow; }
    }
    //public class InternetTimeProvider : ITimeProvider
    //{
    //    public void Demo()
    //    {
    //        var client = new NtpClient();
    //        // Oonly disponible in await/async platforms
    //        //var currentTime = await client.RequestTimeAsync();

    //        client.TimeReceived += Client_TimeReceived;
    //        client.ErrorOccurred += Client_ErrorOccurred;
    //        client.BeginRequestTime();
    //    }


    //    private void Client_ErrorOccurred(object sender, NtpNetworkErrorEventArgs e)
    //    {
    //        //TODO: Handle errors here.
    //    }

    //    private void Client_TimeReceived(object sender, NtpTimeReceivedEventArgs e)
    //    {
    //        //TODO: Use retrieved time here. Time is provided by e.CurrentTime.
    //        System.Diagnostics.Debug.WriteLine(e.CurrentTime);
    //    }
    //}
}
