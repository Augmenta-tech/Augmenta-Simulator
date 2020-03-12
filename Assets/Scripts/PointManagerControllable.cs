using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointManagerControllable : Controllable {

    [Header("Output Settings")]
    [OSCProperty]
    public bool Mute;

    [Header("Area Settings")]
    [OSCProperty]
    public int Width;
    [OSCProperty]
    public int Height;

    [Header("Points Settings")]
    [OSCProperty(isInteractible = false)]
    public int PointsCount;
    [OSCProperty][Range(0, 30)]
    public int DesiredPointsCount;
    [OSCProperty]
    public float PointSizeX;
    [OSCProperty]
    public float PointSizeY;
    [OSCProperty]
    public float PointSizeZ;

    [OSCProperty][Range(0.0f,10.0f)]
    public float Speed;

    [Header("Noisy Data Simulation")]
    [OSCProperty][Range(0.0f, 1.0f)]
    public float NoiseIntensity;

    [OSCProperty][Range(0.0f, 1.0f)]
    public float IncorrectDetectionProbability = 0;
    [OSCProperty]
    public float IncorrectDetectionDuration = 0.1f;

    [OSCProperty][Range(0.0f, 1.0f)]
    public float PointFlickeringProbability = 0;
    [OSCProperty]
    public float PointFlickeringDuration = 0.1f;

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
        ((PointManager)TargetScript).PointSize = new Vector3(PointSizeX / Width, PointSizeY / Height, PointSizeZ);
    }
}
