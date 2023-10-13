using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

using UnityOSC;

public enum AugmentaMessageType
{
    SceneUpdated,
    AugmentaObjectEnter,
    AugmentaObjectUpdate,
    AugmentaObjectLeave,
    AugmentaObjectEnterExtra,
    AugmentaObjectUpdateExtra,
    AugmentaObjectLeaveExtra
}

public class PointManager : MonoBehaviour {

    [Header("Raycast Settings")]
    public new Camera camera;
    public LayerMask areaLayer;
    public LayerMask pointsLayer;

    [Header("Area settings")]
    private float _width = 5;
    public float width {
        get { return _width; }
        set { _width = value; UpdateAreaSize(); }
    }

    private float _height = 5;
    public float height {
        get { return _height; }
        set { _height = value; UpdateAreaSize(); }
    }

    public float pixelSize = 0.005f;

    [Header("Points settings")]
    public bool _mute;
    public bool mute {
        get { return _mute; }
        set { _mute = value;

            if (_mute) {
                backgroundMaterial.SetColor("_BorderColor", Color.gray);
            } else {
                backgroundMaterial.SetColor("_BorderColor", borderColor);
            }

            if (instantiatedPoints == null) return;

            foreach (var point in instantiatedPoints.Values)
                point.GetComponent<PointBehaviour>().MutePoint(_mute);

            foreach (var point in _incorrectInstantiatedPoints.Values)
                point.GetComponent<PointBehaviour>().MutePoint(_mute);

            foreach (var point in _flickeringPoints.Values)
                point.GetComponent<PointBehaviour>().MutePoint(_mute);
        }
    }

    public float outputMaxMPS = 0;

    public int pointsCount {
        get { return _pointsCount; }
    }
    private int _pointsCount;

    public int desiredPointsCount {
        get { return _desiredPointsCount; }
        set { _desiredPointsCount = value; 
            UpdateInstantiatedPointsCount();
        }
    }
    private int _desiredPointsCount;

    private float _speed = 1;
    public float speed {
        get { return _speed; }
        set { _speed = value;
            if (instantiatedPoints == null) return;

            foreach (var obj in instantiatedPoints)
                obj.Value.GetComponent<PointBehaviour>().speed = speed;

            foreach (var obj in _incorrectInstantiatedPoints)
                obj.Value.GetComponent<PointBehaviour>().speed = speed;

            foreach (var obj in _flickeringPoints)
                obj.Value.GetComponent<PointBehaviour>().speed = speed;
        }
    }

    //Points size

    private Vector3 _minPointSize = new Vector3(0.3f, 0.3f, 1.5f);
    public Vector3 minPointSize {
        get { return _minPointSize; }
        set {
            _minPointSize = value;
            UpdatePointsSize();
        }
    }

    private Vector3 _maxPointSize = new Vector3(0.7f, 0.7f, 2.0f);
    public Vector3 maxPointSize {
        get { return _maxPointSize; }
        set {
            _maxPointSize = value;
            UpdatePointsSize();
        }
    }

    private bool _animateSize = false;
    public bool animateSize {
        get { return _animateSize; }
        set { _animateSize = value;

            if (instantiatedPoints == null) return;

            foreach (var obj in instantiatedPoints)
                obj.Value.GetComponent<PointBehaviour>().animateSize = _animateSize;

            foreach (var obj in _incorrectInstantiatedPoints)
                obj.Value.GetComponent<PointBehaviour>().animateSize = _animateSize;

            foreach (var obj in _flickeringPoints)
                obj.Value.GetComponent<PointBehaviour>().animateSize = _animateSize;
        }
    }

    private float _sizeVariationSpeed = .2f;
    public float sizeVariationSpeed {
        get { return _sizeVariationSpeed; }
        set {
            _sizeVariationSpeed = value;

            if (instantiatedPoints == null) return;

            foreach (var obj in instantiatedPoints)
                obj.Value.GetComponent<PointBehaviour>().sizeVariationSpeed = _sizeVariationSpeed;

            foreach (var obj in _incorrectInstantiatedPoints)
                obj.Value.GetComponent<PointBehaviour>().sizeVariationSpeed = _sizeVariationSpeed;

            foreach (var obj in _flickeringPoints)
                obj.Value.GetComponent<PointBehaviour>().sizeVariationSpeed = _sizeVariationSpeed;
        }
    }

    //Noise Parameters

    private float _movementNoiseAmplitude = 0;
    public float movementNoiseAmplitude {
        get { return _movementNoiseAmplitude; }
        set {
            _movementNoiseAmplitude = value;
            if (instantiatedPoints == null) return;

            foreach (var obj in instantiatedPoints)
                obj.Value.GetComponent<PointBehaviour>().movementNoiseAmplitude = _movementNoiseAmplitude;

            foreach (var obj in _incorrectInstantiatedPoints)
                obj.Value.GetComponent<PointBehaviour>().movementNoiseAmplitude = _movementNoiseAmplitude;

            foreach (var obj in _flickeringPoints)
                obj.Value.GetComponent<PointBehaviour>().movementNoiseAmplitude = _movementNoiseAmplitude;
        }
    }

    private float _movementNoiseFrequency = 20;
    public float movementNoiseFrequency {
        get { return _movementNoiseFrequency; }
        set {
            _movementNoiseFrequency = value;
            if (instantiatedPoints == null) return;

            foreach (var obj in instantiatedPoints)
                obj.Value.GetComponent<PointBehaviour>().movementNoiseFrequency = _movementNoiseFrequency;

            foreach (var obj in _incorrectInstantiatedPoints)
                obj.Value.GetComponent<PointBehaviour>().movementNoiseFrequency = _movementNoiseFrequency;

            foreach (var obj in _flickeringPoints)
                obj.Value.GetComponent<PointBehaviour>().movementNoiseFrequency = _movementNoiseFrequency;
        }
    }

