using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointManagerControllable : Controllable {

    [Header("Output settings")]
    [OSCProperty]
    public bool Mute;
    [OSCProperty]
    public int OutputPort;
    [OSCProperty]
    public string OutputIP;

    [Header("Area settings")]
    [OSCProperty]
    public int Width;
    [OSCProperty]
    public int Height;

    [Header("Points settings")]
    [OSCProperty]
    [Range(0,30)]
    public int NbPoints;
    [OSCProperty]
    public float PointSizeX;
    [OSCProperty]
    public float PointSizeY;

    [OSCProperty]
    [Range(0.0f,10.0f)]
    public float Speed;

    [OSCProperty]
    [Range(0.0f, 1.0f)]
    public float NoiseIntensity;

    [OSCMethod]
    public void RemoveAll()
    {
        ((PointManager)TargetScript).RemovePoints();
    }

    public override void DataLoaded()
    {
        ((PointManager)TargetScript).ChangeResolution();
    }

    public override void OnUiValueChanged(string name)
    {
        base.OnUiValueChanged(name);
        ((PointManager)TargetScript).PointSize = new Vector2(PointSizeX / Width, PointSizeY / Height);
    }
}
