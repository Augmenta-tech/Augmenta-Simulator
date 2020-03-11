using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using UnityOSC;

public class OSCManager : MonoBehaviour
{
    public static OSCManager activeManager;

    [Header("Output settings")]
    private int _outputPort = 12000;
    public int OutputPort {
        get { return _outputPort; }
        set { _outputPort = value; CreateAugmentaClient(); }
    }

    private string _outputIp = "127.0.0.1";
    public string OutputIP {
        get { return _outputIp; }
        set { _outputIp = value; CreateAugmentaClient(); }
    }

    public bool debug = false;

    private List<string> augmentaOutputs;

    private OSCManagerControllable controllable;

    #region MonoBehaviour Implementation

    private void Awake() {

        activeManager = this;

        augmentaOutputs = new List<string>();
    }

    // Start is called before the first frame update
    void Start()
    {
        controllable = FindObjectOfType<OSCManagerControllable>();

        CreateYoServer();
        CreateAugmentaClient();
    }

    #endregion

    /// <summary>
    /// Create client to send Augmenta message
    /// </summary>
    void CreateAugmentaClient() {

            //Create output client
            if (OSCMaster.Clients.ContainsKey("AugmentaSimulatorOutput")) {
                OSCMaster.RemoveClient("AugmentaSimulatorOutput");
            }
            OSCMaster.CreateClient("AugmentaSimulatorOutput", IPAddress.Parse(OutputIP), OutputPort);
    }

    /// <summary>
    /// Send message through the Augmenta client
    /// </summary>
    /// <param name="msg"></param>
    public void SendAugmentaMessage(OSCMessage message) {

        OSCMaster.Clients["AugmentaSimulatorOutput"].Send(message);

        foreach(var output in augmentaOutputs)
            OSCMaster.Clients[output].Send(message);
    }

    /// <summary>
    /// Create receiver for yo protocol
    /// </summary>
    void CreateYoServer() {

        if (OSCMaster.Receivers.ContainsKey("AugmentaYo"))
            return;

        OSCMaster.CreateReceiver("AugmentaYo", 36278);
        OSCMaster.Receivers["AugmentaYo"].messageReceived += OnYoMessageReceived;
    }

    /// <summary>
    /// Answer yo protocol
    /// </summary>
    void OnYoMessageReceived(OSCMessage message) {

        string[] addressSplit = message.Address.Split('/');

        switch (addressSplit[1]) {

            case "yo":

                //Answer yo
                string yoIP = message.Data[0].ToString();
                int yoPort = (int)message.Data[1];

                OSCMessage wassupMessage = new OSCMessage("/wassup");

                wassupMessage.Append(NetworkManager.GetIpv4());
                wassupMessage.Append(NetworkManager.GetMacAddress());

                if (debug)
                    Debug.Log("Answering yo from "+yoIP+":"+yoPort+" with " + NetworkManager.GetIpv4() + " and " + NetworkManager.GetMacAddress());

                OSCMaster.SendMessage(wassupMessage, yoIP, yoPort);

                break;

            case "connect":

                //Answer connect
                string outputID = message.Data[0].ToString();
                string outputIP = message.Data[1].ToString();
                int outputPort = (int)message.Data[2];

                //Create output client
                if (OSCMaster.Clients.ContainsKey(outputID)) {
                    OSCMaster.RemoveClient(outputID);
                }
                OSCMaster.CreateClient(outputID, outputIP, outputPort);

                augmentaOutputs.Add(outputID);

                if (debug)
                    Debug.Log("Created output " + outputID + " at " + outputIP + ":" + outputPort);

                break;

            case "disconnect":

                //Answer disconnect
                string disconnectIP = message.Data[0].ToString();
                int disconnectPort = (int)message.Data[1];

                foreach(var client in OSCMaster.Clients) {
                    if(client.Value.ClientIPAddress.ToString() == disconnectIP && client.Value.Port == disconnectPort) {
                        OSCMaster.RemoveClient(client.Key);

                        if (debug)
                            Debug.Log("Removed output " + client.Key + " at " + disconnectIP + ":" + disconnectPort);

                        augmentaOutputs.Remove(client.Key);
                    }
                }

                break;

            case "ping":

                //Answer ping
                string pongIP = message.Data[0].ToString();
                int pongPort = (int)message.Data[1];

                OSCMessage pongMessage = new OSCMessage("/pong");

                if (debug)
                    Debug.Log("Answering ping from " + pongIP + ":" + pongPort);

                OSCMaster.SendMessage(pongMessage, pongIP, pongPort);

                break;

            case "info":

                //Answer info
                string infoIP = message.Data[0].ToString();
                int infoPort = (int)message.Data[1];

                OSCMessage infoMessage = new OSCMessage("/info");

                infoMessage.Append(NetworkManager.GetIpv4());
                infoMessage.Append("Augmenta Simulator");
                infoMessage.Append(NetworkManager.GetMacAddress());
                infoMessage.Append(Application.version);
                infoMessage.Append(controllable.currentPreset);
                infoMessage.Append("Simulator");
                infoMessage.Append("Simulated");

                if (debug)
                    Debug.Log("Answering info from " + infoIP + ":" + infoPort + " with " + NetworkManager.GetIpv4() + " Augmenta Simulator " + NetworkManager.GetMacAddress() + " " + Application.version + " " + controllable.currentPreset + " Simulator Simulated"); ;

                OSCMaster.SendMessage(infoMessage, infoIP, infoPort);
                break;

            case "heartbeat":

                //Answer heartbeat
                if (debug)
                    Debug.Log("Received heartbeat");
                break;
        }
    }
}
