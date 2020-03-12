using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using UnityOSC;
using System.Linq;

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

    [Tooltip("Time in s before a connection without heartbeat is deleted. 0 = Never.")]
    public float ConnectionTimeout = 60;

    public bool debug = false;

    private Dictionary<string, float> augmentaOutputs; // <ID, timer>
    private KeyValuePair<string, float> tmpOutput;
    private List<string> outputsToDelete;

    private OSCManagerControllable controllable;

    #region MonoBehaviour Implementation

    private void Awake() {

        activeManager = this;

        augmentaOutputs = new Dictionary<string, float>();
        outputsToDelete = new List<string>();
    }

    // Start is called before the first frame update
    void Start()
    {
        controllable = FindObjectOfType<OSCManagerControllable>();

        CreateYoServer();
        CreateAugmentaClient();
    }

    private void Update() {

        
        UpdateOutputsTimers();

    }

    #endregion

    /// <summary>
    /// Increase output timers and delete timed out outputs
    /// </summary>
    void UpdateOutputsTimers() {

        outputsToDelete.Clear();

        for(int i=0; i<augmentaOutputs.Count; i++) {

            tmpOutput = augmentaOutputs.ElementAt(i);

            //Increase output timers
            augmentaOutputs[tmpOutput.Key] = tmpOutput.Value + Time.deltaTime;

            //Check for deletion only if connection timeout is strictly positive
            if (ConnectionTimeout <= 0)
                continue;

            //Mark for deletion
            if (augmentaOutputs[tmpOutput.Key] > ConnectionTimeout)
                outputsToDelete.Add(tmpOutput.Key);
        }

        //Delete timed out outputs
        foreach(var output in outputsToDelete) {

            if(OSCMaster.Clients.ContainsKey(output))
                OSCMaster.RemoveClient(output);

            augmentaOutputs.Remove(output);

            if (debug)
                Debug.Log("Output " + output + " timed out.");
        }
    }

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
            OSCMaster.Clients[output.Key].Send(message);
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
                string outputIP = message.Data[1].ToString();
                int outputPort = (int)message.Data[2];

                string outputID = GetIDFromIPAndPort(outputIP, outputPort);

                //Create output client
                if (!OSCMaster.Clients.ContainsKey(outputID))
                    OSCMaster.CreateClient(outputID, outputIP, outputPort);

                if(!augmentaOutputs.ContainsKey(outputID))
                    augmentaOutputs.Add(outputID, 0);

                if (debug)
                    Debug.Log("Created output " + outputID);

                break;

            case "disconnect":

                //Answer disconnect
                string disconnectIP = message.Data[0].ToString();
                int disconnectPort = (int)message.Data[1];

                string disconnectID = GetIDFromIPAndPort(disconnectIP, disconnectPort);

                if(OSCMaster.Clients.ContainsKey(disconnectID)) {

                    OSCMaster.RemoveClient(disconnectID);

                    if (debug)
                        Debug.Log("Removed output " + disconnectID);
                                                             
                    augmentaOutputs.Remove(disconnectID);
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
                infoMessage.Append(controllable.currentPreset != "" ? controllable.currentPreset : "None");
                infoMessage.Append("Simulator");
                infoMessage.Append("Simulated");

                if (debug)
                    Debug.Log("Answering info from " + infoIP + ":" + infoPort + " with " + NetworkManager.GetIpv4() + " Augmenta Simulator " + NetworkManager.GetMacAddress() + " " + Application.version + " " + controllable.currentPreset + " Simulator Simulated"); ;

                OSCMaster.SendMessage(infoMessage, infoIP, infoPort);
                break;

            case "heartbeat":

                //Answer heartbeat
                string heartbeatIP = message.Data[1].ToString();
                int heartbeatPort = (int)message.Data[2];

                string heartbeatID = GetIDFromIPAndPort(heartbeatIP, heartbeatPort);

                if (debug)
                    Debug.Log("Received heartbeat for " + heartbeatID);

                if (augmentaOutputs.ContainsKey(heartbeatID))
                    augmentaOutputs[heartbeatID] = 0;

                break;
        }
    }

    /// <summary>
    /// Returns an ID based on the IP address and the port
    /// </summary>
    /// <param name="IP"></param>
    /// <param name="port"></param>
    /// <returns></returns>
    string GetIDFromIPAndPort(string IP, int port) {

        return string.Join(":", new string[] { IP, port.ToString() });
    }
}
