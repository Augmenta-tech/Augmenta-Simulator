using System.Collections;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Net;
using System.Linq;

public class PointManager : MonoBehaviour {

    [Header("Raycast Settings")]
    public new Camera camera;
    public LayerMask areaLayer;
    public LayerMask pointsLayer;

    [Header("Output settings")]
    public List<string> protocolVersions;

    private string _protocolVersion = "2";
    public string protocolVersion {
        get { return _protocolVersion; }
        set { _protocolVersion = value; }
    }

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
            if (instantiatedPoints == null) return;

            foreach (var point in instantiatedPoints.Values)
                UpdatePointColor(point.GetComponent<PointBehaviour>());

            foreach (var point in _incorrectInstantiatedPoints.Values)
                UpdatePointColor(point.GetComponent<PointBehaviour>());

            foreach (var point in _flickeringPoints.Values)
                UpdatePointColor(point.GetComponent<PointBehaviour>());
        }
    }

    public int pointsCount {
        get { return _pointsCount; }
        set { }
    }
    private int _pointsCount;

    public int desiredPointsCount {
        get { return _desiredPointsCount; }
        set { _desiredPointsCount = value; UpdateInstantiatedPointsCount(); }
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

    public float incorrectDetectionProbability = 0;
    public float incorrectDetectionDuration = 0.1f;

    public float pointFlickeringProbability = 0;
    public float pointFlickeringDuration = 0.1f;

    public GameObject pointPrefab;

    public static Dictionary<int, GameObject> instantiatedPoints;

    public Material backgroundMaterial;

    private int _frameCounter;
    private int _highestPid;

    private GameObject _cursorPoint;
    private Ray _ray;
    private RaycastHit _raycastHit;

    private Dictionary<int, GameObject> _incorrectInstantiatedPoints;
    private Dictionary<int, GameObject> _flickeringPoints;

    private List<int> _keysList;

    #region MonoBehaviour Implementation

    void Start () {

        instantiatedPoints = new Dictionary<int, GameObject>();
        _incorrectInstantiatedPoints = new Dictionary<int, GameObject>();
        _flickeringPoints = new Dictionary<int, GameObject>();

        _highestPid = 0;

        UpdateAreaSize();
    }

    void Update() {

        //Check keyboard inputs
        ProcessKeyboardInputs();

        _frameCounter++;

        //Process mouse
        ProcessMouseInputs();

        //Create incorrect detections
        CreateIncorrectDetection();

        //Create flickering
        CreateFlickeringPoints();

        //Send Augmenta scene message
        SendSceneUpdated();

        //Send Augmenta persons messages
        foreach (var point in instantiatedPoints)
            SendPersonUpdated(point.Value);

        foreach (var point in _incorrectInstantiatedPoints)
            SendPersonUpdated(point.Value);
    }

    public void OnMouseDrag() {
        if (_cursorPoint == null) return;

        _ray = camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(_ray, out _raycastHit, 100.0f, areaLayer)) {

            _cursorPoint.transform.position = new Vector3(_raycastHit.point.x, 0, _raycastHit.point.z);
        }
    }

	#endregion

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

            if (Physics.Raycast(_ray, out _raycastHit, 100.0f, pointsLayer)) {

                //Point hit
                _cursorPoint = _raycastHit.transform.gameObject;

            } else if(Physics.Raycast(_ray, out _raycastHit, 100.0f, areaLayer)) {

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

            if (Physics.Raycast(_ray, out _raycastHit, 100.0f, pointsLayer)) {

                RemovePoint(_raycastHit.transform.GetComponent<PointBehaviour>().pid);

            }
        }

    }

	#endregion

	#region Points Handling

    /// <summary>
    /// Update point color
    /// </summary>
    /// <param name="target"></param>
	public void UpdatePointColor(PointBehaviour target) {
        if (mute)
            target.UpdatePointColor(Color.gray);
        else
            target.UpdatePointColor(target.pointColor);
    }

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

        if(Random.Range(0.0f, 1.0f) <= incorrectDetectionProbability) {
            InstantiatePoint(true);
        }
    }

    /// <summary>
    /// Create point flickering
    /// </summary>
    private void CreateFlickeringPoints() {

        if (instantiatedPoints.Count == 0)
            return;

        if (Random.Range(0.0f, 1.0f) <= pointFlickeringProbability) {

            int flickeringIndex = Random.Range(0, instantiatedPoints.Count);

            try {
                int pidToFlicker = instantiatedPoints.ElementAt(flickeringIndex).Key;

                _flickeringPoints.Add(pidToFlicker, instantiatedPoints.ElementAt(flickeringIndex).Value);

                SendPersonLeft(instantiatedPoints[pidToFlicker]);
                instantiatedPoints.ElementAt(flickeringIndex).Value.GetComponent<PointBehaviour>().StartFlickering();
                instantiatedPoints.Remove(pidToFlicker);
                _pointsCount--;

                //Update OIDs
                UpdateOIDs();
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
        UpdateOIDs();
        SendPersonEntered(instantiatedPoints[pid]);
    }

    /// <summary>
    /// Create or remove instantiated points according to the desired point count
    /// </summary>
    public void UpdateInstantiatedPointsCount() {

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

		newPointBehaviour.pointColor = Color.HSVToRGB(Random.value, 0.85f, 0.75f);
		newPointBehaviour.manager = this;
		UpdatePointColor(newPointBehaviour);
		newPoint.transform.parent = transform;
        newPoint.transform.localPosition = GetNewPointPosition();
		newPointBehaviour.speed = speed;
		newPointBehaviour.pid = _highestPid;
        newPointBehaviour.size = new Vector3(Random.Range(minPointSize.x, maxPointSize.x), 
                                             Random.Range(minPointSize.y, maxPointSize.y), 
                                             Random.Range(minPointSize.z, maxPointSize.z));
        newPointBehaviour.animateSize = animateSize;
        newPointBehaviour.sizeVariationSpeed = sizeVariationSpeed;
        newPointBehaviour.movementNoiseAmplitude = movementNoiseAmplitude;
        newPointBehaviour.movementNoiseFrequency = movementNoiseFrequency;
        newPointBehaviour.isIncorrectDetection = isIncorrectDetection;
        newPointBehaviour.isFlickering = false;

        if (isIncorrectDetection) {
            _incorrectInstantiatedPoints.Add(_highestPid, newPoint);
            UpdateOIDs();
            SendPersonEntered(_incorrectInstantiatedPoints[_highestPid]);
        } else {
            instantiatedPoints.Add(_highestPid, newPoint);
            UpdateOIDs();
            SendPersonEntered(instantiatedPoints[_highestPid]);
        }

        if (isFromCursor) {
            _cursorPoint = newPoint;
            OnMouseDrag();
        }

        _highestPid++;
        _pointsCount++;
	}

    /// <summary>
    /// Return a local position for a new point
    /// </summary>
    /// <returns></returns>
    Vector3 GetNewPointPosition() {

        float x = Random.Range(0.0f, 1.0f);
        float y = Random.Range(0.0f, 1.0f);
        
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

		SendPersonLeft(instantiatedPoints[pidToRemove]);
		Destroy(instantiatedPoints[pidToRemove]);
		instantiatedPoints.Remove(pidToRemove);

        //Update OIDs
        UpdateOIDs();

        _pointsCount--;
    }

    /// <summary>
    /// Remove point with given pid
    /// </summary>
    public void RemovePoint(int pid) {

        if (instantiatedPoints.ContainsKey(pid)) {
            SendPersonLeft(instantiatedPoints[pid]);
            Destroy(instantiatedPoints[pid]);
            instantiatedPoints.Remove(pid);

            //Update OIDs
            UpdateOIDs();

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
            SendPersonLeft(_incorrectInstantiatedPoints[pid]);
            Destroy(_incorrectInstantiatedPoints[pid]);
            _incorrectInstantiatedPoints.Remove(pid);

            //Update OIDs
            UpdateOIDs();

            _pointsCount--;
        }
    }

    /// <summary>
    /// Recompute points OIDs
    /// </summary>
    private void UpdateOIDs() {

        _keysList = instantiatedPoints.Keys.ToList();
        _keysList.Sort();

        for(int i=0; i<instantiatedPoints.Count; i++) {
            instantiatedPoints[_keysList[i]].GetComponent<PointBehaviour>().oid = i;
        }

        _keysList = _incorrectInstantiatedPoints.Keys.ToList();
        _keysList.Sort();

        for (int i = 0; i < _incorrectInstantiatedPoints.Count; i++) {
            _incorrectInstantiatedPoints[_keysList[i]].GetComponent<PointBehaviour>().oid = instantiatedPoints.Count + i;
        }

    }

    public void RemovePoints() {
        _pointsCount = 0;
        _highestPid = 0;

        foreach (var obj in instantiatedPoints) {
            SendPersonLeft(obj.Value);
            Destroy(obj.Value);
        }

        instantiatedPoints.Clear();
    }

	#endregion

	#region Area Handling

    private void UpdateAreaSize() {

        if (_width <= 0) _width = 1.0f;
        if (_height <= 0) _height = 1.0f;

        transform.localScale = new Vector3(_width, _height, 1);

        UpdateBackgroundTexture();

    }

	private void UpdateBackgroundTexture()
    {
        backgroundMaterial.mainTextureScale = transform.localScale * 0.5f;
    }

	#endregion

	#region OSC Message

	//OSC Protocol V1

	/*
        0: pid (int)                        // Personal ID ex : 42th person to enter stage has pid=42
        1: oid (int)                        // Ordered ID ex : if 3 person on stage, 43th person might have oid=2
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

    public void SendSceneUpdated()
    {
        if (mute) return;

        var msg = new UnityOSC.OSCMessage("/au/scene");
        msg.Append(_frameCounter);
        //Compute point size
        msg.Append(instantiatedPoints.Count * 0.25f * (maxPointSize.x + minPointSize.x) * (maxPointSize.y + minPointSize.y));
        msg.Append(_pointsCount);
        //Compute average motion
        var velocitySum = Vector3.zero;
        foreach(var element in instantiatedPoints)
        {
            velocitySum += -element.Value.GetComponent<PointBehaviour>().normalizedVelocity;
        }

        if(instantiatedPoints.Count > 0)
            velocitySum /= instantiatedPoints.Count;

        msg.Append(velocitySum.x);
        msg.Append(velocitySum.z);
        if(protocolVersion == "1") {
            msg.Append((int)(width / pixelSize));
            msg.Append((int)(height / pixelSize));
        } else if( protocolVersion == "2") {
            msg.Append(width);
            msg.Append(height);
        }
        msg.Append(100);

        OSCManager.activeManager.SendAugmentaMessage(msg);
    }

    public void SendPersonEntered(GameObject obj)
    {
        SendAugmentaMessage("/au/personEntered", obj);
    }

    public void SendPersonUpdated(GameObject obj)
    {
        SendAugmentaMessage("/au/personUpdated", obj);
    }

    public void SendPersonLeft(GameObject obj)
    {
        SendAugmentaMessage("/au/personWillLeave", obj);
    }


    public void SendAugmentaMessage(string address, GameObject obj)
    {
        if (mute) return;

        var msg = new UnityOSC.OSCMessage(address);
        var behaviour = obj.GetComponent<PointBehaviour>();
        float pointX = 0.5f + behaviour.transform.position.x / width;
        float pointY = 0.5f - behaviour.transform.position.z / height;

        msg.Append(behaviour.pid);

        //oid
        msg.Append(behaviour.oid);

        msg.Append((int)behaviour.age);
        //centroid
        msg.Append(pointX);
        msg.Append(pointY);
        //Velocity
        msg.Append(-behaviour.normalizedVelocity.x);
        msg.Append(-behaviour.normalizedVelocity.z);

        msg.Append(0);

        //Bounding
        msg.Append(pointX - behaviour.size.x * 0.5f / width);
        msg.Append(pointY - behaviour.size.y * 0.5f / height);

        msg.Append(behaviour.size.x / width);
        msg.Append(behaviour.size.y / height);

        msg.Append(pointX);
        msg.Append(pointY);
        msg.Append(behaviour.size.z);

        OSCManager.activeManager.SendAugmentaMessage(msg);
    }

    #endregion
}
