using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using UnityOSC;
using System.Linq;

public class OSCManager : MonoBehaviour
{
    public static OSCManager activeManager;

    public ZeroconfManager zeroconfManager;

    [Header("Yo Version")]
    public int yoVersion = 2;
    public int yoPort = 36278;

    [Header("Output settings")]
    private int _outputPort = 12000;
    public int outputPort {
        get { return _outputPort; }
        set { _outputPort = value; CreateAugmentaClient(); }
    }

    private string _outputIp = "127.0.0.1";
    public string outputIP {
        get { return _outputIp; }
        set { _outputIp = value; CreateAugmentaClient(); }
    }

    [Tooltip("Time in s before a connection without heartbeat is deleted. 0 = Never.")]
    public float connectionTimeout = 60;

    public bool debug = false;

    private Dictionary<string, float> _augmentaOutputs; // <ID, timer>
    private KeyValuePair<string, float> _tmpOutput;
    private List<string> _outputsToDelete;

    private OSCManagerControllable _controllable;
    private NodeManager _nodeManager;

    private bool _initialized = false;
    private bool _yoServerCreated = false;

    #region MonoBehaviour Implementation

    private void Awake() {

        if (!_initialized)
            Initialize();
    }

    private void Update() {

        if (!_initialized)
            Initialize();

        if (!_yoServerCreated)
            CreateYoServer();

        UpdateOutputsTimers();
    }

    #endregion

    void Initialize() {
        activeManager = this;

        _augmentaOutputs = new Dictionary<string, float>();
        _outputsToDelete = new List<string>();

        _controllable = FindObjectOfType<OSCManagerControllable>();
        _nodeManager = FindObjectOfType<NodeManager>();

        CreateYoServer();
        CreateAugmentaClient();

        _initialized = true;
    }

