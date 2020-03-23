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
    public List<string> ProtocolVersions;
    public string ProtocolVersion = "1";

    [Header("Area settings")]
    private float _width = 5;
    public float Width {
        get { return _width; }
        set { _width = value; UpdateAreaSize(); }
    }

    private float _height = 5;
    public float Height {
        get { return _height; }
        set { _height = value; UpdateAreaSize(); }
    }

    private float _meterPerPixel = 0.005f;
    public float MeterPerPixel {
        get { return _meterPerPixel; }
        set { _meterPerPixel = value; }
    }

    [Header("Points settings")]
    public bool _mute;
    public bool Mute {
        get { return _mute; }
        set { _mute = value;
            if (InstantiatedPoints == null) return;

            foreach (var point in InstantiatedPoints.Values)
                UpdatePointColor(point.GetComponent<PointBehaviour>());

            foreach (var point in _incorrectInstantiatedPoints.Values)
                UpdatePointColor(point.GetComponent<PointBehaviour>());

            foreach (var point in _flickeringPoints.Values)
                UpdatePointColor(point.GetComponent<PointBehaviour>());
        }
    }

    public int PointsCount {
        get { return _pointsCount; }
        set { }
    }
    private int _pointsCount;

    public int DesiredPointsCount {
        get { return _desiredPointsCount; }
        set { _desiredPointsCount = value; UpdateInstantiatedPointsCount(); }
    }
    private int _desiredPointsCount;

    private float _speed = 1;
    public float Speed {
        get { return _speed; }
        set { _speed = value;
            if (InstantiatedPoints == null) return;

            foreach (var obj in InstantiatedPoints)
                obj.Value.GetComponent<PointBehaviour>().Speed = Speed;

            foreach (var obj in _incorrectInstantiatedPoints)
                obj.Value.GetComponent<PointBehaviour>().Speed = Speed;

            foreach (var obj in _flickeringPoints)
                obj.Value.GetComponent<PointBehaviour>().Speed = Speed;
        }
    }

    //Points size

    private Vector3 _minPointSize = new Vector3(0.3f, 0.3f, 1.5f);
    public Vector3 MinPointSize {
        get { return _minPointSize; }
        set {
            _minPointSize = value;
            UpdatePointsSize();
        }
    }

    private Vector3 _maxPointSize = new Vector3(0.7f, 0.7f, 2.0f);
    public Vector3 MaxPointSize {
        get { return _maxPointSize; }
        set {
            _maxPointSize = value;
            UpdatePointsSize();
        }
    }

    private bool _animateSize = false;
    public bool AnimateSize {
        get { return _animateSize; }
        set { _animateSize = value;

            if (InstantiatedPoints == null) return;

            foreach (var obj in InstantiatedPoints)
                obj.Value.GetComponent<PointBehaviour>().animateSize = _animateSize;

            foreach (var obj in _incorrectInstantiatedPoints)
                obj.Value.GetComponent<PointBehaviour>().animateSize = _animateSize;

            foreach (var obj in _flickeringPoints)
                obj.Value.GetComponent<PointBehaviour>().animateSize = _animateSize;
        }
    }

    private float _sizeVariationSpeed = .2f;
    public float SizeVariationSpeed {
        get { return _sizeVariationSpeed; }
        set {
            _sizeVariationSpeed = value;

            if (InstantiatedPoints == null) return;

            foreach (var obj in InstantiatedPoints)
                obj.Value.GetComponent<PointBehaviour>().sizeVariationSpeed = _sizeVariationSpeed;

            foreach (var obj in _incorrectInstantiatedPoints)
                obj.Value.GetComponent<PointBehaviour>().sizeVariationSpeed = _sizeVariationSpeed;

            foreach (var obj in _flickeringPoints)
                obj.Value.GetComponent<PointBehaviour>().sizeVariationSpeed = _sizeVariationSpeed;
        }
    }

    //Noise Parameters

    private float _movementNoise = 0;
    public float MovementNoise {
        get { return _movementNoise; }
        set { _movementNoise = value;
            if (InstantiatedPoints == null) return;

            foreach (var obj in InstantiatedPoints)
                obj.Value.GetComponent<PointBehaviour>().movementNoise = _movementNoise;

            foreach (var obj in _incorrectInstantiatedPoints)
                obj.Value.GetComponent<PointBehaviour>().movementNoise = _movementNoise;

            foreach (var obj in _flickeringPoints)
                obj.Value.GetComponent<PointBehaviour>().movementNoise = _movementNoise;
        }
    }

    public float IncorrectDetectionProbability = 0;
    public float IncorrectDetectionDuration = 0.1f;

    public float PointFlickeringProbability = 0;
    public float PointFlickeringDuration = 0.1f;

    public GameObject PointPrefab;

    public static Dictionary<int, GameObject> InstantiatedPoints;

    public Material backgroundMaterial;

    private int _frameCounter;
    private int _highestPid;

    private GameObject _cursorPoint;
    private Ray ray;
    private RaycastHit raycastHit;

    private Dictionary<int, GameObject> _incorrectInstantiatedPoints;
    private Dictionary<int, GameObject> _flickeringPoints;

    private List<int> keysList;

    #region MonoBehaviour Implementation

    void Start () {

        InstantiatedPoints = new Dictionary<int, GameObject>();
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
        foreach (var point in InstantiatedPoints)
            SendPersonUpdated(point.Value);

        foreach (var point in _incorrectInstantiatedPoints)
            SendPersonUpdated(point.Value);
    }

    public void OnMouseDrag() {
        if (_cursorPoint == null) return;

        ray = camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out raycastHit, 100.0f, areaLayer)) {

            _cursorPoint.transform.position = new Vector3(raycastHit.point.x, raycastHit.point.y, 0);
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
            Mute = !Mute;
    }

    /// <summary>
    /// Process mouse button presses
    /// </summary>
    public void ProcessMouseInputs() {

        //Left clic
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()
            && !Input.GetKey(KeyCode.LeftAlt)) {
            ray = camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out raycastHit, 100.0f, pointsLayer)) {

                //Point hit
                _cursorPoint = raycastHit.transform.gameObject;

            } else if(Physics.Raycast(ray, out raycastHit, 100.0f, areaLayer)) {

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
            ray = camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out raycastHit, 100.0f, pointsLayer)) {

                RemovePoint(raycastHit.transform.GetComponent<PointBehaviour>().pid);

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
        if (Mute)
            target.UpdatePointColor(Color.gray);
        else
            target.UpdatePointColor(target.PointColor);
    }

    /// <summary>
    /// Update points size to match min/max size criteria
    /// </summary>
    void UpdatePointsSize() {

        if (InstantiatedPoints == null) return;

        PointBehaviour tmpBehaviour;

        foreach (var obj in InstantiatedPoints) {
            tmpBehaviour = obj.Value.GetComponent<PointBehaviour>();
            tmpBehaviour.size = Vector3.Min(tmpBehaviour.size, MaxPointSize);
            tmpBehaviour.size = Vector3.Max(tmpBehaviour.size, MinPointSize);
        }

        foreach (var obj in _incorrectInstantiatedPoints) {
            tmpBehaviour = obj.Value.GetComponent<PointBehaviour>();
            tmpBehaviour.size = Vector3.Min(tmpBehaviour.size, MaxPointSize);
            tmpBehaviour.size = Vector3.Max(tmpBehaviour.size, MinPointSize);
        }

        foreach (var obj in _flickeringPoints) {
            tmpBehaviour = obj.Value.GetComponent<PointBehaviour>();
            tmpBehaviour.size = Vector3.Min(tmpBehaviour.size, MaxPointSize);
            tmpBehaviour.size = Vector3.Max(tmpBehaviour.size, MinPointSize);
        }

    }

    /// <summary>
    /// Instantiate incorrect detection points
    /// </summary>
    private void CreateIncorrectDetection() {

        if(Random.Range(0.0f, 1.0f) <= IncorrectDetectionProbability) {
            InstantiatePoint(true);
        }
    }

    /// <summary>
    /// Create point flickering
    /// </summary>
    private void CreateFlickeringPoints() {

        if (InstantiatedPoints.Count == 0)
            return;

        if (Random.Range(0.0f, 1.0f) <= PointFlickeringProbability) {

            int flickeringIndex = Random.Range(1, InstantiatedPoints.Count);

            try {
                int pidToFlicker = InstantiatedPoints.ElementAt(flickeringIndex).Key;

                _flickeringPoints.Add(pidToFlicker, InstantiatedPoints.ElementAt(flickeringIndex).Value);

                SendPersonLeft(InstantiatedPoints[pidToFlicker]);
                InstantiatedPoints.ElementAt(flickeringIndex).Value.GetComponent<PointBehaviour>().StartFlickering();
                InstantiatedPoints.Remove(pidToFlicker);
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

        InstantiatedPoints.Add(pid, _flickeringPoints[pid]);
        _pointsCount++;

        var pointBehaviour = _flickeringPoints[pid].GetComponent<PointBehaviour>();
        pointBehaviour.isFlickering = false;
        pointBehaviour.ShowPoint();

        _flickeringPoints.Remove(pid);
        UpdateOIDs();
        SendPersonEntered(InstantiatedPoints[pid]);
    }

    /// <summary>
    /// Create or remove instantiated points according to the desired point count
    /// </summary>
    public void UpdateInstantiatedPointsCount() {

        if (_desiredPointsCount <= 0) _desiredPointsCount = 0;

        //Create new points
        while (InstantiatedPoints.Count < _desiredPointsCount) {
            InstantiatePoint();
        }

        //Remove points
        while (InstantiatedPoints.Count > _desiredPointsCount) {
            RemovePoint();
        }
    }

    /// <summary>
    /// Instantiate new point
    /// </summary>
    public void InstantiatePoint(bool isIncorrectDetection = false, bool isFromCursor = false) {

		GameObject newPoint = Instantiate(PointPrefab);

		PointBehaviour newPointBehaviour = newPoint.GetComponent<PointBehaviour>();

		newPointBehaviour.PointColor = Color.HSVToRGB(Random.value, 0.85f, 0.75f);
		newPointBehaviour.manager = this;
		UpdatePointColor(newPointBehaviour);
		newPoint.transform.parent = transform;
        newPoint.transform.localPosition = GetNewPointPosition();
		newPointBehaviour.Speed = Speed;
		newPointBehaviour.pid = _highestPid;
        newPointBehaviour.size = new Vector3(Random.Range(MinPointSize.x, MaxPointSize.x), 
                                             Random.Range(MinPointSize.y, MaxPointSize.y), 
                                             Random.Range(MinPointSize.z, MaxPointSize.z));
        newPointBehaviour.animateSize = AnimateSize;
        newPointBehaviour.sizeVariationSpeed = SizeVariationSpeed;
        newPointBehaviour.movementNoise = MovementNoise;
        newPointBehaviour.isIncorrectDetection = isIncorrectDetection;
        newPointBehaviour.isFlickering = false;

        if (isIncorrectDetection) {
            _incorrectInstantiatedPoints.Add(_highestPid, newPoint);
            UpdateOIDs();
            SendPersonEntered(_incorrectInstantiatedPoints[_highestPid]);
        } else {
            InstantiatedPoints.Add(_highestPid, newPoint);
            UpdateOIDs();
            SendPersonEntered(InstantiatedPoints[_highestPid]);
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

        if (InstantiatedPoints.Count == 0)
            return;

        int pidToRemove = InstantiatedPoints.ElementAt(InstantiatedPoints.Count - 1).Key;

        //Do not remove the cursor point unless it is the last one
        if(pidToRemove == 0 && InstantiatedPoints.Count > 1)
            pidToRemove = InstantiatedPoints.ElementAt(InstantiatedPoints.Count - 2).Key;

		SendPersonLeft(InstantiatedPoints[pidToRemove]);
		Destroy(InstantiatedPoints[pidToRemove]);
		InstantiatedPoints.Remove(pidToRemove);

        //Update OIDs
        UpdateOIDs();

        _pointsCount--;
    }

    /// <summary>
    /// Remove point with given pid
    /// </summary>
    public void RemovePoint(int pid) {

        if (InstantiatedPoints.ContainsKey(pid)) {
            SendPersonLeft(InstantiatedPoints[pid]);
            Destroy(InstantiatedPoints[pid]);
            InstantiatedPoints.Remove(pid);

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

        keysList = InstantiatedPoints.Keys.ToList();
        keysList.Sort();

        for(int i=0; i<InstantiatedPoints.Count; i++) {
            InstantiatedPoints[keysList[i]].GetComponent<PointBehaviour>().oid = i;
        }

        keysList = _incorrectInstantiatedPoints.Keys.ToList();
        keysList.Sort();

        for (int i = 0; i < _incorrectInstantiatedPoints.Count; i++) {
            _incorrectInstantiatedPoints[keysList[i]].GetComponent<PointBehaviour>().oid = InstantiatedPoints.Count + i;
        }

    }

    public void RemovePoints() {
        _pointsCount = 0;
        _highestPid = 0;

        foreach (var obj in InstantiatedPoints) {
            SendPersonLeft(obj.Value);
            Destroy(obj.Value);
        }

        InstantiatedPoints.Clear();
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
        if (Mute) return;

        var msg = new UnityOSC.OSCMessage("/au/scene");
        msg.Append(_frameCounter);
        //Compute point size
        msg.Append(InstantiatedPoints.Count * 0.25f * (MaxPointSize.x + MinPointSize.x) * (MaxPointSize.y + MinPointSize.y));
        msg.Append(_pointsCount);
        //Compute average motion
        var velocitySum = Vector3.zero;
        foreach(var element in InstantiatedPoints)
        {
            velocitySum += -element.Value.GetComponent<PointBehaviour>().NormalizedVelocity;
        }
        velocitySum /= InstantiatedPoints.Count;

        msg.Append(velocitySum.x);
        msg.Append(velocitySum.y);
        if(ProtocolVersion == "1") {
            msg.Append((int)(Width / MeterPerPixel));
            msg.Append((int)(Height / MeterPerPixel));
        } else if( ProtocolVersion == "2") {
            msg.Append(Width);
            msg.Append(Height);
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
        if (Mute) return;

        var msg = new UnityOSC.OSCMessage(address);
        var behaviour = obj.GetComponent<PointBehaviour>();
        float pointX = 0.5f + behaviour.transform.position.x / Width;
        float pointY = 0.5f - behaviour.transform.position.y / Height;


        msg.Append(behaviour.pid);

        //oid
        msg.Append(behaviour.oid);

        msg.Append((int)behaviour.Age);
        //centroid
        msg.Append(pointX);
        msg.Append(pointY);
        //Velocity
        msg.Append(-behaviour.NormalizedVelocity.x);
        msg.Append(-behaviour.NormalizedVelocity.y);

        msg.Append(0);

        //Bounding
        msg.Append(pointX - behaviour.size.x * 0.5f / Width);
        msg.Append(pointY - behaviour.size.y * 0.5f / Height);

        msg.Append(behaviour.size.x / Width);
        msg.Append(behaviour.size.y / Height);

        msg.Append(pointX);
        msg.Append(pointY);
        msg.Append(behaviour.size.z);

        OSCManager.activeManager.SendAugmentaMessage(msg);
    }

    #endregion
}
