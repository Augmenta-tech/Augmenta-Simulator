using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

public class IPManager
{
    public static string GetIpv4()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        NetworkInterfaceType _type = NetworkInterfaceType.Wireless80211;
        string output = "";
        foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
            {
                foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
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
}