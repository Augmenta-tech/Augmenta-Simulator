using System.Collections;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using UnityEngine;


public class FakePointManager : MonoBehaviour {

    [Header("Network settings")]
    public string TargetIP;
    public int TargetPort;
    public bool Mute;

    [Header("Area settings")]
    public int Width;
    public Vector2 WidthLimit;
    public int Height;
    public Vector2 HeightLimit;
    public float Ratio;

    [Header("Points settings")]
    public int NbPoints;
    public Vector2 PointSize;
    public float Speed;

    private int _frameCounter;
    public GameObject Prefab;
    private int InstanceNumber;

    private int _highestPid;

    public static Dictionary<int, GameObject> InstanciatedPoints;

    private float _oldSpeed;
    private Vector2 _oldSize;

    private float _oldWidth, _oldHeight;

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
        Debug.Log("Received : " + message.Address);
        foreach(var data in message.Data)
        {
            Debug.Log("Data : " + data.ToString());
        }

        string[] addSplit = message.Address.Split(new char[] { '/' });
        if (addSplit[1] == "yo")
        {
            var msg = new UnityOSC.OSCMessage("/wassup");
            msg.Append(Network.player.ipAddress);
            msg.Append(ShowNetworkInterfaces());
            Debug.Log("Answered : /wassup " + Network.player.ipAddress + " " + ShowNetworkInterfaces());
            OSCMaster.sendMessage(msg, message.Data[0].ToString(), int.Parse(message.Data[1].ToString()));
        }

        if (addSplit[1] == "connect")
        {
            TargetIP = message.Data[1].ToString();
            TargetPort = int.Parse(message.Data[2].ToString());
            Debug.Log("TargetIp : " + message.Data[1].ToString() + ":" + message.Data[2].ToString());
        }

        if (addSplit[1] == "info")
        {
            var msg = new UnityOSC.OSCMessage("/info");
            msg.Append(Network.player.ipAddress);
            msg.Append("Simulator");
            msg.Append(ShowNetworkInterfaces());
            msg.Append("2.1");
            msg.Append("SIMULATOR");
            msg.Append("SIMULATOR");
            msg.Append("SIMULATOR");
            OSCMaster.sendMessage(msg, message.Data[0].ToString(), int.Parse(message.Data[1].ToString()));
        }
    }

    public string ShowNetworkInterfaces()
    {
        
        IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
        NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

        string mac = "";
        foreach (NetworkInterface adapter in nics)
        {
            PhysicalAddress address = adapter.GetPhysicalAddress();
            byte[] bytes = address.GetAddressBytes();
            for (int i = 0; i < bytes.Length; i++)
            {
                mac = string.Concat(mac + (string.Format("{0}", bytes[i].ToString("X2"))));
                if (i != bytes.Length - 1)
                {
                    mac = string.Concat(mac + ":");
                }
            }
            mac += "\n";
            Debug.Log("Mac : " + mac);
            return mac;
        }
        return null;
    }

    void Update () {

        _frameCounter++; 

        if (_oldWidth != Width || _oldHeight != Height)
        {
            _oldWidth = Width;
            _oldHeight = Height;
            ChangeResolution();
        }

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
                CursorPoint.GetComponent<FakePointBehaviour>().pid = 0;
                CursorPoint.GetComponent<FakePointBehaviour>().manager = this;
                CursorPoint.GetComponent<FakePointBehaviour>().isMouse = true;
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
            InstanceNumber++;

            if (_highestPid <= 0)
                _highestPid = 1;

            var newPoint = Instantiate(Prefab);
            
            newPoint.GetComponent<MeshRenderer>().material.SetColor("_PointColor", Color.HSVToRGB(Random.value, 0.75f, 0.75f));
            newPoint.transform.parent = transform;
            newPoint.GetComponent<FakePointBehaviour>().Speed = Speed;
            newPoint.GetComponent<FakePointBehaviour>().pid = _highestPid;
            newPoint.GetComponent<FakePointBehaviour>().manager = this;
            newPoint.transform.localScale = new Vector3(PointSize.x, PointSize.y, 2f);

            InstanciatedPoints.Add(_highestPid, newPoint);

          
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
        var newWidth = Width;
        var newHeight = Height;

        newWidth = (int)Mathf.Clamp(Width, WidthLimit.x, WidthLimit.y);
        newHeight = (int)Mathf.Clamp(Height, HeightLimit.x, HeightLimit.y);

        Screen.SetResolution(newWidth, newHeight, false);
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

        var msg = new UnityOSC.OSCMessage("/au/scene/");
        msg.Append(_frameCounter);
        //Compute point size
        msg.Append(InstanceNumber * PointSize.x * PointSize.y);
        msg.Append(NbPoints);
        //Compute average motion
        var velocitySum = Vector3.zero;
        foreach(var element in InstanciatedPoints)
        {
            velocitySum += -element.Value.GetComponent<FakePointBehaviour>().NormalizedVelocity;
        }
        velocitySum /= InstanciatedPoints.Count;

        msg.Append(velocitySum.x);
        msg.Append(velocitySum.y);
        msg.Append(Width);
        msg.Append(Height);
        msg.Append(100f);

        OSCMaster.sendMessage(msg, TargetIP, TargetPort);
    }

    public void SendPersonEntered(GameObject obj)
    {
        SendAugmentaMessage("/au/personEntered/", obj);
    }

    public void SendPersonUpdated(GameObject obj)
    {
        SendAugmentaMessage("/au/personUpdated/", obj);
    }

    public void SendPersonLeft(GameObject obj)
    {
        SendAugmentaMessage("/au/personWillLeave/", obj);
    }


    public void SendAugmentaMessage(string address, GameObject obj)
    {
        if (Mute) return;

        var msg = new UnityOSC.OSCMessage(address);
        var behaviour = obj.GetComponent<FakePointBehaviour>();
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

        OSCMaster.sendMessage(msg, TargetIP, TargetPort);
    }
    
}
