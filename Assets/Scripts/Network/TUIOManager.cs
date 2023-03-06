using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using UnityOSC;
using System.Linq;
using System;

public class TUIOManager : MonoBehaviour
{
    public static TUIOManager activeManager;

    [Header("Output settings")]
    private int _TUIOPort = 3333;
    public int TUIOPort {
        get { return _TUIOPort; }
        set { _TUIOPort = value; CreateAugmentaClient(); }
    }

    private int _outputScene = 9001;
    public int outputScene
    {
        get { return _outputScene; }
        set { _outputScene = value; CreateAugmentaClient(); }
    }

    private string _outputIp = "127.0.0.1";
    public string outputIP {
        get { return _outputIp; }
        set { _outputIp = value; CreateAugmentaClient(); }
    }

    public string descriptor;
    public string preset;
    public string dimension;

    public enum AugmentaTUIODescriptor
    {
        OBJECT,
        CURSOR,
        BLOB
    }

    public enum AugmentaTUIODimension
    {
        TUIO2D,
        TUIO25D,
        TUIO3D
    }

    public enum AugmentaTUIOPreset
    {
        NONE,
        NOTCH,
        TOUCHDESIGNER,
        MINIMAL,
        BEST
    }

    public AugmentaTUIODescriptor TUIODescriptor
    {
        get { return _TUIODescriptor; }
        set { _TUIODescriptor = value; }
    }
    private AugmentaTUIODescriptor _TUIODescriptor = AugmentaTUIODescriptor.OBJECT;

    public AugmentaTUIODimension TUIODimension
    {
        get { return _TUIODimension; }
        set { _TUIODimension = value; }
    }
    private AugmentaTUIODimension _TUIODimension = AugmentaTUIODimension.TUIO25D;

    public AugmentaTUIOPreset TUIOPreset
    {
        get { return _TUIOPreset; }
        set { _TUIOPreset = value; }
    }
    private AugmentaTUIOPreset _TUIOPreset = AugmentaTUIOPreset.NONE;

    public float sceneDepth = 10;

    private Dictionary<string, float> _augmentaOutputs; // <ID, timer>
    private KeyValuePair<string, float> _tmpOutput;
    private List<string> _outputsToDelete;

    private TUIOManagerControllable _controllable;
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

        UpdateOutputsTimers();
    }

    #endregion

    void Initialize() {
        activeManager = this;

        _augmentaOutputs = new Dictionary<string, float>();
        _outputsToDelete = new List<string>();

        _controllable = FindObjectOfType<TUIOManagerControllable>();
        _nodeManager = FindObjectOfType<NodeManager>();

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


        }

        //Delete timed out outputs
        foreach(var output in _outputsToDelete) {

            if(OSCMaster.Clients.ContainsKey(output))
                OSCMaster.RemoveClient(output);

            _augmentaOutputs.Remove(output);
        }
    }

    /// <summary>
    /// Create client to send Augmenta message
    /// </summary>
    void CreateAugmentaClient() {

            //Create output client
            if (OSCMaster.Clients.ContainsKey("AugmentaSimulatorOutputTUIO")) {
                OSCMaster.RemoveClient("AugmentaSimulatorOutputTUIO");
            }
            OSCMaster.CreateClient("AugmentaSimulatorOutputTUIO", IPAddress.Parse(outputIP), TUIOPort);

            if (OSCMaster.Clients.ContainsKey("AugmentaSimulatorOutputScene"))
            {
                OSCMaster.RemoveClient("AugmentaSimulatorOutputScene");
            }
            OSCMaster.CreateClient("AugmentaSimulatorOutputScene", IPAddress.Parse(outputIP), outputScene);
    }

    /// <summary>
    /// Send message through the Augmenta client
    /// </summary>
    /// <param name="msg"></param>
    public void SendAugmentaMessage(OSCMessage message, string client) {

        if (!_initialized)
            Initialize();

        if (client == "TUIO") { OSCMaster.Clients["AugmentaSimulatorOutputTUIO"].Send(message); }
        if (client == "Scene") { OSCMaster.Clients["AugmentaSimulatorOutputScene"].Send(message); }
        
        foreach (var output in _augmentaOutputs)
            OSCMaster.Clients[output.Key].Send(message);
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

    public String GetAddressTUIO(AugmentaTUIODescriptor description, AugmentaTUIODimension dimension)
    {
        string addr = "";
        switch (description)
        {
            case AugmentaTUIODescriptor.OBJECT:
                switch (dimension)
                {
                    case AugmentaTUIODimension.TUIO2D:
                        addr = "/tuio/2Dobj";
                        break;
                    case AugmentaTUIODimension.TUIO25D:
                        addr = "/tuio/25Dobj";
                        break;
                    case AugmentaTUIODimension.TUIO3D:
                        addr = "/tuio/3Dobj";
                        break;
                }

                break;

            case AugmentaTUIODescriptor.CURSOR:
                switch (dimension)
                {
                    case AugmentaTUIODimension.TUIO2D:
                        addr = "/tuio/2Dcur";
                        break;
                    case AugmentaTUIODimension.TUIO25D:
                        addr = "/tuio/25Dcur";
                        break;
                    case AugmentaTUIODimension.TUIO3D:
                        addr = "/tuio/3Dcur";
                        break;
                }

                break;

            case AugmentaTUIODescriptor.BLOB:
                switch (dimension)
                {
                    case AugmentaTUIODimension.TUIO2D:
                        addr = "/tuio/2Dblb";
                        break;
                    case AugmentaTUIODimension.TUIO25D:
                        addr = "/tuio/25Dblb";
                        break;
                    case AugmentaTUIODimension.TUIO3D:
                        addr = "/tuio/3Dblb";
                        break;
                }
                break;
        }

        return addr;
    }
}

