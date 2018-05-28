using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FakePointManager : MonoBehaviour {

    [Header("Network settings")]
    public string TargetIP;
    public int TargetPort;

    [Header("Area settings")]
    public int Width;
    public int WidthMin;
    public int Height;
    public int HeightMin;
    public float Ratio;

    [Header("Points settings")]
    public int NbPoints;
    public Vector2 PointSize;
    public float Speed;

    public bool UseBrownianMotion;
    public GameObject Prefab;
    private int InstanceNumber;

    private int _highestPid;

    public static Dictionary<int, GameObject> InstanciatedPoints;

    private float _oldSpeed;
    private Vector2 _oldSize;

    private GameObject CursorPoint;
    public bool CanMoveCursorPoint;

    void Start () {
        InstanciatedPoints = new Dictionary<int, GameObject>();
        CanMoveCursorPoint = true;
        _highestPid = 0;
        _oldSpeed = Speed;
        _oldSize = PointSize;
        OSCMaster.instance.messageAvailable += HandleMessageAvailable;
        Physics2D.IgnoreLayerCollision(9, 9);
    }

    private void HandleMessageAvailable(UnityOSC.OSCMessage message)
    {
        string[] addSplit = message.Address.Split(new char[] { '/' });
        if (addSplit[1] == "yo")
        {
            var msg = new UnityOSC.OSCMessage("/wassup");
            msg.Append(TargetIP + " " + TargetPort);
            Debug.Log(message.Data);
            OSCMaster.sendMessage(msg, message.Data[0].ToString(), int.Parse(message.Data[1].ToString()));
        }
    }

    void Update () {
        if (InstanceNumber != NbPoints)
            InstantiatePoint();

        if (_oldSpeed != Speed)
        {
            foreach (var obj in InstanciatedPoints)
            {
                obj.Value.GetComponent<FakePointBehaviour>().Speed = Speed;
                obj.Value.GetComponent<FakePointBehaviour>().UpdateSpeed();
            }
            _oldSpeed = Speed;
        }

        if (_oldSize!= PointSize)
        {
            foreach (var obj in InstanciatedPoints)
            {
                obj.Value.transform.localScale = new Vector3(PointSize.x, PointSize.y, 2f);
            }
            _oldSize = PointSize;
        }

        if (Input.GetMouseButton(1))
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
        if (Input.GetMouseButton(0) && CanMoveCursorPoint)
        {
            if (CursorPoint == null)
            {
                CursorPoint = Instantiate(Prefab);
                CursorPoint.GetComponent<MeshRenderer>().material.SetColor("_PointColor", Color.HSVToRGB(Random.value, 0.85f, 0.75f));
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
                CursorPoint.GetComponent<FakePointBehaviour>().enabled = false;
                CursorPoint.GetComponent<FakePointBehaviour>().pid = 0;
                CursorPoint.GetComponent<FakePointBehaviour>().manager = this;
     
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

    public void InstantiatePoint()
    {
        if (NbPoints <= 0) NbPoints = 0;

        if (InstanceNumber < NbPoints)
        {
            _highestPid++;
            var newPoint = Instantiate(Prefab);
            
            newPoint.GetComponent<MeshRenderer>().material.SetColor("_PointColor", Color.HSVToRGB(Random.value, 0.75f, 0.75f));
            newPoint.transform.parent = transform;
            newPoint.GetComponent<FakePointBehaviour>().Speed = Speed;
            newPoint.GetComponent<FakePointBehaviour>().pid = _highestPid;
            newPoint.GetComponent<FakePointBehaviour>().manager = this;
            newPoint.transform.localScale = new Vector3(PointSize.x, PointSize.y, 2f);

            InstanciatedPoints.Add(_highestPid, newPoint);
            InstanceNumber++;

            SendPersonEntered(InstanciatedPoints[_highestPid]);
        }

        if (InstanceNumber > NbPoints)
        {
            SendPersonLeft(InstanciatedPoints[_highestPid]);
            Destroy(InstanciatedPoints[_highestPid]);
            InstanciatedPoints.Remove(_highestPid);
            _highestPid--;
            InstanceNumber--;

        }
    }

    private void ComputeOrthoCamera()
    {
        if (Width <= 0) Width = 1;
        if (Height <= 0) Height = 1;

        Ratio = ((float)Width / (float)Height);

        transform.localScale = new Vector3(Width, Height,1f) / 100;

        Camera.main.aspect = Ratio;
        Camera.main.orthographicSize = transform.localScale.y / 2;
    }

    public void Clear()
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
        var newWidth = Mathf.Clamp(Width, WidthMin, Screen.width);
        var newHeight = Mathf.Clamp(Height, HeightMin, Screen.height);
        var ratio = Width / Height;

        if (Height > Width)
            Screen.SetResolution(newWidth, newHeight * ratio, false);
        else
            Screen.SetResolution(newWidth * ratio, newHeight, false);
    }

    public void OnMouseDrag()
    {
        if (CursorPoint == null || !CanMoveCursorPoint) return;

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
        * Augmenta OSC Protocol :

            /au/personWillLeave/ args0 arg1 ... argn
            /au/personUpdated/   args0 arg1 ... argn
            /au/personEntered/   args0 arg1 ... argn

            where args are :

            0: pid (int)
            1: oid (int)
            2: age (int)
            3: centroid.x (float)
            4: centroid.y (float)
            5: velocity.x (float)
            6: velocity.y (float)
            7: depth (float)
            8: boundingRect.x (float)
            9: boundingRect.y (float)
            10: boundingRect.width (float)
            11: boundingRect.height (float)
            12: highest.x (float)
            13: highest.y (float)
            14: highest.z (float)
            15:
            16:
            17:
            18:
            19:
            20+ : contours (if enabled)

            /au/scene/   args0 arg1 ... argn

            0: currentTime (int)
            1: percentCovered (float)
            2: numPeople (int)
            3: averageMotion.x (float)
            4: averageMotion.y (float)
            5: scene.width (int)
            6: scene.height (int)
    */

    public void SendSceneUpdated()
    {
        var msg = new UnityOSC.OSCMessage("/au/scene/");
        msg.Append(0);
        msg.Append(0f);
        msg.Append(NbPoints);
        msg.Append(0.5f);
        msg.Append(0.5f);
        msg.Append(Width);
        msg.Append(Height);
        msg.Append(0f);

        OSCMaster.sendMessage(msg, TargetIP, TargetPort);
    }

    public void SendPersonEntered(GameObject obj)
    {
        var msg = new UnityOSC.OSCMessage("/au/personEntered/");
        var behaviour = obj.GetComponent<FakePointBehaviour>();

        msg.Append(behaviour.pid);
        msg.Append(behaviour.pid);
        msg.Append(behaviour.pid);

        msg.Append(Camera.main.WorldToViewportPoint(obj.transform.position).x);
        msg.Append(Camera.main.WorldToViewportPoint(obj.transform.position).y);

        //Velocity
        msg.Append(behaviour.direction.x * behaviour.Speed);
        msg.Append(behaviour.direction.y * behaviour.Speed);

        msg.Append((float)behaviour.pid);

        //Bounding
        msg.Append(Camera.main.WorldToViewportPoint(obj.transform.position).x - PointSize.x);
        msg.Append(Camera.main.WorldToViewportPoint(obj.transform.position).y - PointSize.y);
        msg.Append(PointSize.x);
        msg.Append(PointSize.y);

        msg.Append(0.0f);
        msg.Append(0.0f);
        msg.Append(0.0f);

        OSCMaster.sendMessage(msg, TargetIP, TargetPort);
    }

    public void SendPersonUpdated(GameObject obj)
    {
        var msg = new UnityOSC.OSCMessage("/au/personUpdated/");
        var behaviour = obj.GetComponent<FakePointBehaviour>();
        var worldToViewPort = Camera.main.WorldToViewportPoint(obj.transform.position);

        msg.Append(behaviour.pid);
        msg.Append(behaviour.pid);
        msg.Append(behaviour.pid);

        msg.Append(1 - worldToViewPort.x);
        msg.Append(worldToViewPort.y);
        //Velocity
        msg.Append(behaviour.direction.x * behaviour.Speed);
        msg.Append(behaviour.direction.y * behaviour.Speed);

        msg.Append((float)behaviour.pid);

        //Bounding
        msg.Append(worldToViewPort.x - PointSize.x/2);
        msg.Append(1 - (worldToViewPort.y + PointSize.y/2));

        msg.Append(PointSize.x);
        msg.Append(PointSize.y);

        msg.Append(0.0f);
        msg.Append(0.0f);
        msg.Append(0.0f);

        OSCMaster.sendMessage(msg, TargetIP, TargetPort);
    }

    public void SendPersonLeft(GameObject obj)
    {
        var msg = new UnityOSC.OSCMessage("/au/personWillLeave/");
        var behaviour = obj.GetComponent<FakePointBehaviour>();

        msg.Append(behaviour.pid);
        msg.Append(behaviour.pid);
        msg.Append(behaviour.pid);

        msg.Append(Camera.main.WorldToViewportPoint(obj.transform.position).x);
        msg.Append(Camera.main.WorldToViewportPoint(obj.transform.position).y);

        //Velocity
        msg.Append(behaviour.direction.x * behaviour.Speed);
        msg.Append(behaviour.direction.y * behaviour.Speed);

        msg.Append((float)behaviour.pid);

        //Bounding
        msg.Append(Camera.main.WorldToViewportPoint(obj.transform.position).x - PointSize.x);
        msg.Append(Camera.main.WorldToViewportPoint(obj.transform.position).y - PointSize.y);
        msg.Append(PointSize.x);
        msg.Append(PointSize.y);

        msg.Append(0.0f);
        msg.Append(0.0f);
        msg.Append(0.0f);

        OSCMaster.sendMessage(msg, TargetIP, TargetPort);
    }
}
