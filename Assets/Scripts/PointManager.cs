using System.Collections;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Net;

public class PointManager : MonoBehaviour {

    [Header("Area settings")]
    private int _width = 1280;
    public int Width
    {
        get
        {
            return _width;
        }
        set
        {
            this._width = value;
            ChangeResolution();
        }
    }
    public Vector2 WidthLimit;

    private int _height = 800;
    public int Height
    {
        get
        {
            return _height;
        }
        set
        {
            this._height = value;
            ChangeResolution();
        }
    }
    public Vector2 HeightLimit;
    public float Ratio;

    [Header("Points settings")]
    public bool _mute;
    public bool Mute
    {
        get
        {
            return _mute;
        }
        set
        {
            this._mute = value;
            if (InstanciatedPoints != null)
            {
                foreach (var point in InstanciatedPoints.Values)
                    ChangePointColor(point.GetComponent<PointBehaviour>());
            }
        }
    }
    public int NbPoints;
    private Vector2 _pointSize;
    public Vector2 PointSize
    {
        get
        {
            return _pointSize;
        }
        set
        {
            this._pointSize = value;

            if (InstanciatedPoints == null) return;

            foreach (var obj in InstanciatedPoints)
            {
                obj.Value.transform.localScale = new Vector3(PointSize.x, PointSize.y, 2f);
            }
        }
    }
    private float _speed;
    public float Speed
    {
        get
        {
            return _speed;
        }
        set
        {
            this._speed = value;

            if (InstanciatedPoints == null) return;

            foreach (var obj in InstanciatedPoints)
            {
                obj.Value.GetComponent<PointBehaviour>().Speed = Speed;
            }
        }
    }

    private int _frameCounter;
    public GameObject Prefab;
    private int InstanceNumber;

    private bool _clickStartedOutsideUI;
    private int _highestPid;
    public static Dictionary<int, GameObject> InstanciatedPoints;

    private GameObject CursorPoint;
    public bool CanMoveCursorPoint;

    void Start () {
        InstanciatedPoints = new Dictionary<int, GameObject>();
        CanMoveCursorPoint = true;
        _highestPid = 0;
        Physics2D.IgnoreLayerCollision(9, 9);
        ChangeResolution();
    }

    public void CheckInputs()
    {
        if (Input.GetKeyUp(KeyCode.M))
            Mute = !Mute;

        if (Input.GetKeyUp(KeyCode.Escape))
            Application.Quit();

        if (Input.GetKeyUp(KeyCode.R))
            RemovePoints();
    }

    void Update () {

        CheckInputs();

        _frameCounter++; 

        while (InstanceNumber != NbPoints)
            InstantiatePoint();

        //"Weird behaviour "fix"
        if (NbPoints == 0)
            CanMoveCursorPoint = true;

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            _clickStartedOutsideUI = true;
        }
        if(Input.GetMouseButtonUp(0))
        {
            _clickStartedOutsideUI = false;
        }

        if (Input.GetMouseButton(1) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (CursorPoint != null)
            {
                NbPoints--;
                InstanceNumber--;
                SendPersonLeft(InstanciatedPoints[0]);
                InstanciatedPoints.Remove(0);
                Destroy(CursorPoint);
            }
        }
        else if (Input.GetMouseButton(0) && CanMoveCursorPoint && !EventSystem.current.IsPointerOverGameObject())
        {
            if (CursorPoint == null)
            {
                CursorPoint = Instantiate(Prefab);
                CursorPoint.GetComponent<PointBehaviour>().PointColor = Color.HSVToRGB(Random.value, 0.85f, 0.75f);
//                CursorPoint.GetComponent<MeshRenderer>().material.SetColor("_PointColor", CursorPoint.GetComponent<PointBehaviour>().PointColor);
                ChangePointColor(CursorPoint.GetComponent<PointBehaviour>());
                CursorPoint.transform.parent = transform;
                var newPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                newPos.z = 0;
                newPos.x = Mathf.Clamp(newPos.x, 0.05f, 0.95f);
                newPos.y = Mathf.Clamp(newPos.y, 0.05f, 0.95f);
                newPos = Camera.main.ViewportToWorldPoint(newPos);
                newPos.z = 0;
                CursorPoint.transform.position = newPos;
                CursorPoint.transform.GetChild(0).GetComponent<TextMesh>().text = "ID : 0";
                CursorPoint.transform.localScale = new Vector3(PointSize.x, PointSize.y, 2f);
                CursorPoint.GetComponent<PointBehaviour>().pid = 0;
                CursorPoint.GetComponent<PointBehaviour>().manager = this;
                CursorPoint.GetComponent<PointBehaviour>().isMouse = true;
                InstanciatedPoints.Add(0, CursorPoint);
                InstanceNumber++;
                NbPoints++;
            }
        }

