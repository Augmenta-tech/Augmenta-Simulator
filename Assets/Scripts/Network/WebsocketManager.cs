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
        activeManager = this;

        if (!_isInitialized) {
            CreateWebsocketServer();
        }
    }

    private void OnDestroy() {

        if(_isInitialized)
            websocketServer.Stop();
    }

    private void CreateWebsocketServer() {

        if (_isInitialized) {
            websocketServer.Stop();
            _isInitialized = false;
        }

        websocketServer = new WebSocketServer(_wsServerPort);

        websocketServer.AddWebSocketService<AugmentaService>("/");

        try {
            websocketServer.Start();
        } catch (Exception e) {
            Debug.LogError(e.Message);
            return;
        }

        _websocketClient = new WebSocket("ws://127.0.0.1:"+_wsServerPort);
        _websocketClient.Connect();

        _isInitialized = true;
    }

    public void SendAugmentaMessage(String msg) {

        if(_isInitialized)
            _websocketClient.Send(_serverPrefix + msg);
    }

	#endregion
}
