using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSCManagerControllable : Controllable
{
    [Header("Output settings")]
    [OSCProperty]
    public int OutputPort;
    [OSCProperty]
    public string OutputIP;
}
