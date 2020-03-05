using System.Collections;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Net;
using System.Linq;

public class PointManager : MonoBehaviour {

    [Header("Output settings")]
    private int _outputPort = 12000;
    public int OutputPort {
        get { return _outputPort; }
        set { _outputPort = value; InitConnection(); }
    }

    private string _outputIp = "127.0.0.1";
    public string OutputIP {
        get { return _outputIp; }
        set { _outputIp = value; InitConnection(); }
    }

    [Header("Area settings")]
    private int _width = 1280;
    public int Width {
        get { return _width; }
        set { _width = value; ChangeResolution(); }
    }
    public Vector2 WidthLimit;

    private int _height = 800;
    public int Height {
        get { return _height; }
        set { _height = value; ChangeResolution(); }
    }
    public Vector2 HeightLimit;

    public float Ratio;

    [Header("Points settings")]
    public bool _mute;
    public bool Mute {
        get { return _mute; }
        set { _mute = value;
            if (InstantiatedPoints == null) return;
            foreach (var point in InstantiatedPoints.Values)
                UpdatePointColor(point.GetComponent<PointBehaviour>());
        }
    }

    public int PointsCount;

    private Vector2 _pointSize = new Vector2(75, 75);
    public Vector2 PointSize {
        get { return _pointSize; }
        set { _pointSize = value;
            if (InstantiatedPoints == null) return;
            foreach (var obj in InstantiatedPoints)
                obj.Value.transform.localScale = new Vector3(PointSize.x, PointSize.y, 2f);
        }
    }

    private float _speed = 1;
    public float Speed {
        get { return _speed; }
        set { _speed = value;
            if (InstantiatedPoints == null) return;
            foreach (var obj in InstantiatedPoints)
                obj.Value.GetComponent<PointBehaviour>().Speed = Speed;
        }
    }

    //Noise Parameters

    private float _noiseIntensity = 0;
    public float NoiseIntensity {
        get { return _noiseIntensity; }
        set { _noiseIntensity = value;
            if (InstantiatedPoints == null) return;
            foreach (var obj in InstantiatedPoints)
                obj.Value.GetComponent<PointBehaviour>().noiseIntensity = _noiseIntensity;
        }
    }

    public float IncorrectDetectionProbability = 0;
    public float IncorrectDetectionDuration = 0.1f;

    public float PointFlickeringProbability = 0;
    public float PointFlickeringDuration = 0.1f;

    public GameObject Prefab;
    public bool CanMoveCursorPoint;

    public static Dictionary<int, GameObject> InstantiatedPoints;

    private int _frameCounter;
    private bool _clickStartedOutsideUI;
    private int _highestPid;
    private GameObject _cursorPoint;

    private Dictionary<int, GameObject> _incorrectInstantiatedPoints;
    private Dictionary<int, GameObject> _flickeringPoints;

    #region MonoBehaviour Implementation

    void Start () {

        InstantiatedPoints = new Dictionary<int, GameObject>();
        _incorrectInstantiatedPoints = new Dictionary<int, GameObject>();
        _flickeringPoints = new Dictionary<int, GameObject>();

        CanMoveCursorPoint = true;

        _highestPid = 0;

        ChangeResolution();

        InitConnection();
    }

    void Update() {

        //Check keyboard inputs
        ProcessKeyboardInputs();

        _frameCounter++;

        //Add or remove points to match desired point count
        UpdateInstantiatedPointsCount();

        //Process mouse
        ProcessMouseInputs();

        //Create incorrect detections
        CreateIncorrectDetection();

        //Create flickering
        CreateFlickeringPoints();

        //Update camera
        //!\ SHOULD PROBABLY ONLY BE DONE ON RESOLUTION CHANGE /!\\
        ComputeOrthoCamera();

        //Send Augmenta scene message
        SendSceneUpdated();

        //Send Augmenta persons messages
        foreach (var point in InstantiatedPoints)
            SendPersonUpdated(point.Value);

        foreach (var point in _incorrectInstantiatedPoints)
            SendPersonUpdated(point.Value);
    }

    public void OnMouseDrag() {
        if (_cursorPoint == null || !CanMoveCursorPoint || !_clickStartedOutsideUI) return;

        var newPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        newPos.z = 0;

        newPos.x = Mathf.Clamp(newPos.x, 0f, 1f);
        newPos.y = Mathf.Clamp(newPos.y, 0f, 1f);

        newPos = Camera.main.ViewportToWorldPoint(newPos);
        newPos.z = 0;
        _cursorPoint.transform.position = newPos;
    }

    #endregion

