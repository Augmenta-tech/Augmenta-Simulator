using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoOutputManagerControllable : Controllable
{
	[OSCProperty] public new bool enabled;
	[OSCProperty] public Vector2 offset;
	[OSCProperty] public Vector2 size;
	[OSCProperty] public Vector2Int resolution;
	[OSCProperty(TargetList = "controlTypes")]
	public string controlType;

	public List<string> controlTypes;

    private VideoOutputManager _videoOutputManager;

    public override void Awake() {

        _videoOutputManager = TargetScript as VideoOutputManager;

        controlType = _videoOutputManager.autoSizeType.ToString();
        controlTypes.Add(VideoOutputManager.AutoSizeType.None.ToString());
        controlTypes.Add(VideoOutputManager.AutoSizeType.ResolutionFromSize.ToString());
        controlTypes.Add(VideoOutputManager.AutoSizeType.SizeFromResolution.ToString());

        base.Awake();
    }

    public override void OnUiValueChanged(string name) {
        base.OnUiValueChanged(name);

        if ((VideoOutputManager.AutoSizeType)Enum.Parse(typeof(VideoOutputManager.AutoSizeType), controlType) != _videoOutputManager.autoSizeType) {
            _videoOutputManager.autoSizeType = (VideoOutputManager.AutoSizeType)Enum.Parse(typeof(VideoOutputManager.AutoSizeType), controlType);
            _videoOutputManager.UpdateVideoOutput();
        }

        OnScriptValueChanged(name);
    }

    public override void OnScriptValueChanged(string name) {
        base.OnScriptValueChanged(name);

        controlType = _videoOutputManager.autoSizeType.ToString();
    }

}