        ComputeOrthoCamera();

        SendSceneUpdated();

        foreach(var obj in InstanciatedPoints)
            SendPersonUpdated(obj.Value);
	}

    public void ChangePointColor(PointBehaviour target) {
        if (Mute)
            target.ChangePointColor(Color.gray);
        else
            target.ChangePointColor(target.PointColor);
    }

    public void InstantiatePoint()
    {
        if (NbPoints <= 0) NbPoints = 0;
        

        if (InstanceNumber < NbPoints)
        {
            _highestPid++;
            InstanceNumber++;

            if (_highestPid <= 0)
                _highestPid = 1;

            var newPoint = Instantiate(Prefab);

            newPoint.GetComponent<PointBehaviour>().PointColor = Color.HSVToRGB(Random.value, 0.85f, 0.75f);
            newPoint.GetComponent<PointBehaviour>().manager = this;
            ChangePointColor(newPoint.GetComponent<PointBehaviour>());
            newPoint.transform.parent = transform;
            newPoint.transform.localPosition = new Vector3(Random.Range(-0.5f + (PointSize.x / Width) , 0.5f - (PointSize.x / Width)), Random.Range(-0.5f + (PointSize.y / Height), 0.5f - (PointSize.y / Height)));
            newPoint.GetComponent<PointBehaviour>().Speed = Speed;
            newPoint.GetComponent<PointBehaviour>().pid = _highestPid;
            newPoint.transform.localScale = new Vector3(PointSize.x, PointSize.y, 2f);

            InstanciatedPoints.Add(_highestPid, newPoint);

          
            SendPersonEntered(InstanciatedPoints[_highestPid]);
        }

        if (InstanceNumber > NbPoints)
        {
            if (InstanciatedPoints.ContainsKey(_highestPid))
            {
                SendPersonLeft(InstanciatedPoints[_highestPid]);
                Destroy(InstanciatedPoints[_highestPid]);
                InstanciatedPoints.Remove(_highestPid);
                _highestPid--;
                InstanceNumber--;
            }
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
        NbPoints = 0;
        InstanceNumber = 0;
        _highestPid = 0;

        foreach (var obj in InstanciatedPoints) {
            SendPersonLeft(obj.Value);
            Destroy(obj.Value);
        }

        InstanciatedPoints.Clear();
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

    public void OnMouseDrag()
    {
        if (CursorPoint == null || !CanMoveCursorPoint || !_clickStartedOutsideUI) return;

        var newPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        newPos.z = 0;

            newPos.x = Mathf.Clamp(newPos.x, 0f, 1f);
            newPos.y = Mathf.Clamp(newPos.y, 0f, 1f);

        newPos = Camera.main.ViewportToWorldPoint(newPos);
        newPos.z = 0;
        CursorPoint.transform.position = newPos;
    }


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

    public void SendSceneUpdated()
    {
        if (Mute) return;

        var msg = new UnityOSC.OSCMessage("/au/scene");
        msg.Append(_frameCounter);
        //Compute point size
        msg.Append(InstanceNumber * PointSize.x * PointSize.y);
        msg.Append(NbPoints);
        //Compute average motion
        var velocitySum = Vector3.zero;
        foreach(var element in InstanciatedPoints)
        {
            velocitySum += -element.Value.GetComponent<PointBehaviour>().NormalizedVelocity;
        }
        velocitySum /= InstanciatedPoints.Count;

        msg.Append(velocitySum.x);
        msg.Append(velocitySum.y);
        msg.Append(Width);
        msg.Append(Height);
        msg.Append(100);

        AugmentaOSCHandler.Instance.SendMessage("AugmentaSimulatorOutput", msg);
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

        if (CursorPoint != null)
            msg.Append(behaviour.pid);
        else
            msg.Append(behaviour.pid - 1);

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

        AugmentaOSCHandler.Instance.SendMessage("AugmentaSimulatorOutput", msg);
    }
    
}
