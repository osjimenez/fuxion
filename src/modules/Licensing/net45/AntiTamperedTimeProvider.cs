using Fuxion;
using Fuxion.Logging;
using Fuxion.Threading.Tasks;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Licensing
{
    /// <summary>
    /// 
    /// TO DO
    /// 
    /// - Have a big list of NTP servers (at least 100). Protect this servers addresses in code with obfuscation or similar
    /// - Check 3 NTP servers per try selected randomly to avoid (in the posible) use an sniffer to put all servers in etc/host or similar
    /// - Have relevant web servers (at least 5) to avoid etc/host file or similar redefinition cheat. An user cannot fake google.com or facebook.com domain because the service will be inaccesible
    ///     https://en.wikipedia.org/wiki/List_of_most_popular_websites
    /// - Check and save time every minute aprox to avoid 'always same date' cheat
    /// - Make a comparison between results of all time sources requested to improve accurate time precision as much as possible and discard irrelevant results
    /// - Use more than one repository to avoid that user can remove or modify the information
    /// - Forbid call same server repetitively to avoid IP ban by DoS attack detection systems
    /// - Optional - Put unresponsive servers in a black list or implement a responsive rating system to avoid null results from all requested servers
    /// - Optional - Receive NTP servers from external source to maintain the list up to date. When no connection with this external source, the last obtained server list must stay persistent and encrypted in local machine
    /// 
    /// PROVIDERS
    /// 
    /// - Base provider - Provider to check time in first place ,usually is the local machine time
    /// - Mandatory providers - Check all these providers and all must get me a valid time
    /// - Randomly mandatory providers - Check a customizable number of these providers selected randomly, all must get me a valid time
    /// - Optional providers - Check all these providers but only a customizable number of these must get me a valid value
    /// - Randomly optional providers - Check a customizable number of these providers but only a customizable number of these must get me a valid value
    /// </summary>
    public class AntiTamperedTimeProvider : ITimeProvider
    {
        public AntiTamperedTimeProvider(ITimeProvider reliableTimeProvider, AntiBackTimeProvider antiBackTimeProvider)
        {
            ReliableTimeProvider = reliableTimeProvider;
            AntiBackTimeProvider = antiBackTimeProvider;
        }
        public ILog Log { get; set; }
        public ITimeProvider ReliableTimeProvider { get; set; }
        public AntiBackTimeProvider AntiBackTimeProvider { get; set; }
        DateTime? LastUtcVerifiedTime { get; set; }
        // TODO - Define a margin to move time and review possible tamper scenarios
        private DateTime GetUtc()
        {
            DateTime? antiBack = null;
            DateTime? reliable = null;
            try
            {
                reliable = ReliableTimeProvider.UtcNow();
                Log?.Info("Reliable provider get time successfully");
                AntiBackTimeProvider.SetValue(reliable.Value);
            }
            catch (Exception ex)
            {
                Log?.Error($"Error '{ex.GetType().Name}' in ReliableTimeProvider: {ex.Message}", ex);
            }
            try
            {
                antiBack = AntiBackTimeProvider.UtcNow();
                Log?.Info("Anti-back provider get time successfully");
            }
            catch (Exception ex){
                Log?.Error($"Error '{ex.GetType().Name}' in AntiBackProvider: {ex.Message}", ex);
            }
            if (reliable != null || antiBack != null)
                return (reliable ?? antiBack).Value;
            throw new TamperedTimeException();
        }
        
        public DateTime Now() { return GetUtc().ToLocalTime(); }
        public DateTimeOffset NowOffsetted() { return GetUtc().ToLocalTime(); }
        public DateTime UtcNow() { return GetUtc(); }
    }
    public class TamperedTimeException : FuxionException { }
}
