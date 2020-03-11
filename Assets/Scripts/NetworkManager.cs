using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Linq;

public class NetworkManager
{
    public static string GetIpv4()
    {
#if UNITY_EDITOR || UNITY_STANDALONE

        string output = "";
        foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (item.OperationalStatus == OperationalStatus.Up)
            {
                foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork && ip.Address.ToString() != "127.0.0.1")
                    {
                        output = ip.Address.ToString();
                    }
                }
            }
        }
        return output;
#else
        return GetIpv4Mobile();
#endif
    }

    private static string GetIpv4Mobile()
    {
        string deviceHostName = Dns.GetHostName();
        var ipCount = Dns.GetHostAddresses(deviceHostName);
        for (int i = 0; i < ipCount.Length; i++)
        {
            IPAddress deviceIP = ipCount[i];
            if (deviceIP.AddressFamily == AddressFamily.InterNetwork)
            {
                return deviceIP.ToString();
            }
        }
        return null;
    }

    public static string GetMacAddress() {

        string output = "";

        foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces()) {

            if (item.OperationalStatus != OperationalStatus.Up)
                continue;

            if (item.GetPhysicalAddress().ToString() != "") {

                output = string.Join(":", item.GetPhysicalAddress().GetAddressBytes().Select(b => b.ToString("X2")));
            }
        }

        return output;
    }
}