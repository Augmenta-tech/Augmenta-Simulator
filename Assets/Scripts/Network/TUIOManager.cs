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

    private TUIOManagerControllable _controllable; // if we need to call methods on the controllable we can use this variable

    private bool _initialized = false;

    #region MonoBehaviour Implementation

    private void Awake() {

        if (!_initialized)
            Initialize();
    }

    private void Update() {

        if (!_initialized)
            Initialize();
    }

    #endregion

    void Initialize() {
        activeManager = this;

        _controllable = FindObjectOfType<TUIOManagerControllable>();

        CreateAugmentaClient();

        _initialized = true;
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

