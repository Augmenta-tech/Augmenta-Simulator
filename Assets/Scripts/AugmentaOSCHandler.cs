using System;
using System.Net;
using System.Collections.Generic;
using System.Net.NetworkInformation;

using UnityEngine;
using UnityOSC;

public class AugmentaOSCHandler : MonoBehaviour
{
    public static AugmentaOSCHandler Instance;

    [Header("Network settings")]
    private string _targetIP;
    public string TargetIP
    {
        get
        {
            return _targetIP;
        }
        set
        {
            this._targetIP = value;
            RemoveClient("AugmentaSimulatorOutput");
            CreateClient("AugmentaSimulatorOutput", IPAddress.Parse(TargetIP), TargetPort);
        }
    }
    private int _targetPort;
    public int TargetPort
    {
        get
        {
            return _targetPort;
        }
        set
        {
            this._targetPort = value;
            RemoveClient("AugmentaSimulatorOutput");
            CreateClient("AugmentaSimulatorOutput", IPAddress.Parse(TargetIP), TargetPort);
        }
    }

    public bool debug;

    private string machineAddress;
    private string netWorkInterface;

    private Dictionary<string, OSCClient> Clients = new Dictionary<string, OSCClient>();
	private Dictionary<string, OSCServer> Servers = new Dictionary<string, OSCServer>();

    void Awake()
    {
        Instance = this;
        TargetIP = "127.0.0.1";
        TargetPort = 7000;

        machineAddress = Network.player.ipAddress;
        netWorkInterface = ShowNetworkInterfaces();

        CreateServer("AugmentaYo", 36278);
    }

	public void CreateClient(string clientId, IPAddress destination, int port)
	{
		var client = new OSCClient(destination, port);
		Clients.Add(clientId, client);
	}

    public void RemoveClient(string clientId)
    {
        if (Clients.ContainsKey(clientId))
            Clients.Remove(clientId);
    }

    public OSCServer CreateServer(string serverId, int port)
	{
        OSCServer server = new OSCServer(port);
        server.PacketReceivedEvent += OnPacketReceived;
		
		Servers.Add(serverId, server);

        return server;
	}

    public void RemoveServer(string serverId)
    {
        if(Servers.ContainsKey(serverId))
            Servers.Remove(serverId);
    }

    void OnPacketReceived(OSCServer server, OSCPacket message)
    {
        // YO PROTOCOL
        string[] addSplit = message.Address.Split(new char[] { '/' });
        if (addSplit[1] == "yo")
        {
            var msg = new UnityOSC.OSCMessage("/wassup");
            msg.Append(machineAddress);
            msg.Append(netWorkInterface);
            if(debug)
                Debug.Log("[Augmenta] /wassup " + machineAddress + " " + netWorkInterface);
            SendMessageTo(msg, message.Data[0].ToString(), int.Parse(message.Data[1].ToString()));
        }

        if (addSplit[1] == "connect")
        {
            Clients.Clear();
            TargetIP = message.Data[1].ToString();
            TargetPort = int.Parse(message.Data[2].ToString());
            if (debug)
                Debug.Log("[Augmenta] Sending to " + message.Data[1].ToString() + ":" + message.Data[2].ToString());
            //CreateClient("AugmentaSimulatorOutput", IPAddress.Parse(TargetIP), TargetPort);
        }

        if (addSplit[1] == "info")
        {
            var msg = new UnityOSC.OSCMessage("/info");
            msg.Append(machineAddress);
            msg.Append("Simulator");
            msg.Append(netWorkInterface);
            msg.Append("2.1");
            msg.Append("SIMULATOR");
            msg.Append("SIMULATOR");
            msg.Append("SIMULATOR");
            SendMessageTo(msg, message.Data[0].ToString(), int.Parse(message.Data[1].ToString()));
        }
    }

    private string ShowNetworkInterfaces()
    {
        IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
        NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

        string mac = "";
        foreach (NetworkInterface adapter in nics)
        {
            PhysicalAddress address = adapter.GetPhysicalAddress();
            byte[] bytes = address.GetAddressBytes();
            for (int i = 0; i < bytes.Length; i++)
            {
                mac = string.Concat(mac + (string.Format("{0}", bytes[i].ToString("X2"))));
                if (i != bytes.Length - 1)
                {
                    mac = string.Concat(mac + ":");
                }
            }
            mac += "\n";
            if (debug)
                Debug.Log("Mac : " + mac);
            return mac;
        }
        return null;
    }

    public void SendMessageTo(OSCMessage m, string host, int port)
    {
        CreateClient("tmp", IPAddress.Parse(host), port);
        SendMessage("tmp", m);
        RemoveClient("tmp");
    }

    public void SendMessage(string clientId, OSCMessage msg)
    {
        if (!Clients.ContainsKey(clientId))
        {
            Debug.LogWarning("ClientID not found");
            return;
        }

        Clients[clientId].Send(msg);
    }

    public void SendMessage<T>(string clientId, string address, T value)
	{
		List<object> temp = new List<object>();
		temp.Add(value);
		
		SendMessage(clientId, address, temp);
	}

	public void SendMessage<T>(string clientId, string address, List<T> values)
	{	
		if(Clients.ContainsKey(clientId))
		{
			OSCMessage message = new OSCMessage(address);
		
			foreach(T msgvalue in values)
			{
				message.Append(msgvalue);
			}
			
			Clients[clientId].Send(message);
		}
		else
		{
			Debug.LogError(string.Format("Can't send OSC messages to {0}. Client doesn't exist.", clientId));
		}
	}

    void OnApplicationQuit()
    {
        foreach (KeyValuePair<string, OSCClient> pair in Clients)
        {
            pair.Value.Close();
        }

        foreach (KeyValuePair<string, OSCServer> pair in Servers)
        {
            pair.Value.Close();
        }

        Instance = null;
    }
}	

