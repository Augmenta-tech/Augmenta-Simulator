using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebsocketManagerControllable : Controllable
{

    [Header("WEBSOCKET SETTINGS")]
    [OSCProperty] public bool enableWebsocket;
    [OSCProperty] public int wsServerPort;

}
