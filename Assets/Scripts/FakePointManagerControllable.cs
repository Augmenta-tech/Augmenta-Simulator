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
    [OSCProperty]
    public bool Mute;

    [Header("Area settings")]
    [OSCProperty]
    public int Width;
    [OSCProperty]
    public int Height;

    [Header("Points settings")]
    [OSCProperty]
    public int NbPoints;
    [OSCProperty]
    public float PointSizeX;
    [OSCProperty]
    public float PointSizeY;

    [OSCProperty]
    [Range(0.0f,10.0f)]
    public float Speed;

    [OSCMethod]
    public void RemovePoints()
    {
        manager.RemovePoints();
    }

    public override void Awake()
    {
        TargetScript = manager;
        base.Awake();
    }

    public override void OnUiValueChanged(string name)
    {
        base.OnUiValueChanged(name);
        manager.PointSize = new Vector2(PointSizeX / Width, PointSizeY / Height);
    }
}
