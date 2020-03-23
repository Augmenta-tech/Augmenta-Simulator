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
    public float Width;
    [OSCProperty]
    [Tooltip("in meters")]
    public float Height;
    [OSCProperty][Tooltip("Meter to pixel conversion when using protocol V1")]
    public float MeterPerPixel;



    [Header("POINTS GENERAL SETTINGS")]
    [OSCProperty(isInteractible = false)]
    public int PointsCount;
    [OSCProperty][Range(0, 30)]
    public int DesiredPointsCount;
    [OSCProperty]
    [Range(0.0f, 10.0f)]
    public float Speed;

    [Header("POINTS SIZE SETTINGS")]
    [OSCProperty]
    public Vector3 MinPointSize;
    [OSCProperty][Tooltip("in meters")]
    public Vector3 MaxPointSize;
    [OSCProperty]
    public bool AnimateSize;
    [OSCProperty][Range(0.0f, 10.0f)]
    public float SizeVariationSpeed;

    [Header("NOISY DATA SIMULATION")]
    [OSCProperty][Range(0.0f, 1.0f)]
    public float MovementNoise;

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
        ((PointManager)TargetScript).ProtocolVersion = ProtocolVersion;
    }

    public override void OnScriptValueChanged(string name) {
        base.OnScriptValueChanged(name);
        ProtocolVersion = ((PointManager)TargetScript).ProtocolVersion;
        ProtocolVersions = ((PointManager)TargetScript).ProtocolVersions;
    }
}
