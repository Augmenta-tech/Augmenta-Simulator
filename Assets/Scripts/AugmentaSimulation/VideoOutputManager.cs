using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;

public class VideoOutputManager : MonoBehaviour
{
	public GameObject videoOutputObject;
	public Material videoOutputObjectMaterial;
	public PointManager pointManager;
	public Camera spoutCamera;
	public Klak.Spout.SpoutSender spoutSender;

	public enum AutoSizeType { None, SizeFromResolution, ResolutionFromSize }
	public AutoSizeType autoSizeType = AutoSizeType.ResolutionFromSize;

	public new bool enabled {
		get { return _enabled; }
		set { _enabled = value; EnableVideoOutput(_enabled); }
	}

	public Vector2 offset {
		get { return _offset; }
		set { _offset = value; UpdateVideoOutput(); }
	}

	public Vector2 size {
		get { return _size; }
		set { _size = value; UpdateVideoOutput(); }
	}

	public Vector2Int resolution {
		get { return _resolution; }
		set { _resolution = value; UpdateVideoOutput(); }
	}

	public bool spoutCameraOrthographic {
		get { return spoutCamera.orthographic; }
		set { spoutCamera.orthographic = value; UpdateCamera(); }
	}

	private RenderTexture _spoutRenderTexture;

	private bool _enabled = false;
	private Vector2 _offset = Vector2.zero;
	private Vector2 _size = Vector2.one;
	private Vector2Int _resolution = Vector2Int.one * 1200;

	private Vector3 _sceneSize;
	private Vector3 _videoOutputSize;
	private Vector3 _videoOutputOffset;

	private void OnEnable() {

		EnableVideoOutput(enabled);
	}

	private void Update() {

		if (!enabled)
			return;

		SendFusionMessage();
	}

	void EnableVideoOutput(bool enable) {

		videoOutputObject.SetActive(enable);
		spoutCamera.gameObject.SetActive(enable);
		UpdateVideoOutput();
	}

	void SendFusionMessage() {

		if (ProtocolVersionManager.protocolVersion == ProtocolVersionManager.AugmentaProtocolVersion.V1)
			return;

		OSCMessage fusionMessage = new OSCMessage("/fusion");

		fusionMessage.Append(offset.x);
		fusionMessage.Append(offset.y);
		fusionMessage.Append(size.x);
		fusionMessage.Append(size.y);
		fusionMessage.Append(resolution.x);
		fusionMessage.Append(resolution.y);

		OSCManager.activeManager.SendAugmentaMessage(fusionMessage);
		TUIOManager.activeManager.SendAugmentaMessage(fusionMessage, "Scene");

        WebsocketManager.activeManager.SendAugmentaMessage(
			"{\n\"fusion\": {\n\"textureOffset\": {\n\"x\": " + offset.x + ",\n\"y\": " + offset.y +
			"\n},\n\"textureBounds\": {\n\"x\": " + size.x + ",\n\"y\": " + size.y +
			"\n},\n\"targetOutSize\": {\n\"x\": " + resolution.x + ",\n\"y\": " + resolution.y + "\n}\n}\n}"
		);
	}

	public void UpdateVideoOutput() {

		//Update size or resolution
		if(autoSizeType == AutoSizeType.ResolutionFromSize) {

			_resolution = new Vector2Int(_resolution.x, (int)(_resolution.x * _size.y / _size.x));

		} else if(autoSizeType == AutoSizeType.SizeFromResolution) {

			_size = new Vector2(_size.x, _size.x * _resolution.y / _resolution.x);
		}

		UpdateOutputObject();

		if (!enabled)
			return;

		UpdateCamera();
		UpdateSpout();
	}

	void UpdateOutputObject() {

		//Update output object scale
		videoOutputObject.transform.localScale = new Vector3(size.x, size.y, 1);

		//Update output object position
		_sceneSize = new Vector3(pointManager.width, -pointManager.height, 0);
		_videoOutputSize = new Vector3(size.x, -size.y, 0);
		_videoOutputOffset = new Vector3(offset.x, -offset.y, -0.001f);

		videoOutputObject.transform.localPosition = (0.5f * (_videoOutputSize - _sceneSize) + _videoOutputOffset);

		//Update object texture scale 
		videoOutputObjectMaterial.mainTextureScale = videoOutputObject.transform.localScale * 0.5f;
	}

	void UpdateCamera() {

		//Update aspect
		spoutCamera.aspect = size.x / size.y;

		//Update FOV
		if (spoutCamera.orthographic) {
			spoutCamera.orthographicSize = size.y * 0.5f;
		} else {
			spoutCamera.fieldOfView = 2.0f * Mathf.Rad2Deg * Mathf.Atan2(size.y * 0.5f, 5);
		}
	}

	void UpdateSpout() {

		if (resolution.x == 0 || resolution.y == 0)
			return;

		if (_spoutRenderTexture)
			_spoutRenderTexture.Release();

		_spoutRenderTexture = new RenderTexture(resolution.x, resolution.y, 0, RenderTextureFormat.ARGB32);

		spoutCamera.targetTexture = _spoutRenderTexture;
		spoutSender.sourceTexture = _spoutRenderTexture;
	}
}