    private float _rotationNoiseAmplitude = 0;
    public float rotationNoiseAmplitude {
        get { return _rotationNoiseAmplitude; }
        set {
            _rotationNoiseAmplitude = value;
            if (instantiatedPoints == null) return;

            foreach (var obj in instantiatedPoints)
                obj.Value.GetComponent<PointBehaviour>().rotationNoiseAmplitude = _rotationNoiseAmplitude;

            foreach (var obj in _incorrectInstantiatedPoints)
                obj.Value.GetComponent<PointBehaviour>().rotationNoiseAmplitude = _rotationNoiseAmplitude;

            foreach (var obj in _flickeringPoints)
                obj.Value.GetComponent<PointBehaviour>().rotationNoiseAmplitude = _rotationNoiseAmplitude;
        }
    }

    private float _rotationNoiseFrequency = 20;
    public float rotationNoiseFrequency {
        get { return _rotationNoiseFrequency; }
        set {
            _rotationNoiseFrequency = value;
            if (instantiatedPoints == null) return;

            foreach (var obj in instantiatedPoints)
                obj.Value.GetComponent<PointBehaviour>().rotationNoiseFrequency = _rotationNoiseFrequency;

            foreach (var obj in _incorrectInstantiatedPoints)
                obj.Value.GetComponent<PointBehaviour>().rotationNoiseFrequency = _rotationNoiseFrequency;

            foreach (var obj in _flickeringPoints)
                obj.Value.GetComponent<PointBehaviour>().rotationNoiseFrequency = _rotationNoiseFrequency;
        }
    }

    public float incorrectDetectionProbability = 0;
    public float incorrectDetectionDuration = 0.1f;

    public float pointFlickeringProbability = 0;
    public float pointFlickeringDuration = 0.1f;

    public GameObject pointPrefab;

    public static Dictionary<int, GameObject> instantiatedPoints;
    private List<int> slots = new List<int>();

    public Material backgroundMaterial;
    public Color borderColor;

    private CameraController cameraController;

    private int _highestId;

    private GameObject _cursorPoint;
    private Ray _ray;
    private RaycastHit _raycastHit;

    private Dictionary<int, GameObject> _incorrectInstantiatedPoints;
    private Dictionary<int, GameObject> _flickeringPoints;

    private bool _initialized = false;
    private float _timer = 0;

    #region MonoBehaviour Implementation

    void Start () {

        if (!_initialized) {
            Initialize();
        }

        UpdateAreaSize();
    }

    void Update() {

        //Check keyboard inputs
        ProcessKeyboardInputs();

        //_frameCounter++;

        //Process mouse
        ProcessMouseInputs();

        //Create incorrect detections
        CreateIncorrectDetection();

        //Create flickering
        CreateFlickeringPoints();

        //Send Augmenta data at desired rate
        if (outputMaxMPS > 0) {
            _timer += Time.deltaTime;

            if (_timer >= 1.0f / outputMaxMPS) {

                SendAugmentaUpdateMessages();
                _timer = 0.0f;
            }
        } else {
            SendAugmentaUpdateMessages();
        }
    }

