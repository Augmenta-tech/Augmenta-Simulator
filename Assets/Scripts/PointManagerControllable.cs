using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointManagerControllable : Controllable {

    public PointManager manager;

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
    public void RemoveAll()
    {
        manager.RemovePoints();
    }

    public override void Awake()
    {
        TargetScript = manager;
        base.Awake();
    }

    public override void DataLoaded()
    {
        manager.ChangeResolution();
    }

    public override void OnUiValueChanged(string name)
    {
        base.OnUiValueChanged(name);
        manager.PointSize = new Vector2(PointSizeX / Width, PointSizeY / Height);
    }
}
