using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;

namespace TestNetworkAvailable
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Uso: TestNetworkAvailable <ip>");
                return;
            }
            var ip = args[0];

            var nic = NetworkInterfaceGet(ip);
            if (nic != null)
            {
                Console.WriteLine($"NIC for {ip} Found!: Status: {nic.OperationalStatus}");
            }
            else
            {
                Console.WriteLine($"NIC for {ip} Not Found!: Global Network Status: {NetworkInterface.GetIsNetworkAvailable()}");
            }
        }

        static protected NetworkInterface NetworkInterfaceGet(string ip)
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in nics)
            {
                IPInterfaceProperties ip_properties = adapter.GetIPProperties();

                //if (!adapter.GetIPProperties().MulticastAddresses.Any())
                //    continue; // most of VPN adapters will be skipped

                if (!adapter.SupportsMulticast)
                    continue; // multicast is meaningless for this type of connection

                if (OperationalStatus.Up != adapter.OperationalStatus)
                    continue; // this adapter is off or not connected

                foreach (UnicastIPAddressInformation inf in ip_properties.UnicastAddresses)
                {
                    if (inf.Address.Equals(IPAddress.Parse(ip)) == true)
                    {
                        return adapter;
                    }
                }
            }
            return null;
        }
    }
}
