using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeManagerControllable : Controllable
{
	[OSCProperty] public string nodeName;

    [OSCProperty] public string sensorType;
    [OSCProperty] public string sensorBrand;
    [OSCProperty] public string sensorName;
    [OSCProperty] public float sensorHFOV;
    [OSCProperty] public float sensorVFOV;

    [OSCProperty] public string floorMode;
    [OSCProperty] public string floorState;
    [OSCProperty] public string backgroundMode;

    [OSCProperty] public string debugPipeName;
    [OSCProperty] public string debugSensor;
    [OSCProperty] public string debugVideoPipe;
    [OSCProperty] public string debugTrackingPipe;
    [OSCProperty] public int debugPID;

    [OSCProperty(isInteractible = false)]
    public string currentTags;

    [OSCProperty] public string newTag;

    [OSCMethod]
    public void AddTag() {

        if ((TargetScript as NodeManager).tagsList.Contains(newTag))
            return;

        (TargetScript as NodeManager).tagsList.Add(newTag);
        (TargetScript as NodeManager).UpdateTagsList();
    }

    [OSCMethod]
    public void RemoveTag() {

        if (!(TargetScript as NodeManager).tagsList.Contains(newTag))
            return;

        (TargetScript as NodeManager).tagsList.Remove(newTag);
        (TargetScript as NodeManager).UpdateTagsList();
    }
}
