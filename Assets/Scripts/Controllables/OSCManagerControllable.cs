using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSCManagerControllable : Controllable
{
    [Header("OUTPUT SETTINGS")]
    [OSCProperty]
    public int outputPort;
    [OSCProperty]
    public string outputIP;
}