    /// <summary>
    /// Process keyboard key presses
    /// </summary>
    public void ProcessKeyboardInputs()
    {
        if (Input.GetKeyUp(KeyCode.M))
            Mute = !Mute;

        if (Input.GetKeyUp(KeyCode.Escape))
            Application.Quit();

        if (Input.GetKeyUp(KeyCode.R))
            RemovePoints();
    }

    /// <summary>
    /// Process mouse button presses
    /// </summary>
    public void ProcessMouseInputs() {

        //Weird behaviour "fix"
        if (PointsCount == 0)
            CanMoveCursorPoint = true;

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) {
            _clickStartedOutsideUI = true;
        }

        if (Input.GetMouseButtonUp(0)) {
            _clickStartedOutsideUI = false;
        }

        if (Input.GetMouseButton(1) && !EventSystem.current.IsPointerOverGameObject()) {
            if (_cursorPoint != null) {
                PointsCount--;
                SendPersonLeft(InstantiatedPoints[0]);
                InstantiatedPoints.Remove(0);
                Destroy(_cursorPoint);
            }
        } else if (Input.GetMouseButton(0) && CanMoveCursorPoint && !EventSystem.current.IsPointerOverGameObject()) {
            if (_cursorPoint == null) {
                _cursorPoint = Instantiate(Prefab);

                PointBehaviour cursorPointBehaviour = _cursorPoint.GetComponent<PointBehaviour>();

                cursorPointBehaviour.PointColor = Color.HSVToRGB(Random.value, 0.85f, 0.75f);
                UpdatePointColor(cursorPointBehaviour);
                _cursorPoint.transform.parent = transform;
                var newPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                newPos.z = 0;
                newPos.x = Mathf.Clamp(newPos.x, 0.05f, 0.95f);
                newPos.y = Mathf.Clamp(newPos.y, 0.05f, 0.95f);
                newPos = Camera.main.ViewportToWorldPoint(newPos);
                newPos.z = 0;
                _cursorPoint.transform.position = newPos;
                _cursorPoint.transform.GetChild(0).GetComponent<TextMesh>().text = "ID : 0";
                _cursorPoint.transform.localScale = new Vector3(PointSize.x, PointSize.y, 2f);
                cursorPointBehaviour.pid = 0;
                cursorPointBehaviour.oid = 0;
                cursorPointBehaviour.manager = this;
                cursorPointBehaviour.isMouse = true;
                InstantiatedPoints.Add(0, _cursorPoint);
                PointsCount++;
            }
        }
    }

    public void UpdatePointColor(PointBehaviour target) {
        if (Mute)
            target.UpdatePointColor(Color.gray);
        else
            target.UpdatePointColor(target.PointColor);
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

            int pidToFlicker = InstantiatedPoints.ElementAt(flickeringIndex).Key;

            _flickeringPoints.Add(pidToFlicker, InstantiatedPoints.ElementAt(flickeringIndex).Value);

            SendPersonLeft(InstantiatedPoints[pidToFlicker]);
            InstantiatedPoints.ElementAt(flickeringIndex).Value.GetComponent<PointBehaviour>().StartFlickering();
            InstantiatedPoints.Remove(pidToFlicker);
            PointsCount--;

            //Update OIDs
            UpdateOIDs();
        }
    }

    /// <summary>
    /// Recreate flickering point
    /// </summary>
    /// <param name="pid"></param>
    public void StopFlickering(int pid) {

        InstantiatedPoints.Add(pid, _flickeringPoints[pid]);
        PointsCount++;

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

        if (PointsCount <= 0) PointsCount = 0;

        //Create new points
        while (InstantiatedPoints.Count < PointsCount) {
            InstantiatePoint();
        }

        //Remove points
        while (InstantiatedPoints.Count > PointsCount) {
            RemovePoint();
        }
    }

    /// <summary>
    /// Instantiate new point
    /// </summary>
    public void InstantiatePoint(bool isIncorrectDetection = false) {

		_highestPid++;

		if (_highestPid <= 0)
			_highestPid = 1;

		GameObject newPoint = Instantiate(Prefab);

		PointBehaviour newPointBehaviour = newPoint.GetComponent<PointBehaviour>();

		newPointBehaviour.PointColor = Color.HSVToRGB(Random.value, 0.85f, 0.75f);
		newPointBehaviour.manager = this;
		UpdatePointColor(newPointBehaviour);
		newPoint.transform.parent = transform;
		newPoint.transform.localPosition = new Vector3(Random.Range(-0.5f + (PointSize.x / Width), 0.5f - (PointSize.x / Width)), Random.Range(-0.5f + (PointSize.y / Height), 0.5f - (PointSize.y / Height)));
		newPointBehaviour.Speed = Speed;
		newPointBehaviour.pid = _highestPid;
		newPoint.transform.localScale = new Vector3(PointSize.x, PointSize.y, 2f);
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
        }
    }

    /// <summary>
    /// Recompute points OIDs
    /// </summary>
    private void UpdateOIDs() {

        for (int i = 0; i < InstantiatedPoints.Count; i++) {
            if(InstantiatedPoints.ElementAt(i).Key != 0)
                InstantiatedPoints.ElementAt(i).Value.GetComponent<PointBehaviour>().oid = i + 1;
        }

        for (int i = 0; i < _incorrectInstantiatedPoints.Count; i++) {
            _incorrectInstantiatedPoints.ElementAt(i).Value.GetComponent<PointBehaviour>().oid = InstantiatedPoints.Count + i + 1;
        }

    }

    private void ComputeOrthoCamera()
    {
        if (Width <= 0) Width = 500;
        if (Height <= 0) Height = 800;

        Ratio = ((float)Width / (float)Height);

        transform.localScale = new Vector3(Width, Height,1f) / 100;
        GetComponent<Renderer>().material.mainTextureScale = transform.localScale * 1.5f;

        Camera.main.aspect = Ratio;
        Camera.main.orthographicSize = transform.localScale.y / 2;
    }

    public void RemovePoints()
    {
        PointsCount = 0;
        _highestPid = 0;

        foreach (var obj in InstantiatedPoints) {
            SendPersonLeft(obj.Value);
            Destroy(obj.Value);
        }

        InstantiatedPoints.Clear();
    }

    public void ChangeResolution()
    {
        var ratio = (float)Width / (float)Height;
        var newWidth = _width;
        var newHeight = _height;

        newWidth = (int)Mathf.Clamp(Width, WidthLimit.x, WidthLimit.y);
        newHeight = (int)Mathf.Clamp(Height, HeightLimit.x, HeightLimit.y);

        Screen.SetResolution(newWidth, newHeight, false);
    }

    #region OSC Message

    //OSC part

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

    public void InitConnection() {
        if (OSCMaster.Clients.ContainsKey("AugmentaSimulatorOutput")) {
            OSCMaster.RemoveClient("AugmentaSimulatorOutput");
        }
        OSCMaster.CreateClient("AugmentaSimulatorOutput", IPAddress.Parse(OutputIP), OutputPort);
    }

    public void SendSceneUpdated()
    {
        if (Mute) return;

        var msg = new UnityOSC.OSCMessage("/au/scene");
        msg.Append(_frameCounter);
        //Compute point size
        msg.Append(InstantiatedPoints.Count * PointSize.x * PointSize.y);
        msg.Append(PointsCount);
        //Compute average motion
        var velocitySum = Vector3.zero;
        foreach(var element in InstantiatedPoints)
        {
            velocitySum += -element.Value.GetComponent<PointBehaviour>().NormalizedVelocity;
        }
        velocitySum /= InstantiatedPoints.Count;

        msg.Append(velocitySum.x);
        msg.Append(velocitySum.y);
        msg.Append(Width);
        msg.Append(Height);
        msg.Append(100);

        OSCMaster.Clients["AugmentaSimulatorOutput"].Send(msg);
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
        var worldToViewPort = Camera.main.WorldToViewportPoint(obj.transform.position);
        worldToViewPort = new Vector3(worldToViewPort.x, Mathf.Abs(worldToViewPort.y - 1), worldToViewPort.z); //Switch from bot-left (0;0) to top-left(0;0)

        msg.Append(behaviour.pid);

        //oid
        msg.Append(behaviour.oid);

        msg.Append((int)behaviour.Age);
        //centroid
        msg.Append(worldToViewPort.x);
        msg.Append(worldToViewPort.y);
        //Velocity
        var normalizedVelocity = behaviour.NormalizedVelocity;
        msg.Append(-normalizedVelocity.x);
        msg.Append(-normalizedVelocity.y);
        msg.Append((float)behaviour.pid);

        //Bounding
        msg.Append(worldToViewPort.x - PointSize.x / 2);
        msg.Append(worldToViewPort.y - PointSize.y / 2);

        msg.Append(PointSize.x);
        msg.Append(PointSize.y);

        msg.Append(worldToViewPort.x);
        msg.Append(worldToViewPort.y);
        msg.Append(0.5f);

        OSCMaster.Clients["AugmentaSimulatorOutput"].Send(msg);
    }

    #endregion
}
