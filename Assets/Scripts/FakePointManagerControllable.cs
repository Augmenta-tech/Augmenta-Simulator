using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakePointManagerControllable : Controllable {

    public FakePointManager manager;

    [Header("Network settings")]
    [OSCProperty]
    public string TargetIP;
    [OSCProperty]
    public int TargetPort;

    [Header("Area settings")]
    [OSCProperty]
    public int Width;
    [OSCProperty]
    public int Height;

    [Header("Points settings")]
    [OSCProperty]
    public int NbPoints;
    [OSCProperty]
    [Range(0.05f, 1.0f)]
    public float PointSize;

    [OSCProperty]
    [Range(0.0f,10.0f)]
    public float Speed;

    [OSCMethod]
    public void Clear()
    {
        NbPoints = 0; //TODO : remove this line, shouldn't be required
        manager.Clear();
    }

	public override void Update () {
        //base.Update();
        manager.TargetIP = TargetIP;
        manager.TargetPort = TargetPort;
        manager.Width = Width;
        manager.Height = Height;

        manager.NbPoints = NbPoints;
        manager.Speed = Speed;
        manager.PointSize = PointSize;
    }
}
