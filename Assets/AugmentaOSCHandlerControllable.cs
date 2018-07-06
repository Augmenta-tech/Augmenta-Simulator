using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentaOSCHandlerControllable : Controllable {

    public AugmentaOSCHandler manager;

    [Header("Augmenta output")]
    [OSCProperty]
    public string TargetIP;

    [OSCProperty]
    public int TargetPort;

    public override void Awake()
    {
        TargetScript = manager;
        base.Awake();
    }
}
