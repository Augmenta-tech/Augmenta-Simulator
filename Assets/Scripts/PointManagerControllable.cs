using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointManagerControllable : Controllable {

    [Header("OUTPUT SETTINGS")]
    [OSCProperty]
    public bool mute;

    public List<string> protocolVersions;
    [OSCProperty(TargetList ="protocolVersions", IncludeInPresets = true)] public string protocolVersion;

    [Header("SCENE SETTINGS")]
    [OSCProperty]
    public float width;
    [OSCProperty]
    [Tooltip("in meters")]
    public float height;
    [OSCProperty]
    [Tooltip("in meters, to convert the area size to pixels.")]
    public float pixelSize;

    [Header("POINTS GENERAL SETTINGS")]
    [OSCProperty(isInteractible = false)]
    public int pointsCount;
    [OSCProperty][Range(0, 50)]
    public int desiredPointsCount;
    [OSCProperty]
    [Range(0.0f, 10.0f)]
    public float speed;

    [Header("POINTS SIZE SETTINGS")]
    [OSCProperty]
    public Vector3 minPointSize;
    [OSCProperty][Tooltip("in meters")]
    public Vector3 maxPointSize;
    [OSCProperty]
    public bool animateSize;
    [OSCProperty][Range(0.0f, 10.0f)]
    public float sizeVariationSpeed;

    [Header("NOISY DATA SIMULATION")]
    [OSCProperty][Range(0.0f, 0.1f)]
    public float movementNoiseAmplitude;
    [OSCProperty][Range(0.0f, 20.0f)]
    public float movementNoiseFrequency;

    [OSCProperty][Range(0.0f, 1.0f)][Tooltip("False positives")]
    public float incorrectDetectionProbability = 0;
    [OSCProperty]
    public float incorrectDetectionDuration = 0.1f;

    [OSCProperty][Range(0.0f, 1.0f)][Tooltip("False negatives")]
    public float pointFlickeringProbability = 0;
    [OSCProperty]
    public float pointFlickeringDuration = 0.1f;

    [OSCMethod]
    public void RemoveAll()
    {
        ((PointManager)TargetScript).RemovePoints();
    }

    private GameObject pixelSizeInputField;
    private GameObject pixelSizeTooltip;

    public override void OnUiValueChanged(string name)
    {
        base.OnUiValueChanged(name);
        ((PointManager)TargetScript).protocolVersion = protocolVersion;

        UpdatePixelSizeDisplay();
    }

    public override void OnScriptValueChanged(string name) {
        base.OnScriptValueChanged(name);
        protocolVersion = ((PointManager)TargetScript).protocolVersion;
        protocolVersions = ((PointManager)TargetScript).protocolVersions;

        UpdatePixelSizeDisplay();
    }

    void InitializePixelSizeObjects() {

        //Get pixel size objects
        foreach (var text in FindObjectsOfType<Text>()) {
            if (text.text == "Pixel Size") {
                pixelSizeInputField = text.transform.parent.gameObject;
                break;
            }
        }

        foreach (var text in FindObjectsOfType<Text>()) {
            if (text.text == "in meters, to convert the area size to pixels.") {
                pixelSizeTooltip = text.gameObject;
                break;
            }
        }
    }

    void UpdatePixelSizeDisplay() {

        if (!pixelSizeInputField || !pixelSizeTooltip)
            InitializePixelSizeObjects();

        if (protocolVersion == "1") {
            pixelSizeInputField.SetActive(true);
            pixelSizeTooltip.SetActive(true);
        } else {
            pixelSizeInputField.SetActive(false);
            pixelSizeTooltip.SetActive(false);
        }
    }
}
