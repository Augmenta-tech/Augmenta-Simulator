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
    public int outputPort {
        get { return _outputPort; }
        set { _outputPort = value; CreateAugmentaClient(); }
    }

    private string _outputIp = "127.0.0.1";
    public string outputIP {
        get { return _outputIp; }
        set { _outputIp = value; CreateAugmentaClient(); }
    }

    private OSCManagerControllable _controllable;

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

        _controllable = FindObjectOfType<OSCManagerControllable>();

        CreateAugmentaClient();

        _initialized = true;
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
    }
}
