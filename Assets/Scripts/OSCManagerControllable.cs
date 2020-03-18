using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSCManagerControllable : Controllable
{
    [Header("OUTPUT SETTINGS")]
    [OSCProperty]
    public int OutputPort;
    [OSCProperty]
    public string OutputIP;
    [OSCProperty]
    [Tooltip("Time in s before a connection without heartbeat is deleted. 0 = Never.")]
    public float ConnectionTimeout;
}