    /// <summary>
    /// Increase output timers and delete timed out outputs
    /// </summary>
    void UpdateOutputsTimers() {

        _outputsToDelete.Clear();

        for(int i=0; i<_augmentaOutputs.Count; i++) {

            _tmpOutput = _augmentaOutputs.ElementAt(i);

            //Increase output timers
            _augmentaOutputs[_tmpOutput.Key] = _tmpOutput.Value + Time.deltaTime;

            //Check for deletion only if connection timeout is strictly positive
            if (connectionTimeout <= 0)
                continue;

            //Mark for deletion
            if (_augmentaOutputs[_tmpOutput.Key] > connectionTimeout)
                _outputsToDelete.Add(_tmpOutput.Key);
        }

        //Delete timed out outputs
        foreach(var output in _outputsToDelete) {

            if(OSCMaster.Clients.ContainsKey(output))
                OSCMaster.RemoveClient(output);

            _augmentaOutputs.Remove(output);

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
            OSCMaster.CreateClient("AugmentaSimulatorOutput", IPAddress.Parse(outputIP), outputPort);
    }

    /// <summary>
    /// Send message through the Augmenta client
    /// </summary>
    /// <param name="msg"></param>
    public void SendAugmentaMessage(OSCMessage message) {

        if (!_initialized)
            Initialize();

        OSCMaster.Clients["AugmentaSimulatorOutput"].Send(message);

        foreach (var output in _augmentaOutputs)
            OSCMaster.Clients[output.Key].Send(message);
    }

    /// <summary>
    /// Create receiver for yo protocol
    /// </summary>
    void CreateYoServer() {

        if (OSCMaster.Receivers.ContainsKey("AugmentaYo"))
            return;

        try {
            OSCMaster.CreateReceiver("AugmentaYo", yoPort);
            OSCMaster.Receivers["AugmentaYo"].messageReceived += OnYoMessageReceived;
            zeroconfManager.Setup(yoPort);
            _yoServerCreated = true;
        } catch {
            Debug.LogError("Failed to create Augmenta Yo server.");
            yoPort++;
		}
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
                wassupMessage.Append(_nodeManager ? _nodeManager.nodeName : "Simulator");
                wassupMessage.Append(_nodeManager ? string.Join(",", _nodeManager.tagsList) : "");

                if (debug)
                    Debug.Log("Answering yo from "+yoIP+":"+yoPort+" with " + NetworkManager.GetIpv4() + " and " + NetworkManager.GetMacAddress());

                try
                {
                    OSCMaster.SendMessage(wassupMessage, yoIP, yoPort);
                }catch(System.Exception e)
                {
                    Debug.LogWarning("Error sending OSC to " + yoIP + ":" + yoPort + " (" + e.Message + ")");
                }

                break;

            case "connect":

                switch (yoVersion) {
                    case 1:
                        HandleConnectV1(message);
                        break;

                    case 2:
                        HandleConnectV2(message);
                        break;
                }

                break;

            case "disconnect":

                switch (yoVersion) {
                    case 1:
                        HandleDisconnectV1(message);
                        break;

                    case 2:
                        HandleDisconnectV2(message);
                        break;
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

                if (debug)
                    Debug.Log("Answering info from " + infoIP + ":" + infoPort + " with " + NetworkManager.GetIpv4() + " Augmenta Simulator " + NetworkManager.GetMacAddress() + " " + Application.version + " " + _controllable.currentPreset + " Simulator Simulated"); ;

                SendInfoMessages(infoIP, infoPort);
                break;

            case "heartbeat":

                switch (yoVersion) {
                    case 1:
                        HandleHeartbeatV1(message);
                        break;

                    case 2:
                        HandleHeartbeatV2(message);
                        break;
                }

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

    void HandleConnectV1(OSCMessage message) {

        //Answer connect
        string outputIP = message.Data[1].ToString();
        int outputPort = (int)message.Data[2];

        string outputID = GetIDFromIPAndPort(outputIP, outputPort);

        //Create output client
        if (!OSCMaster.Clients.ContainsKey(outputID))
            OSCMaster.CreateClient(outputID, outputIP, outputPort);

        if (!_augmentaOutputs.ContainsKey(outputID))
            _augmentaOutputs.Add(outputID, 0);

        if (debug)
            Debug.Log("Created output " + outputID);
    }

    void HandleConnectV2(OSCMessage message) {

        //Answer connect
        string outputIP = message.Data[0].ToString();
        int outputPort = (int)message.Data[1];
        string protocolType = message.Data[2].ToString();
        int version = (int)message.Data[3];

        if(protocolType != "osc" && protocolType != "OSC") {
            if (debug)
                Debug.Log("Can only create osc protocol type.");

            return;
        }

        string outputID = GetIDFromIPAndPort(outputIP, outputPort);

        //Create output client
        if (!OSCMaster.Clients.ContainsKey(outputID))
            OSCMaster.CreateClient(outputID, outputIP, outputPort);

        if (!_augmentaOutputs.ContainsKey(outputID))
            _augmentaOutputs.Add(outputID, 0);

        if (debug)
            Debug.Log("Created output " + outputID);
    }

    void HandleDisconnectV1(OSCMessage message) {

        //Answer disconnect
        string disconnectIP = message.Data[0].ToString();
        int disconnectPort = (int)message.Data[1];

        string disconnectID = GetIDFromIPAndPort(disconnectIP, disconnectPort);

        if (OSCMaster.Clients.ContainsKey(disconnectID)) {

            OSCMaster.RemoveClient(disconnectID);

            if (debug)
                Debug.Log("Removed output " + disconnectID);

            _augmentaOutputs.Remove(disconnectID);
        }
    }

    void HandleDisconnectV2(OSCMessage message) {

        //Answer disconnect
        string disconnectIP = message.Data[0].ToString();
        int disconnectPort = (int)message.Data[1];

        if (message.Data.Count > 2)
        {
            string protocolType = message.Data[2].ToString();
            int version = (int)message.Data[3];
        }

        string disconnectID = GetIDFromIPAndPort(disconnectIP, disconnectPort);

        if (OSCMaster.Clients.ContainsKey(disconnectID)) {

            OSCMaster.RemoveClient(disconnectID);

            if (debug)
                Debug.Log("Removed output " + disconnectID);

            _augmentaOutputs.Remove(disconnectID);
        }
    }

    void HandleHeartbeatV1(OSCMessage message) {

        //Answer heartbeat
        string heartbeatIP = message.Data[1].ToString();
        int heartbeatPort = (int)message.Data[2];

        string heartbeatID = GetIDFromIPAndPort(heartbeatIP, heartbeatPort);

        if (debug)
            Debug.Log("Received heartbeat for " + heartbeatID);

        if (_augmentaOutputs.ContainsKey(heartbeatID))
            _augmentaOutputs[heartbeatID] = 0;
    }

    void HandleHeartbeatV2(OSCMessage message) {

        //Answer heartbeat
        string heartbeatIP = message.Data[0].ToString();
        int heartbeatPort = (int)message.Data[1];
        string protocolType = message.Data[2].ToString();
        int version = (int)message.Data[3];

        if (protocolType != "osc" && protocolType != "OSC") {
            if (debug)
                Debug.Log("Can only send osc protocol type.");

            return;
        }

        string heartbeatID = GetIDFromIPAndPort(heartbeatIP, heartbeatPort);

        if (debug)
            Debug.Log("Received heartbeat for " + heartbeatID);

        if (_augmentaOutputs.ContainsKey(heartbeatID))
            _augmentaOutputs[heartbeatID] = 0;

    }

    void SendInfoMessages(string infoIP, int infoPort) {

        switch (ProtocolVersionManager.protocolVersion) {
            case ProtocolVersionManager.AugmentaProtocolVersion.V1:
                SendInfoMessagesV1(infoIP, infoPort);
                break;
            case ProtocolVersionManager.AugmentaProtocolVersion.V2:
                SendInfoMessagesV2(infoIP, infoPort);
                break;
        }

    }

    void SendInfoMessagesV1(string infoIP, int infoPort) {

        OSCMessage infoMessage = new OSCMessage("/info");

        infoMessage.Append(NetworkManager.GetIpv4());
        infoMessage.Append("Augmenta Simulator");
        infoMessage.Append(NetworkManager.GetMacAddress());
        infoMessage.Append(Application.version);
        infoMessage.Append(_controllable.currentPreset != "" ? _controllable.currentPreset : "None");
        infoMessage.Append("Simulator");
        infoMessage.Append("Simulated");

        OSCMaster.SendMessage(infoMessage, infoIP, infoPort);
    }

    void SendInfoMessagesV2(string infoIP, int infoPort) {

        //Fusion 
        OSCMessage infoMessage = new OSCMessage("/info/name");
        if (_nodeManager) {
            infoMessage.Append(_nodeManager.nodeName);
        } else {
            infoMessage.Append("Augmenta Simulator");
        }
        OSCMaster.SendMessage(infoMessage, infoIP, infoPort);

        infoMessage = new OSCMessage("/info/type");
        if (_nodeManager) {
            infoMessage.Append("Node");
        } else {
            infoMessage.Append("Simulator");
        }
        OSCMaster.SendMessage(infoMessage, infoIP, infoPort);

        infoMessage = new OSCMessage("/info/mac");
        infoMessage.Append(NetworkManager.GetMacAddress());
        OSCMaster.SendMessage(infoMessage, infoIP, infoPort);

        infoMessage = new OSCMessage("/info/ip");
        infoMessage.Append(NetworkManager.GetIpv4());
        OSCMaster.SendMessage(infoMessage, infoIP, infoPort);

        infoMessage = new OSCMessage("/info/version");
        infoMessage.Append(Application.version);
        OSCMaster.SendMessage(infoMessage, infoIP, infoPort);

        infoMessage = new OSCMessage("/info/currentFile");
        infoMessage.Append(_controllable.currentPreset != "" ? _controllable.currentPreset : "None");
        OSCMaster.SendMessage(infoMessage, infoIP, infoPort);

        infoMessage = new OSCMessage("/info/protocolAvailable");
        infoMessage.Append("OSC");
        infoMessage.Append("1");
        infoMessage.Append("2");
        OSCMaster.SendMessage(infoMessage, infoIP, infoPort);

        if (_nodeManager) {
            //Node
            infoMessage = new OSCMessage("/info/sensor/type");
            infoMessage.Append(_nodeManager.sensorType);
            OSCMaster.SendMessage(infoMessage, infoIP, infoPort);

            infoMessage = new OSCMessage("/info/sensor/brand");
            infoMessage.Append(_nodeManager.sensorBrand);
            OSCMaster.SendMessage(infoMessage, infoIP, infoPort);

            infoMessage = new OSCMessage("/info/sensor/name");
            infoMessage.Append(_nodeManager.sensorName);
            OSCMaster.SendMessage(infoMessage, infoIP, infoPort);

            infoMessage = new OSCMessage("/info/sensor/hFov");
            infoMessage.Append(_nodeManager.sensorHFOV);
            OSCMaster.SendMessage(infoMessage, infoIP, infoPort);

            infoMessage = new OSCMessage("/info/sensor/vFov");
            infoMessage.Append(_nodeManager.sensorVFOV);
            OSCMaster.SendMessage(infoMessage, infoIP, infoPort);

            infoMessage = new OSCMessage("/info/floorMode");
            infoMessage.Append(_nodeManager.floorMode);
            OSCMaster.SendMessage(infoMessage, infoIP, infoPort);

            infoMessage = new OSCMessage("/info/floorState");
            infoMessage.Append(_nodeManager.floorState);
            OSCMaster.SendMessage(infoMessage, infoIP, infoPort);

            infoMessage = new OSCMessage("/info/backgroundMode");
            infoMessage.Append(_nodeManager.backgroundMode);
            OSCMaster.SendMessage(infoMessage, infoIP, infoPort);

            infoMessage = new OSCMessage("/info/debug/pipeName");
            infoMessage.Append(_nodeManager.debugPipeName);
            OSCMaster.SendMessage(infoMessage, infoIP, infoPort);

            infoMessage = new OSCMessage("/info/debug/sensor");
            infoMessage.Append(_nodeManager.debugSensor);
            OSCMaster.SendMessage(infoMessage, infoIP, infoPort);

            infoMessage = new OSCMessage("/info/debug/videoPipe");
            infoMessage.Append(_nodeManager.debugVideoPipe);
            OSCMaster.SendMessage(infoMessage, infoIP, infoPort);

            infoMessage = new OSCMessage("/info/debug/trackingPipe");
            infoMessage.Append(_nodeManager.debugTrackingPipe);
            OSCMaster.SendMessage(infoMessage, infoIP, infoPort);

            infoMessage = new OSCMessage("/info/debug/pid");
            infoMessage.Append(_nodeManager.debugPID);
            OSCMaster.SendMessage(infoMessage, infoIP, infoPort);

            infoMessage = new OSCMessage("/info/tags");
            foreach(string tag in _nodeManager.tagsList)
                infoMessage.Append(tag);
            OSCMaster.SendMessage(infoMessage, infoIP, infoPort);
        }

    }
}