    public void OnMouseDrag() {
        if (_cursorPoint == null) return;

        _ray = camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(_ray, out _raycastHit, Mathf.Infinity, areaLayer)) {

            _cursorPoint.transform.position = new Vector3(_raycastHit.point.x, 0, _raycastHit.point.z);
        }
    }

	#endregion

    void Initialize() {
        System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

        instantiatedPoints = new Dictionary<int, GameObject>();
        _incorrectInstantiatedPoints = new Dictionary<int, GameObject>();
        _flickeringPoints = new Dictionary<int, GameObject>();

        _highestId = 0;

        _initialized = true;
    }

    void SendAugmentaUpdateMessages() {

        //Send Augmenta scene message
        SendAugmentaMessage(AugmentaMessageType.SceneUpdated);

        // SendStartingMessage()
        TUIOManager.activeManager.SendAugmentaMessage(CreateAugmentaMessageAliveTUIO(), "TUIO");

        // Send Augmenta persons update messages
        foreach (var point in instantiatedPoints)
            SendAugmentaMessage(AugmentaMessageType.AugmentaObjectUpdate, point.Value);

        foreach (var point in _incorrectInstantiatedPoints)
            SendAugmentaMessage(AugmentaMessageType.AugmentaObjectUpdate, point.Value);

        // SendEndingMessage()
        TUIOManager.activeManager.SendAugmentaMessage(CreateAugmentaMessageFseqTUIO(), "TUIO");
    }

	#region Inputs Handling

	/// <summary>
	/// Process keyboard key presses
	/// </summary>
	public void ProcessKeyboardInputs()
    {
        if (Input.GetKeyUp(KeyCode.M))
            mute = !mute;
    }

    /// <summary>
    /// Process mouse button presses
    /// </summary>
    public void ProcessMouseInputs() {

        //Left clic
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()
            && !Input.GetKey(KeyCode.LeftAlt)) {
            _ray = camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(_ray, out _raycastHit, Mathf.Infinity, pointsLayer)) {

                //Point hit
                _cursorPoint = _raycastHit.transform.gameObject;

            } else if(Physics.Raycast(_ray, out _raycastHit, Mathf.Infinity, areaLayer)) {

                //Area hit
                InstantiatePoint(false, true);
            }
        }

        //Left release
        if (Input.GetMouseButtonUp(0)) {
            if (!_cursorPoint)
                return;

            _cursorPoint = null;
        }

        //Right clic
        if (Input.GetMouseButtonDown(1) && !EventSystem.current.IsPointerOverGameObject()) {
            _ray = camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(_ray, out _raycastHit, Mathf.Infinity, pointsLayer)) {

                RemovePoint(_raycastHit.transform.GetComponent<PointBehaviour>().id);
                _desiredPointsCount--;
            }
        }

    }

	#endregion

	#region Points Handling

    /// <summary>
    /// Update points size to match min/max size criteria
    /// </summary>
    void UpdatePointsSize() {

        if (instantiatedPoints == null) return;

        PointBehaviour tmpBehaviour;

        foreach (var obj in instantiatedPoints) {
            tmpBehaviour = obj.Value.GetComponent<PointBehaviour>();
            tmpBehaviour.size = Vector3.Min(tmpBehaviour.size, maxPointSize);
            tmpBehaviour.size = Vector3.Max(tmpBehaviour.size, minPointSize);
        }

        foreach (var obj in _incorrectInstantiatedPoints) {
            tmpBehaviour = obj.Value.GetComponent<PointBehaviour>();
            tmpBehaviour.size = Vector3.Min(tmpBehaviour.size, maxPointSize);
            tmpBehaviour.size = Vector3.Max(tmpBehaviour.size, minPointSize);
        }

        foreach (var obj in _flickeringPoints) {
            tmpBehaviour = obj.Value.GetComponent<PointBehaviour>();
            tmpBehaviour.size = Vector3.Min(tmpBehaviour.size, maxPointSize);
            tmpBehaviour.size = Vector3.Max(tmpBehaviour.size, minPointSize);
        }

    }

    /// <summary>
    /// Instantiate incorrect detection points
    /// </summary>
    private void CreateIncorrectDetection() {

        if(UnityEngine.Random.Range(0.0f, 1.0f) <= incorrectDetectionProbability) {
            InstantiatePoint(true);
        }
    }

    /// <summary>
    /// Create point flickering
    /// </summary>
    private void CreateFlickeringPoints() {

        if (instantiatedPoints.Count == 0)
            return;

        if (UnityEngine.Random.Range(0.0f, 1.0f) <= pointFlickeringProbability) {

            int flickeringIndex = UnityEngine.Random.Range(0, instantiatedPoints.Count);

            try {
                int pidToFlicker = instantiatedPoints.ElementAt(flickeringIndex).Key;

                _flickeringPoints.Add(pidToFlicker, instantiatedPoints.ElementAt(flickeringIndex).Value);

                SendAugmentaMessage(AugmentaMessageType.AugmentaObjectLeave, instantiatedPoints[pidToFlicker]);
                instantiatedPoints.ElementAt(flickeringIndex).Value.GetComponent<PointBehaviour>().StartFlickering();
                instantiatedPoints.Remove(pidToFlicker);
                _pointsCount--;

            } catch {

            }
        }
    }

    /// <summary>
    /// Recreate flickering point
    /// </summary>
    /// <param name="pid"></param>
    public void StopFlickering(int pid) {

        instantiatedPoints.Add(pid, _flickeringPoints[pid]);
        _pointsCount++;

        var pointBehaviour = _flickeringPoints[pid].GetComponent<PointBehaviour>();
        pointBehaviour.isFlickering = false;
        pointBehaviour.ShowPoint();

        _flickeringPoints.Remove(pid);
        SendAugmentaMessage(AugmentaMessageType.AugmentaObjectEnter, instantiatedPoints[pid]);
    }

    /// <summary>
    /// Create or remove instantiated points according to the desired point count
    /// </summary>
    public void UpdateInstantiatedPointsCount() {

        if (!_initialized) {
            Initialize();
        }

        if (_desiredPointsCount <= 0) _desiredPointsCount = 0;

        //Create new points
        while (instantiatedPoints.Count < _desiredPointsCount) {
            InstantiatePoint();
        }

        //Remove points
        while (instantiatedPoints.Count > _desiredPointsCount) {
            RemovePoint();
        }
    }

	/// <summary>
	/// Instantiate new point
	/// </summary>
	public void InstantiatePoint(bool isIncorrectDetection = false, bool isFromCursor = false) {

		GameObject newPoint = Instantiate(pointPrefab);

		PointBehaviour newPointBehaviour = newPoint.GetComponent<PointBehaviour>();

		newPointBehaviour.pointColor = Color.HSVToRGB(UnityEngine.Random.value, 0.85f, 0.75f);
		newPointBehaviour.manager = this;
        newPointBehaviour.UpdatePointColor(mute ? Color.gray : newPointBehaviour.pointColor);
		newPoint.transform.parent = transform;
		newPoint.transform.localPosition = GetNewPointPosition();
		newPointBehaviour.speed = speed;
		newPointBehaviour.id = _highestId;
        newPointBehaviour.slotid = registerObject(newPoint);
        newPointBehaviour.size = new Vector3(UnityEngine.Random.Range(minPointSize.x, maxPointSize.x),
                                             UnityEngine.Random.Range(minPointSize.y, maxPointSize.y),
                                             UnityEngine.Random.Range(minPointSize.z, maxPointSize.z));
		newPointBehaviour.animateSize = animateSize;
		newPointBehaviour.sizeVariationSpeed = sizeVariationSpeed;
		newPointBehaviour.movementNoiseAmplitude = movementNoiseAmplitude;
		newPointBehaviour.movementNoiseFrequency = movementNoiseFrequency;
        newPointBehaviour.rotationNoiseAmplitude = rotationNoiseAmplitude;
        newPointBehaviour.rotationNoiseFrequency = rotationNoiseFrequency;
        newPointBehaviour.isIncorrectDetection = isIncorrectDetection;
		newPointBehaviour.isFlickering = false;

        if (isFromCursor)
        {
            _cursorPoint = newPoint;
            OnMouseDrag();
            _desiredPointsCount++;
        }

        if (isIncorrectDetection) {
			_incorrectInstantiatedPoints.Add(_highestId, newPoint);
			SendAugmentaMessage(AugmentaMessageType.AugmentaObjectEnter, _incorrectInstantiatedPoints[_highestId]);
		} else {
			instantiatedPoints.Add(_highestId, newPoint);
			SendAugmentaMessage(AugmentaMessageType.AugmentaObjectEnter, instantiatedPoints[_highestId]);
		}

        _highestId++;
		_pointsCount++;
	}

	/// <summary>
	/// Return a local position for a new point
	/// </summary>
	/// <returns></returns>
	Vector3 GetNewPointPosition() {

        float x = UnityEngine.Random.Range(0.0f, 1.0f);
        float y = UnityEngine.Random.Range(0.0f, 1.0f);
        
        if(x >= 0.25f && x < 0.75f) {
            //X is in the center, Y must be on the border
            if (y > 0.25f && y <= 0.5f) y -= 0.25f;
            if (y > 0.5f && y <= 0.75f) y += 0.25f;
        }

        return new Vector3(x - 0.5f, y - 0.5f, 0);
    }

    /// <summary>
    /// Remove point with highest pid
    /// </summary>
    public void RemovePoint() {

        if (instantiatedPoints.Count == 0)
            return;

        int pidToRemove = instantiatedPoints.ElementAt(instantiatedPoints.Count - 1).Key;

        //Do not remove the cursor point unless it is the last one
        if(pidToRemove == 0 && instantiatedPoints.Count > 1)
            pidToRemove = instantiatedPoints.ElementAt(instantiatedPoints.Count - 2).Key;

		SendAugmentaMessage(AugmentaMessageType.AugmentaObjectLeave, instantiatedPoints[pidToRemove]);
        unregisterObject(instantiatedPoints[pidToRemove]);
		Destroy(instantiatedPoints[pidToRemove]);
		instantiatedPoints.Remove(pidToRemove);

        _pointsCount--;
    }

    /// <summary>
    /// Remove point with given pid
    /// </summary>
    public void RemovePoint(int pid) {

        if (instantiatedPoints.ContainsKey(pid)) {
            SendAugmentaMessage(AugmentaMessageType.AugmentaObjectLeave, instantiatedPoints[pid]);
            unregisterObject(instantiatedPoints[pid]);
            Destroy(instantiatedPoints[pid]);
            instantiatedPoints.Remove(pid);

            _pointsCount--;

        } else {
            RemoveIncorrectPoint(pid);
        }
    }

    /// <summary>
    /// Remove incorrect point with given pid
    /// </summary>
    public void RemoveIncorrectPoint(int pid) {

        if (_incorrectInstantiatedPoints.ContainsKey(pid)) {
            SendAugmentaMessage(AugmentaMessageType.AugmentaObjectLeave, _incorrectInstantiatedPoints[pid]);
            unregisterObject(_incorrectInstantiatedPoints[pid]);
            Destroy(_incorrectInstantiatedPoints[pid]);
            _incorrectInstantiatedPoints.Remove(pid);

            _pointsCount--;
        }
    }

    /// <summary>
    /// Compute points SLOTIDs
    /// </summary> 
    private int getFirstFreeId() {

        int firstFree = slots.IndexOf(-1);
        return firstFree;
	}

    private int registerObject(GameObject obj){
        var behaviour = obj.GetComponent<PointBehaviour>();
        int freeId = getFirstFreeId();

        if(freeId == -1)
        {
            freeId = slots.Count;
            slots.Add(behaviour.id);
        }

        else slots[freeId] = behaviour.id;

        return freeId;
    }

    private void unregisterObject(GameObject obj)
    {
        var behaviour = obj.GetComponent<PointBehaviour>();
        int indexToRemove = behaviour.slotid;
        slots[indexToRemove] = -1;
    }

    public void RemovePoints() {
        _pointsCount = 0;
        _highestId = 0;

        foreach (var obj in instantiatedPoints) {
            SendAugmentaMessage(AugmentaMessageType.AugmentaObjectLeave, obj.Value);
            Destroy(obj.Value);
        }

        instantiatedPoints.Clear();
        slots.Clear();
    }

	#endregion

	#region Area Handling

    private void UpdateAreaSize() {

        if (_width <= 0) _width = 1.0f;
        if (_height <= 0) _height = 1.0f;

        transform.localScale = new Vector3(_width, _height, 1);

        UpdateBackgroundTexture();
        UpdateCamera();

        //Update points local scale
        if (instantiatedPoints == null) return;

        foreach (var obj in instantiatedPoints)
            obj.Value.transform.localScale = new Vector3(1.0f / _width, 1.0f / _height, 1);

        foreach (var obj in _incorrectInstantiatedPoints)
            obj.Value.transform.localScale = new Vector3(1.0f / _width, 1.0f / _height, 1);

        foreach (var obj in _flickeringPoints)
            obj.Value.transform.localScale = new Vector3(1.0f / _width, 1.0f / _height, 1);
    }

	private void UpdateBackgroundTexture()
    {
        backgroundMaterial.mainTextureScale = transform.localScale * 0.5f;
    }

    void UpdateCamera() {

        if (!cameraController)
            cameraController = FindObjectOfType<CameraController>();

        cameraController.SetDistanceFromAreaSize();
    }

    #endregion

    #region OSC Message

    /// <summary>
    /// Send an Augmenta OSC message 
    /// </summary>
    /// <param name="messageType"></param>
    /// <param name="obj"></param>
    public void SendAugmentaMessage(AugmentaMessageType messageType, GameObject obj = null) {

        if (mute) return;

        // Craft and send a message that contains the event + extra info
        WebsocketManager.activeManager.SendAugmentaMessage(CreateAugmentaMessageJSON(messageType,obj));

        if (messageType == AugmentaMessageType.SceneUpdated) { TUIOManager.activeManager.SendAugmentaMessage(CreateAugmentaMessageSetTUIO(messageType, obj),"Scene"); }
        else { TUIOManager.activeManager.SendAugmentaMessage(CreateAugmentaMessageSetTUIO(messageType, obj), "TUIO"); }
        

        switch (ProtocolVersionManager.protocolVersion) {
            case ProtocolVersionManager.AugmentaProtocolVersion.V1:
                OSCManager.activeManager.SendAugmentaMessage(CreateAugmentaMessageV1(messageType, obj));
                break;

            case ProtocolVersionManager.AugmentaProtocolVersion.V2:
                OSCManager.activeManager.SendAugmentaMessage(CreateAugmentaMessageV2(messageType, obj));
                if(messageType != AugmentaMessageType.SceneUpdated)
                    //Send corresponding /extra message
                    OSCManager.activeManager.SendAugmentaMessage(CreateAugmentaMessageV2((AugmentaMessageType)Enum.Parse(typeof(AugmentaMessageType), messageType.ToString()+"Extra"), obj));
                break;
        }
    }

    #region Protocol V1
    //OSC Protocol V1

    /*
        0: pid (int)                        // Personal ID ex : 42th person to enter stage has pid=42
        1: slotid (int)                        // Ordered ID ex : if 3 person on stage, 43th person might have slotid=2
        2: age (int)                        // Time on stage (in frame number)
        3: centroid.x (float 0:1)           // Position projected to the ground
        4: centroid.y (float 0:1)               
        5: velocity.x (float -1:1)           // Speed and direction vector
        6: velocity.y (float -1:1)
        7: depth (float 0:1)                // Distance to sensor (in m) (not implemented)
        8: boundingRect.x (float 0:1)       // Top view bounding box
        9: boundingRect.y (float 0:1)
        10: boundingRect.width (float 0:1)
        11: boundingRect.height (float 0:1)
        12: highest.x (float 0:1)           // Highest point placement
        13: highest.y (float 0:1)
        14: highest.z (float 0:1)           // Height of the person

        /au/scene   args0 arg1 ... argn

        0: currentTime (int)                // Time (in frame number)
        1: percentCovered (float 0:1)       // Percent covered
        2: numPeople (int)                  // Number of person
        3: averageMotion.x (float 0:1)          // Average motion
        4: averageMotion.y (float 0:1)
        5: scene.width (int)                // Scene size
        6: scene.height (int)
        7: scene.depth (int)
    */

    /// <summary>
    /// Create an Augmenta message with protocol V1.
    /// </summary>
    /// <param name="messageType"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    private OSCMessage CreateAugmentaMessageV1(AugmentaMessageType messageType, GameObject obj = null) {

        switch (messageType) {
            case AugmentaMessageType.AugmentaObjectEnter:
                return CreateAugmentaObjectMessageV1("/au/personEntered", obj);

            case AugmentaMessageType.AugmentaObjectUpdate:
                return CreateAugmentaObjectMessageV1("/au/personUpdated", obj);

            case AugmentaMessageType.AugmentaObjectLeave:
                return CreateAugmentaObjectMessageV1("/au/personWillLeave", obj);

            case AugmentaMessageType.SceneUpdated:
                return CreateSceneMessageV1("/au/scene");

            default:
                return null;
        }
    }

    /// <summary>
    /// Create an AugmentaObject message with protocol V1.
    /// </summary>
    /// <param name="address"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    private OSCMessage CreateAugmentaObjectMessageV1(string address, GameObject obj) {

        var msg = new OSCMessage(address);
        var behaviour = obj.GetComponent<PointBehaviour>();

        float pointX = 0.5f + behaviour.transform.position.x / width;
        float pointY = 0.5f - behaviour.transform.position.z / height;

        msg.Append(behaviour.id);
        msg.Append(behaviour.slotid);
        msg.Append((int)behaviour.ageInFrames);
        msg.Append(pointX);
        msg.Append(pointY);
        msg.Append(-behaviour.normalizedVelocity.x);
        msg.Append(-behaviour.normalizedVelocity.z);
        msg.Append(0.0f);
        msg.Append(pointX - behaviour.size.x * 0.5f / width);
        msg.Append(pointY - behaviour.size.y * 0.5f / height);
        msg.Append(behaviour.size.x / width);
        msg.Append(behaviour.size.y / height);
        msg.Append(pointX);
        msg.Append(pointY);
        msg.Append(behaviour.size.z);

        return msg;
    }

    private OSCMessage CreateSceneMessageV1(string address) {

        var msg = new OSCMessage(address);

        msg.Append(Time.frameCount);
        //Compute point size
        msg.Append(instantiatedPoints.Count * 0.25f * (maxPointSize.x + minPointSize.x) * (maxPointSize.y + minPointSize.y));
        msg.Append(_pointsCount);
        //Compute average motion
        var velocitySum = Vector3.zero;
        foreach (var element in instantiatedPoints) {
            velocitySum += -element.Value.GetComponent<PointBehaviour>().normalizedVelocity;
        }

        if (instantiatedPoints.Count > 0)
            velocitySum /= instantiatedPoints.Count;

        msg.Append(velocitySum.x);
        msg.Append(velocitySum.z);
        msg.Append(Mathf.RoundToInt(width / pixelSize));
        msg.Append(Mathf.RoundToInt(height / pixelSize));
        msg.Append(100);

        return msg;
    }

    #endregion

    #region Protocol V2
    /* Augmenta OSC Protocol v2.0

        /object/enter arg0 arg1 ... argN
        /object/leave arg0 arg1 ... argN
        /object/update arg0 arg1 ... argN

        where args are : 
        0: frame(int)     // Frame number
        1: id(int)                        // id ex : 42th object to enter stage has pid=42
        2: slotid(int)                        // Ordered id ex : if 3 objects on stage, 43th object might have slotid=2 
        3: age(float)                      // Alive time (in s)
        4: centroid.x(float 0:1)           // Position projected to the ground (normalised)
        5: centroid.y(float 0:1)               
        6: velocity.x(float -1:1)           // Speed and direction vector (in unit.s-1) (normalised)
        7: velocity.y(float -1:1)
        8: orientation(float 0:360) // With respect to horizontal axis right (0° = (1,0)), rotate counterclockwise
				                    // Estimation of the object orientation from its rotation and velocity
        9: boundingRect.x(float 0:1)       // Bounding box center coord (normalised)	
        10: boundingRect.y(float 0:1)       
        11: boundingRect.width(float 0:1) // Bounding box width (normalised)
        12: boundingRect.height(float 0:1)
        13: boundingRect.rotation(float 0:360) // With respect to horizontal axis right counterclockwise
        14: height(float)           // Height of the object (in m) (absolute)

        /scene   arg0 arg1 ... argN
        0: frame (int)                // Frame number
        1: objectCount (int)                  // Number of objects
        2: scene.width (float)             // Scene width in 
        3: scene.height (float)
       
        /fusion arg0 arg1 ... argN

        0: videoOut.PixelWidth (int)      // VideoOut width in fusion
        1: videoOut.PixelHeight (int)
        2: videoOut.coord.x (int)          // top left coord in fusion
        3: videoOut.coord.y (int)
        4: scene.coord.x (float)          // Scene top left coord (0 for node by default)
        5: scene.coord.y (float)

    */

    /// <summary>
    /// Create an Augmenta message with protocol V2.
    /// </summary>
    /// <param name="messageType"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    private OSCMessage CreateAugmentaMessageV2(AugmentaMessageType messageType, GameObject obj = null) {

        switch (messageType) {
            case AugmentaMessageType.AugmentaObjectEnter:
                return CreateAugmentaObjectMessageV2("/object/enter", obj);

            case AugmentaMessageType.AugmentaObjectUpdate:
                return CreateAugmentaObjectMessageV2("/object/update", obj);

            case AugmentaMessageType.AugmentaObjectLeave:
                return CreateAugmentaObjectMessageV2("/object/leave", obj);

            case AugmentaMessageType.AugmentaObjectEnterExtra:
                return CreateAugmentaObjectExtraMessageV2("/object/enter/extra", obj);

            case AugmentaMessageType.AugmentaObjectUpdateExtra:
                return CreateAugmentaObjectExtraMessageV2("/object/update/extra", obj);

            case AugmentaMessageType.AugmentaObjectLeaveExtra:
                return CreateAugmentaObjectExtraMessageV2("/object/leave/extra", obj);

            case AugmentaMessageType.SceneUpdated:
                return CreateSceneMessageV2("/scene");

            default:
                return null;
        }
    }

    /// <summary>
    /// Create an AugmentaObject message with protocol V2.
    /// </summary>
    /// <param name="address"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    private OSCMessage CreateAugmentaObjectMessageV2(string address, GameObject obj) {

        var msg = new OSCMessage(address);
        var behaviour = obj.GetComponent<PointBehaviour>();

        float pointX = 0.5f + behaviour.transform.position.x / width;
        float pointY = 0.5f - behaviour.transform.position.z / height;

        float rotation = behaviour.point.transform.localRotation.eulerAngles.z >= 0 ? behaviour.point.transform.localRotation.eulerAngles.z : behaviour.point.transform.localRotation.eulerAngles.z + 360.0f ;

        msg.Append(Time.frameCount);                      // Frame number
        msg.Append(behaviour.id);                       // id ex : 42th object to enter stage has id=42
        msg.Append(behaviour.slotid);                      // Ordered id ex : if 3 objects on stage, 43th object might have slotid=2 
        msg.Append(behaviour.ageInSeconds);             // Alive time (in s)
        msg.Append(pointX);                             // Position projected to the ground (normalized)
        msg.Append(pointY);
        msg.Append(-behaviour.normalizedVelocity.x);    // Speed and direction vector (in unit.s-1) (normalized)
        msg.Append(-behaviour.normalizedVelocity.z);
        msg.Append(behaviour.orientation);              // Orientation with respect to horizontal axis right (0° = (1,0)), rotate counterclockwise. Estimation of the object orientation from its rotation and velocity
        msg.Append(pointX);                             // Bounding box center coord (normalized)
        msg.Append(pointY); 
        msg.Append(behaviour.size.x / width);           // Bounding box width (normalized)
        msg.Append(behaviour.size.y / height);          
        msg.Append(rotation);                           // With respect to horizontal axis right (0° = (1,0)), rotate counterclockwise
        msg.Append(behaviour.size.z);                   // Height of the object (in m) (absolute)

        return msg;
    }

    /// <summary>
    /// Create an AugmentaObjectExtra message with protocol V2.
    /// </summary>
    /// <param name="address"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    private OSCMessage CreateAugmentaObjectExtraMessageV2(string address, GameObject obj) {

        var msg = new OSCMessage(address);
        var behaviour = obj.GetComponent<PointBehaviour>();

        float pointX = 0.5f + behaviour.transform.position.x / width;
        float pointY = 0.5f - behaviour.transform.position.z / height;

        msg.Append(Time.frameCount);                      // Frame number
        msg.Append(behaviour.id);                       // id ex : 42th object to enter stage has id=42
        msg.Append(behaviour.slotid);                      // Ordered id ex : if 3 objects on stage, 43th object might have slotid=2 
        msg.Append(pointX);                             // Highest point placement (normalized)
        msg.Append(pointY);
        msg.Append(Vector3.Distance(camera.transform.position, behaviour.transform.position)); //Sensor distance in meters
        msg.Append(behaviour.pointColor.grayscale);     // Reflectivity value from lidar sensor

        return msg;
    }

    /// <summary>
    /// Create an Augmenta scene message with protocol V2.
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    private OSCMessage CreateSceneMessageV2(string address) {

        var msg = new OSCMessage(address);

        msg.Append(Time.frameCount);  // Frame number
        msg.Append(_pointsCount);   // Objects count
        msg.Append(width);          // Scene width
        msg.Append(height);         //Scene height

        return msg;
    }

    #endregion

    #endregion

    /// <summary>
    /// Clamp angle between 0 and 360 degrees
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    private float ClampAngle(float angle) {

        while (angle < 0)
            angle += 360.0f;

        return angle % 360.0f;
    }

    #region TUIO

    private OSCMessage CreateAugmentaMessageAliveTUIO()
    {
        string addr = TUIOManager.activeManager.GetAddressTUIO(TUIOManager.activeManager.TUIODescriptor, TUIOManager.activeManager.TUIODimension);
        var msg = new OSCMessage(addr);

        msg.Append("alive");

        foreach (var point in instantiatedPoints)
        {
            var behaviour = point.Value.GetComponent<PointBehaviour>();
            msg.Append(behaviour.id);
        }

        foreach (var point in _incorrectInstantiatedPoints)
        {
            var behaviour = point.Value.GetComponent<PointBehaviour>();
            msg.Append(behaviour.id);
        }
        
        return msg;
    }

    private OSCMessage CreateAugmentaMessageSetTUIO(AugmentaMessageType messageType, GameObject obj = null)
    {
        string addr = TUIOManager.activeManager.GetAddressTUIO(TUIOManager.activeManager.TUIODescriptor, TUIOManager.activeManager.TUIODimension);
        
        switch (messageType)
        {
            case AugmentaMessageType.AugmentaObjectUpdate:
                return CreateAugmentaMessageObjectTUIO(addr, obj); //renvoyer le message 

            case AugmentaMessageType.SceneUpdated:
                return CreateAugmentaMessageTUIOScene();

            default:
                var msg = new OSCMessage(addr);
                return msg;
        }
    }

    private OSCMessage CreateAugmentaMessageFseqTUIO()
    {
        string addr = TUIOManager.activeManager.GetAddressTUIO(TUIOManager.activeManager.TUIODescriptor, TUIOManager.activeManager.TUIODimension);
        var msg = new OSCMessage(addr);

        msg.Append("fseq");
        msg.Append(Time.frameCount);

        return msg;
    }

    private OSCMessage CreateAugmentaMessageObjectTUIO(string address, GameObject obj)
    {
        var msg = new OSCMessage(address);
        msg.Append("set");

        var behaviour = obj.GetComponent<PointBehaviour>();
        float pointX = 0.5f + behaviour.transform.position.x / width;
        float pointY = 0.5f - behaviour.transform.position.z / height;
        float rotation = behaviour.point.transform.localRotation.eulerAngles.z >= 0 ? behaviour.point.transform.localRotation.eulerAngles.z : behaviour.point.transform.localRotation.eulerAngles.z + 360.0f;

        //float boundingRotation = behaviour.boundingRotation - orientationParam->floatValue();
        float a = (behaviour.orientation / 180) * Mathf.PI; //
        float bba = (rotation / 180) * Mathf.PI; // bb orientation in rad
        float z = behaviour.transform.position.y / TUIOManager.activeManager.sceneDepth; // centroid / sceneDepth -> centroid == behaviour.size.z/2 here

        switch (TUIOManager.activeManager.TUIODescriptor)
        {
            case TUIOManager.AugmentaTUIODescriptor.OBJECT:
                switch (TUIOManager.activeManager.TUIODimension)
                {
                    case TUIOManager.AugmentaTUIODimension.TUIO2D:
                        msg.Append(behaviour.id); // s
                        msg.Append(behaviour.slotid); // i (class id)
                        msg.Append(pointX); // x
                        msg.Append(pointY); // y
                        msg.Append(a); // a (angle) 
                        msg.Append(-behaviour.normalizedVelocity.x); // X
                        msg.Append(-behaviour.normalizedVelocity.z); // Y
                        msg.Append(0.0f); // A
                        msg.Append(0.0f); // m
                        msg.Append(0.0f); // r (rotation acceleration)
                        break;
                    case TUIOManager.AugmentaTUIODimension.TUIO25D:
                        msg.Append(behaviour.id); // s
                        msg.Append(behaviour.slotid); // i (class id)
                        msg.Append(pointX); // x
                        msg.Append(pointY); // y
                        msg.Append(z); // z
                        msg.Append(a); // a
                        msg.Append(-behaviour.normalizedVelocity.x); // X
                        msg.Append(-behaviour.normalizedVelocity.z); // Y
                        msg.Append(-behaviour.normalizedVelocity.y / TUIOManager.activeManager.sceneDepth); // Z
                        msg.Append(0.0f); // A
                        msg.Append(0.0f); // m
                        msg.Append(0.0f); // r
                        break;
                    case TUIOManager.AugmentaTUIODimension.TUIO3D:
                        msg.Append(behaviour.id); // s
                        msg.Append(behaviour.slotid); // i (class id)
                        msg.Append(pointX); // x
                        msg.Append(pointY); // y
                        msg.Append(z); // z
                        msg.Append(a); // a
                        msg.Append(0.0f); // b
                        msg.Append(0.0f); // c
                        msg.Append(-behaviour.normalizedVelocity.x); // X
                        msg.Append(-behaviour.normalizedVelocity.z); // Y
                        msg.Append(-behaviour.normalizedVelocity.y / TUIOManager.activeManager.sceneDepth); // Z
                        msg.Append(0.0f); // A
                        msg.Append(0.0f); // B
                        msg.Append(0.0f); // C
                        msg.Append(0.0f); // m
                        msg.Append(0.0f); // r
                        break;
                }

                break;

            case TUIOManager.AugmentaTUIODescriptor.CURSOR:
                switch (TUIOManager.activeManager.TUIODimension)
                {
                    case TUIOManager.AugmentaTUIODimension.TUIO2D:
                        msg.Append(behaviour.id); // s
                        msg.Append(pointX); // x
                        msg.Append(pointY); // y
                        msg.Append(-behaviour.normalizedVelocity.x); // X
                        msg.Append(-behaviour.normalizedVelocity.z); // Y
                        msg.Append(0.0f); // m
                        break;
                    case TUIOManager.AugmentaTUIODimension.TUIO25D:
                        msg.Append(behaviour.id); // s
                        msg.Append(pointX); // x
                        msg.Append(pointY); // y
                        msg.Append(z); // z
                        msg.Append(-behaviour.normalizedVelocity.x); // X
                        msg.Append(-behaviour.normalizedVelocity.z); // Y
                        msg.Append(-behaviour.normalizedVelocity.y / TUIOManager.activeManager.sceneDepth); // Z
                        msg.Append(0.0f); // m
                        break;
                    case TUIOManager.AugmentaTUIODimension.TUIO3D:
                        msg.Append(behaviour.id); // s
                        msg.Append(pointX); // x
                        msg.Append(pointY); // y
                        msg.Append(z); // z
                        msg.Append(-behaviour.normalizedVelocity.x); // X
                        msg.Append(-behaviour.normalizedVelocity.z); // Y
                        msg.Append(-behaviour.normalizedVelocity.y / TUIOManager.activeManager.sceneDepth); // Z
                        msg.Append(0.0f); // m
                        break;
                }

                break;

            case TUIOManager.AugmentaTUIODescriptor.BLOB:
                switch (TUIOManager.activeManager.TUIODimension)
                {
                    case TUIOManager.AugmentaTUIODimension.TUIO2D:
                        msg.Append(behaviour.id); // s
                        msg.Append(pointX); // x
                        msg.Append(pointY); // y
                        msg.Append(bba); // a
                        msg.Append(behaviour.size.x / width); // w (bounding box dimension)
                        msg.Append(behaviour.size.y / height); // h
                        msg.Append(behaviour.size.x / width * behaviour.size.y / height); // f (area)
                        msg.Append(-behaviour.normalizedVelocity.x); // X
                        msg.Append(-behaviour.normalizedVelocity.z); // Y
                        msg.Append(0.0f); // A
                        msg.Append(0.0f); // m
                        msg.Append(0.0f); // r
                        break;
                    case TUIOManager.AugmentaTUIODimension.TUIO25D:
                        msg.Append(behaviour.id); // s
                        msg.Append(pointX); // x
                        msg.Append(pointY); // y
                        msg.Append(z); // z
                        msg.Append(bba); // a 
                        msg.Append(behaviour.size.x / width); // w (bounding box dimension)
                        msg.Append(behaviour.size.y / height); // h
                        msg.Append(behaviour.size.x / width * behaviour.size.y / height); // f (area)
                        msg.Append(-behaviour.normalizedVelocity.x); // X
                        msg.Append(-behaviour.normalizedVelocity.z); // Y
                        msg.Append(-behaviour.normalizedVelocity.y / TUIOManager.activeManager.sceneDepth); // Z
                        msg.Append(0.0f); // A 
                        msg.Append(0.0f); // m
                        msg.Append(0.0f); // r
                        break;
                    case TUIOManager.AugmentaTUIODimension.TUIO3D:
                        msg.Append(behaviour.id); // s
                        msg.Append(pointX); // x
                        msg.Append(pointY); // y
                        msg.Append(z); // z
                        msg.Append(bba); // a
                        msg.Append(0.0f); // b
                        msg.Append(0.0f); // c
                        msg.Append(behaviour.size.x / width); // w (bounding box dimension)
                        msg.Append(behaviour.size.y / height); // h
                        msg.Append(behaviour.size.z / TUIOManager.activeManager.sceneDepth); // d
                        msg.Append(behaviour.size.x / width * behaviour.size.y / height * behaviour.size.z / TUIOManager.activeManager.sceneDepth); // v (volume)
                        msg.Append(-behaviour.normalizedVelocity.x); // X
                        msg.Append(-behaviour.normalizedVelocity.z); // Y
                        msg.Append(-behaviour.normalizedVelocity.y / TUIOManager.activeManager.sceneDepth); // Z
                        msg.Append(0.0f); // A
                        msg.Append(0.0f); // B
                        msg.Append(0.0f); // C
                        msg.Append(0.0f); // m
                        msg.Append(0.0f); // r
                        break;
                }

                break;
        }

        return msg;
    }

    private OSCMessage CreateAugmentaMessageTUIOScene()
    {
        string addr = "/scene";
        var msg = new OSCMessage(addr);

        msg.Append(Time.frameCount);  // Frame number
        msg.Append(_pointsCount);   // Objects count
        msg.Append(width);          // Scene width
        msg.Append(height);         // Scene height
        msg.Append(TUIOManager.activeManager.sceneDepth);          // For TUIO output we add the sceneDepth parameter to the scene message (because we use it to normalize the height to follow the TUIO standard)

        return msg;
    }

    #endregion

    #region Websocket

    public String CreateAugmentaMessageJSON(AugmentaMessageType messageType, GameObject obj = null) {

        switch (messageType) {
            case AugmentaMessageType.AugmentaObjectEnter:
                return "{\n\"object\": {\n\"enter\": " + CreateAugmentaMessageJSONData(obj) + "\n}\n}";
            case AugmentaMessageType.AugmentaObjectUpdate:
                return "{\n\"object\": {\n\"update\": " + CreateAugmentaMessageJSONData(obj) + "\n}\n}";
            case AugmentaMessageType.AugmentaObjectLeave:
                return "{\n\"object\": {\n\"leave\": " + CreateAugmentaMessageJSONData(obj) + "\n}\n}";
            case AugmentaMessageType.SceneUpdated:
                return CreateAugmentaMessageJSONScene();
            default:
                Debug.Log("Unsupported message type " + messageType.ToString());
                return String.Empty;
        }
    }

    private String CreateAugmentaMessageJSONData(GameObject obj = null) {
        var behaviour = obj.GetComponent<PointBehaviour>();
        float pointX = 0.5f + behaviour.transform.position.x / width;
        float pointY = 0.5f - behaviour.transform.position.z / height;
        float rotation = behaviour.point.transform.localRotation.eulerAngles.z >= 0 ? behaviour.point.transform.localRotation.eulerAngles.z : behaviour.point.transform.localRotation.eulerAngles.z + 360.0f;

        return "{\n\"frame\":" + Time.frameCount.ToString() + ",\n\"id\":" + behaviour.id.ToString() +
              ",\n\"slotid\":" + behaviour.slotid.ToString() + ",\n\"age\":" + behaviour.ageInSeconds.ToString() +
              ",\n\"centroid\": {\n\"x\":" + pointX.ToString() + ",\n\"y\":" + pointY.ToString() + "\n}" +
              ",\n\"velocity\": {\n\"x\":" + (-behaviour.normalizedVelocity.x).ToString() + ",\n\"y\":" + (-behaviour.normalizedVelocity.z).ToString() + "\n}" +
              ",\n\"orientation\":" + behaviour.orientation.ToString() +
              ",\n\"boundingRect\": {\n\"x\":" + pointX.ToString() + ",\n\"y\":" + pointY.ToString() +
              ",\n\"width\":" + (behaviour.size.x / width).ToString() + ",\n\"height\":" + (behaviour.size.y / height).ToString() + ",\n\"rotation\": " + rotation.ToString() +
              "\n},\n\"height\" :" + behaviour.size.z.ToString() +
              ",\n\"extra\": {\n\"frame\": " + Time.frameCount.ToString() + ",\"id\": " + behaviour.id.ToString() + ",\"slotid\": " + behaviour.slotid.ToString() +
              ",\n\"highest\": {\n\"x\": " + pointX.ToString() + ",\n\"y\": " + pointY.ToString() + "\n}" +
              ",\n\"distance\": " + Vector3.Distance(camera.transform.position, behaviour.transform.position).ToString() + ",\"reflectivity\": " + behaviour.pointColor.grayscale + "\n}\n}";
    }

    private String CreateAugmentaMessageJSONScene() {
        return "{\n\"scene\" : {\n\"frame\": " + Time.frameCount.ToString() + ",\n\"objectCount\" : " + _pointsCount.ToString() +
                ",\n\"scene\" : {\n\"width\" : " + width.ToString() + ",\n\"height\" : " + height.ToString() + "\n}\n}\n}";
    }

    #endregion
}
