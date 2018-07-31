using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointBehaviour : MonoBehaviour {

    public PointManager manager;
    public long Age;

    public TextMesh PointInfoText;
    public Transform Point;
    public Transform VelocityVisualizer;
    public float VelocityThickness;

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
            GetComponent<Rigidbody2D>().velocity = direction * Speed;
        }
    }
    public int pid;
    public int oid;
    public Vector3 direction;
    public bool isMouse;
    public Color PointColor;

    public Vector3 NormalizedVelocity;

    public Vector3 _oldPosition;

	void Start () {
        direction = Random.onUnitSphere;
        direction.z = 0;

        var rndVelocity = direction * Speed;
        if (!isMouse) 
            GetComponent<Rigidbody2D>().velocity = rndVelocity;
        else
            GetComponent<Rigidbody2D>().isKinematic = true;

        //GetComponent<Rigidbody2D>().velocity = direction * Speed;

        //Point.GetComponent<MeshRenderer>().material.SetColor("_BorderColor", PointColor);
    }

    public void UpdateOldPosition()
    {
        _oldPosition = transform.position;
    }

    private void ComputeNormalizedVelocity()
    {
        var worldToViewPort = Camera.main.WorldToViewportPoint(transform.position);
        //Switch from bot-left (0;0) to top-left(0;0)
        worldToViewPort = new Vector3(worldToViewPort.x, Mathf.Abs(worldToViewPort.y - 1), worldToViewPort.z);
        var oldPositionNormalized = Camera.main.WorldToViewportPoint(_oldPosition);
        //Switch from bot-left (0;0) to top-left(0;0)
        oldPositionNormalized = new Vector3(oldPositionNormalized.x, Mathf.Abs(oldPositionNormalized.y - 1), oldPositionNormalized.z);

        _oldPosition = transform.position;

        //Debug.Log("Velocity : " + (oldPositionNormalized - worldToViewPort));
        NormalizedVelocity = oldPositionNormalized - worldToViewPort;
        //NormalizedVelocity = GetComponent<Rigidbody2D>().velocity
    }

    private void Update()
    {
        Age++;
      
        //Compute velocity

        if (isMouse)
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            Speed = 0.0f;
        }

        //Screen out
        var actualPos = Camera.main.WorldToViewportPoint(transform.position);

        var newPos = transform.position;

        if (actualPos.x < 0.00f || actualPos.x > 1f || actualPos.y < 0.0f || actualPos.y > 1f)
            newPos = Vector3.zero;

        transform.position = newPos;

        ComputeNormalizedVelocity();
        if (manager == null)
            return;
        //Update bouding box
       // Point.transform.localScale = new Vector3(manager.Width* manager.transform.localScale.x, manager.Height * manager.transform.localScale.y, 0.1f);

        var oid = pid;
        //if (!isMouse)
        //    oid -= 1;

        //udpate text
        PointInfoText.text = "PID : " + pid + '\n' + '\n'  + "OID : " + oid;

        //Update velocity
        float angle = Mathf.Atan2(NormalizedVelocity.y, NormalizedVelocity.x) * 180 / Mathf.PI;
        if (float.IsNaN(angle))
            return;


        //Debug.Log("Scale : " + NormalizedVelocity.magnitude * 100);

        VelocityVisualizer.localRotation = Quaternion.Euler(new Vector3(0, 0, -angle + 90));
        
        //111
        VelocityVisualizer.localScale = new Vector3(VelocityThickness, NormalizedVelocity.magnitude * 100, VelocityThickness);
    }

    private void OnMouseDown()
    {
        manager.CanMoveCursorPoint = false;
    }

    private void OnMouseDrag()
    {
        var newPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        newPos.z = 0;
        newPos.x = Mathf.Clamp(newPos.x, 0.05f, 0.95f);
        newPos.y = Mathf.Clamp(newPos.y, 0.05f, 0.95f);
        newPos = Camera.main.ViewportToWorldPoint(newPos);
        newPos.z = 0;
        transform.position = newPos;
    }

    private void OnMouseUp()
    {
        manager.CanMoveCursorPoint = true;
    }

    public void ChangePointColor(Color col)
    {
        Point.GetComponent<MeshRenderer>().material.SetColor("_BorderColor", col);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.name == "Left" || collision.name == "Right")
        {
            direction = new Vector2(-direction.x , direction.y);
        }
        if(collision.name == "Top" || collision.name == "Bot")
        {
            direction = new Vector2(direction.x, -direction.y);
        }

        GetComponent<Rigidbody2D>().velocity = direction * Speed;
    }
}
