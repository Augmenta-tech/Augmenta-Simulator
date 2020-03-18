using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointManagerControllable : Controllable {

    [Header("OUTPUT SETTINGS")]
    [OSCProperty]
    public bool Mute;

    public List<string> ProtocolVersions;
    [OSCProperty(TargetList ="ProtocolVersions", IncludeInPresets = true)] public string ProtocolVersion;

    [Header("AREA SETTINGS")]
    [OSCProperty]
    [Tooltip("in meters")]
    public float Width;
    [OSCProperty]
    [Tooltip("in meters")]
    public float Height;
    [OSCProperty]
    public float MetersPerPixel;



    [Header("POINTS SETTINGS")]
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

    [Header("NOISY DATA SIMULATION")]
    [OSCProperty][Range(0.0f, 1.0f)][Tooltip("Movement noise")]
    public float NoiseIntensity;

    [OSCProperty][Range(0.0f, 1.0f)][Tooltip("False positives")]
    public float IncorrectDetectionProbability = 0;
    [OSCProperty]
    public float IncorrectDetectionDuration = 0.1f;

    [OSCProperty][Range(0.0f, 1.0f)][Tooltip("False negatives")]
    public float PointFlickeringProbability = 0;
    [OSCProperty]
    public float PointFlickeringDuration = 0.1f;

    [OSCMethod]
    public void RemoveAll()
    {
        ((PointManager)TargetScript).RemovePoints();
    }

    public override void OnUiValueChanged(string name)
    {
        base.OnUiValueChanged(name);
        ((PointManager)TargetScript).PointSize = new Vector3(PointSizeX / Width, PointSizeY / Height, PointSizeZ);
        ((PointManager)TargetScript).ProtocolVersion = ProtocolVersion;
    }

    public override void OnScriptValueChanged(string name) {
        base.OnScriptValueChanged(name);
        ProtocolVersion = ((PointManager)TargetScript).ProtocolVersion;
        ProtocolVersions = ((PointManager)TargetScript).ProtocolVersions;
    }
}
