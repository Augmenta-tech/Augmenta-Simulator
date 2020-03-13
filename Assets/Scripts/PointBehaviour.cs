using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointBehaviour : MonoBehaviour {

    public PointManager manager;
    public long Age;

    public TextMesh PointInfoText;
    public Transform Point;
    public Transform VelocityVisualizer;
    public new Rigidbody rigidbody;

    public float VelocityThickness;

    private float _speed;
    public float Speed {
        get { return _speed; }
        set { _speed = value;
            rigidbody.velocity = direction * Speed;
        }
    }

    public int pid;
    public int oid;
    public Vector3 direction;
    public bool isMovedByMouse;
    public Color PointColor;

    public Vector3 NormalizedVelocity;

    public Vector3 _oldPosition;

    public float noiseIntensity = 0;

    public bool isIncorrectDetection = false;
    public bool isFlickering = false;

    private float timer = 0;

    private Ray ray;
    private RaycastHit raycastHit;

	#region MonoBehaviour Implementation

	void Start () {
        direction = Random.onUnitSphere;
        direction.z = 0;

        //Get velocity
        if (isIncorrectDetection) {
            rigidbody.velocity = Vector3.zero;
            Speed = 0.0f;
        } else {
            var rndVelocity = direction * Speed;
            rigidbody.velocity = rndVelocity;
        }

        VelocityVisualizer.localScale = Vector3.zero;

        timer = 0;
    }

    private void Update() {
        //Handle IncorrectDetection points
        if (isIncorrectDetection) {
            timer += Time.deltaTime;
            if (timer > manager.IncorrectDetectionDuration) {
                manager.RemoveIncorrectPoint(pid);
            }
        }

        //Handle flickering
        if (isFlickering) {
            timer += Time.deltaTime;
            if (timer > manager.PointFlickeringDuration) {
                manager.StopFlickering(pid);
            }
        }

        Age++;

        //Update position
        var newPos = transform.position + Random.Range(-noiseIntensity, noiseIntensity) * Vector3.right + Random.Range(-noiseIntensity, noiseIntensity) * Vector3.up;

        newPos.x = Mathf.Clamp(newPos.x, -(manager.Width + manager.PointSize.x) * 0.5f, (manager.Width - manager.PointSize.x) * 0.5f);
        newPos.y = Mathf.Clamp(newPos.y, -(manager.Height + manager.PointSize.y) * 0.5f, (manager.Height - manager.PointSize.y) * 0.5f);

        transform.position = newPos;

        //udpate text
        PointInfoText.text = "PID : " + pid + '\n' + '\n' + "OID : " + oid;
    }

    private void FixedUpdate() {

        ComputeNormalizedVelocity();
        //Update velocity
        float angle = Mathf.Atan2(NormalizedVelocity.y, NormalizedVelocity.x) * 180 / Mathf.PI;
        if (float.IsNaN(angle))
            return;

        VelocityVisualizer.localRotation = Quaternion.Euler(new Vector3(0, 0, -angle + 90));

        VelocityVisualizer.localScale = new Vector3(VelocityThickness, NormalizedVelocity.magnitude, VelocityThickness);
    }

    public void OnMouseDrag() {

        ray = manager.camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out raycastHit, 100.0f, manager.areaLayer)) {

            transform.position = new Vector3(raycastHit.point.x, raycastHit.point.y, 0);
        }
    }

    #endregion

    private void ComputeNormalizedVelocity()
    {
        var worldToViewPort = Camera.main.WorldToViewportPoint(transform.position);
        //Switch from bot-left (0;0) to top-left(0;0)
        worldToViewPort = new Vector3(worldToViewPort.x, Mathf.Abs(worldToViewPort.y - 1), worldToViewPort.z);
        var oldPositionNormalized = Camera.main.WorldToViewportPoint(_oldPosition);
        //Switch from bot-left (0;0) to top-left(0;0)
        oldPositionNormalized = new Vector3(oldPositionNormalized.x, Mathf.Abs(oldPositionNormalized.y - 1), oldPositionNormalized.z);

        _oldPosition = transform.position;

        NormalizedVelocity = (oldPositionNormalized - worldToViewPort) / Time.deltaTime;
    }

    public void UpdatePointColor(Color color)
    {
        Point.GetComponent<MeshRenderer>().material.SetColor("_BorderColor", color);
    }

    public void StartFlickering() {

        timer = 0;
        HidePoint();
        isFlickering = true;
    }

    public void HidePoint() {

        PointInfoText.gameObject.SetActive(false);
        Point.gameObject.SetActive(false);
        VelocityVisualizer.gameObject.SetActive(false);
    }

    public void ShowPoint() {

        PointInfoText.gameObject.SetActive(true);
        Point.gameObject.SetActive(true);
        VelocityVisualizer.gameObject.SetActive(true);
    }
}
