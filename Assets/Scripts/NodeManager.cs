using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeManager : MonoBehaviour
{
    public ZeroconfManager zeroconfManager;

    public string nodeName {
        get { return _nodeName; }
        set { _nodeName = value; UpdateName(); }
    }
    private string _nodeName = "node";

    public List<string> tagsList = new List<string>();
    [HideInInspector] public string currentTags;
    [HideInInspector] public string newTag;

    public string sensorType = "DepthCamera";
    public string sensorBrand = "Orbbec";
    public string sensorName = "AstraPro";
    public float sensorHFOV = 60.0f;
    public float sensorVFOV = 49.5f;

    public string floorMode = "Auto";
    public string floorState = "Found";
    public string backgroundMode = "Auto";

    public string debugPipeName = "OrbbecAstra";
    public string debugSensor = "OrbbecAstraPro";
    public string debugVideoPipe = "DepthPipe";
    public string debugTrackingPipe = "TrackingV1";
    public int debugPID = 69;

    private void Awake() {

        UpdateName();
    }

    void UpdateName() {

        zeroconfManager.UpdateName(_nodeName);
    }

    public void UpdateTagsList() {

        currentTags = "";

        foreach (string tag in tagsList)
            currentTags += tag + " ";
    }
}
