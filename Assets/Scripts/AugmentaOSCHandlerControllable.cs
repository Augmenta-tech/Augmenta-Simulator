using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentaOSCHandlerControllable : Controllable {

    [Header("Augmenta output")]
    [OSCProperty]
    public string TargetIP;

    [OSCProperty]
    public int TargetPort;
}
