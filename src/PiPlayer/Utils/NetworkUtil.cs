using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace PiPlayer.Utils
{
    public static class NetworkUtil
    {
        public static IEnumerable<IPAddress> GetCurrentGatewayAddresses()
        {
            List<IPAddress> ips = new List<IPAddress>();
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    var ipProperties = networkInterface.GetIPProperties();
                    foreach (var gateway in ipProperties.GatewayAddresses)
                    {
                        ips.Add(gateway.Address);
                    }
                }
            }
            return ips.OrderBy(p => p.AddressFamily);
        }

        public static async Task<bool> PingAsync(IPAddress host, CancellationToken cancellationToken)
        {
            using Ping ping = new Ping();
            try
            {
                PingReply reply = await ping.SendPingAsync(host, TimeSpan.FromSeconds(1), cancellationToken: cancellationToken);
                if (reply.Status == IPStatus.Success)
                {
                    return true;
                }
            }
            catch { }
            return false;
        }
    }
}
