using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;

public class WebsocketManager : MonoBehaviour
{
    #region Augmenta Service

    // Declare Augmenta service for the websocket-sharp library
    public class AugmentaService : WebSocketBehavior
    {
        protected override void OnMessage (MessageEventArgs e) {
            //if we receive a message with the server prefix it will be broadcasted to all connected clients
            if (e.IsText && e.Data.StartsWith(serverPrefix)) {
                Sessions.Broadcast(e.Data.Substring(serverPrefix.Length));
            }
        }
    }
    #endregion

    #region Websocket Manager 

	public static WebsocketManager activeManager;

    [Header("Output settings")]
    private int _wsServerPort = 8080;
    public int wsServerPort {
        get { return _wsServerPort; }
        set {
            _wsServerPort = value;
            CreateWebsocketServer();
        }
    }

    private bool _isInitialized = false;

    private WebSocketServer websocketServer;

    private WebSocket _websocketClient; // client used in this class to publish the augmenta data
    
    private static String _serverPrefix = "{\"_server_data_\":{}},"; // used to distinguish messages comming from the server, should not break json data
    public static String serverPrefix {
        get { return _serverPrefix; }
    }


    private void Awake() {
        if (!_isInitialized) {
            Initialize();
        }
    }

    private void OnDestroy() {
        websocketServer.Stop();
    }

    void Initialize() {
        activeManager = this;

        CreateWebsocketServer();
        _isInitialized = true;
    }

    // Start is called before the first frame update
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void CreateWebsocketServer() {

        if(_isInitialized) websocketServer.Stop();

        websocketServer = new WebSocketServer(_wsServerPort);

        websocketServer.AddWebSocketService<AugmentaService>("/");

        websocketServer.Start();

        _websocketClient = new WebSocket("ws://127.0.0.1:"+_wsServerPort);
        _websocketClient.Connect();
    }

    public void SendAugmentaMessage(String msg) {
        _websocketClient.Send(_serverPrefix + msg);
    }

	#endregion
}
