using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FakePointManager : MonoBehaviour {

    [Header("Network settings")]
    public string TargetIP;
    public int TargetPort;

    [Header("Area settings")]
    public int Width;
    public int Height;
    public float Ratio;

    [Header("Points settings")]
    public int NbPoints;
    public float PointSize;
    public float Speed;

    public bool UseBrownianMotion;
    public GameObject Prefab;
    private int InstanceNumber;

    private int _highestPid;

    public static Dictionary<int, GameObject> InstanciatedPoints;

    private float _oldSpeed;
    private float _oldSize;

    private GameObject CursorPoint;
    public bool CanMoveCursorPoint;

    void Start () {
        InstanciatedPoints = new Dictionary<int, GameObject>();
        CanMoveCursorPoint = true;
        _highestPid = 0;
        _oldSpeed = Speed;
        _oldSize = PointSize;

        Physics2D.IgnoreLayerCollision(9, 9);
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
                obj.Value.transform.localScale = new Vector3(PointSize, PointSize, PointSize);
            }
            _oldSize = PointSize;
        }

        if (Input.GetMouseButton(1))
        {
            if (CursorPoint != null)
            {
                NbPoints--;
                InstanciatedPoints.Remove(0);
                Destroy(CursorPoint);
            }
        }
        if (Input.GetMouseButton(0) && CanMoveCursorPoint)
        {
            if (CursorPoint == null)
            {
                CursorPoint = Instantiate(Prefab);

                var newPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                newPos.z = 0;
                newPos.x = Mathf.Clamp(newPos.x, 0.05f, 0.95f);
                newPos.y = Mathf.Clamp(newPos.y, 0.05f, 0.95f);
                newPos = Camera.main.ViewportToWorldPoint(newPos);
                newPos.z = 0;
                CursorPoint.transform.position = newPos;
                CursorPoint.transform.GetChild(0).GetComponent<TextMesh>().text = "ID : 0";
                CursorPoint.transform.localScale = new Vector3(PointSize, PointSize, PointSize);
                CursorPoint.GetComponent<FakePointBehaviour>().enabled = false;
                CursorPoint.GetComponent<FakePointBehaviour>().pid = 0;
                CursorPoint.GetComponent<FakePointBehaviour>().manager = this;

                InstanciatedPoints.Add(0, CursorPoint);
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
            var newPoint = Instantiate(Prefab);//, this.transform);
            newPoint.GetComponent<FakePointBehaviour>().Speed = Speed;
            newPoint.GetComponent<FakePointBehaviour>().pid = _highestPid;
            newPoint.GetComponent<FakePointBehaviour>().manager = this;
            newPoint.transform.localScale = new Vector3(PointSize, PointSize, PointSize);

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

        Ratio = (Width / (float)Height);

        transform.localScale = new Vector3(Width, Height,0.01f) / 100;

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

    public void OnMouseDrag()
    {
        if (CursorPoint == null || !CanMoveCursorPoint) return;

        var newPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        newPos.z = 0;
        newPos.x = Mathf.Clamp(newPos.x, 0.05f, 0.95f);
        newPos.y = Mathf.Clamp(newPos.y, 0.05f, 0.95f);
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
        msg.Append(0);
        msg.Append(NbPoints);
        msg.Append(0.5f);
        msg.Append(0.5f);
        msg.Append(Width);
        msg.Append(Height);

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

        msg.Append(behaviour.pid);

        //Bounding
        msg.Append(Camera.main.WorldToViewportPoint(obj.transform.position).x - PointSize);
        msg.Append(Camera.main.WorldToViewportPoint(obj.transform.position).y - PointSize);
        msg.Append(PointSize);
        msg.Append(PointSize);

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

        msg.Append(behaviour.pid);

        //Bounding
        msg.Append(worldToViewPort.x - PointSize/10);
        msg.Append(1 -(worldToViewPort.y + PointSize/10));
        if (Width > Height)
        {
            msg.Append(PointSize / 10);
            msg.Append((PointSize / 10) * ((float)Width / (float)Height));
        }
        else
        {
            msg.Append((PointSize / 10) * ((float)Height / (float)Width));
            Debug.Log("AH");
            msg.Append(PointSize / 10);
        }
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

        msg.Append(behaviour.pid);

        //Bounding
        msg.Append(Camera.main.WorldToViewportPoint(obj.transform.position).x - PointSize);
        msg.Append(Camera.main.WorldToViewportPoint(obj.transform.position).y - PointSize);
        msg.Append(PointSize);
        msg.Append(PointSize);

        msg.Append(0.0f);
        msg.Append(0.0f);
        msg.Append(0.0f);

        OSCMaster.sendMessage(msg, TargetIP, TargetPort);
    }



}
