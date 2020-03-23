using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointBehaviour : MonoBehaviour {

    [Header("Parameters to fill")]
    public TextMesh pointInfoText;
    public Transform point;
    public Transform velocityVisualizer;
    public new Rigidbody rigidbody;
    public new Collider collider;

    public float velocityThickness = 0.03f;

    [Header("Autofilled Parameters")]
    public PointManager manager;
    public long age;

    private float _speed;
    public float speed {
        get { return _speed; }
        set { _speed = value;
            rigidbody.velocity = direction * speed;
        }
    }

    public int pid;
    public int oid;
    public Vector3 direction;
    public bool isMovedByMouse;
    public Color pointColor;

    public Vector3 size;

    public bool animateSize;
    public float sizeVariationSpeed;

    public Vector3 normalizedVelocity;

    public float movementNoiseAmplitude = 0;
    public float movementNoiseFrequency = 20;

    public bool isIncorrectDetection = false;
    public bool isFlickering = false;

    private Vector3 _oldPosition;
    private float _timer = 0;

    private Ray _ray;
    private RaycastHit _raycastHit;

    private float _relativeTime = 0;
    private bool _oldPositionIsValid = false;

	#region MonoBehaviour Implementation

	void Start () {
        direction = Random.onUnitSphere;
        direction.z = 0;

        //Get velocity
        if (isIncorrectDetection) {
            rigidbody.velocity = Vector3.zero;
            speed = 0.0f;
        } else {
            var rndVelocity = direction * speed;
            rigidbody.velocity = rndVelocity;
        }

        velocityVisualizer.localScale = Vector3.zero;

        _timer = 0;
        _relativeTime = 0;

        UpdatePointSize();
    }

    private void Update() {

        //Handle IncorrectDetection points
        if (isIncorrectDetection) {
            _timer += Time.deltaTime;
            if (_timer > manager.incorrectDetectionDuration) {
                manager.RemoveIncorrectPoint(pid);
            }
        }

        //Handle flickering
        if (isFlickering) {
            _timer += Time.deltaTime;
            if (_timer > manager.pointFlickeringDuration) {
                manager.StopFlickering(pid);
            }
        }

        age++;

        //Update size
        UpdatePointSize();

        //Update position
        UpdatePointPosition();

        //Udpate text
        pointInfoText.text = "PID : " + pid + '\n' + '\n' + "OID : " + oid;
    }

    private void FixedUpdate() {

        ComputeNormalizedVelocity();
        //Update velocity
        float angle = Mathf.Atan2(normalizedVelocity.y, normalizedVelocity.x) * 180 / Mathf.PI;
        if (float.IsNaN(angle))
            return;

        velocityVisualizer.localRotation = Quaternion.Euler(new Vector3(0, 0, -angle + 90));

        velocityVisualizer.localScale = new Vector3(velocityThickness, normalizedVelocity.magnitude, velocityThickness);
    }

    public void OnMouseDrag() {

        _ray = manager.camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(_ray, out _raycastHit, 100.0f, manager.areaLayer)) {

            transform.position = new Vector3(_raycastHit.point.x, _raycastHit.point.y, 0);
        }
    }

    #endregion

    private void ComputeNormalizedVelocity()
    {
        if (_oldPositionIsValid) {
            normalizedVelocity = ((transform.position - _oldPosition) / Time.deltaTime);
            normalizedVelocity = new Vector3(-normalizedVelocity.x / manager.width, normalizedVelocity.y / manager.height, 0);
        } else { 
            normalizedVelocity = Vector3.zero; 
        }
        _oldPosition = transform.position;
        _oldPositionIsValid = true;
    }

    public void UpdatePointColor(Color color)
    {
        point.GetComponent<MeshRenderer>().material.SetColor("_BorderColor", color);
    }

    void UpdatePointSize() {

        if (animateSize) {

            _relativeTime += Time.deltaTime * sizeVariationSpeed;
            size.x = Mathf.Lerp(manager.minPointSize.x, manager.maxPointSize.x, Mathf.PerlinNoise(pid * 10, _relativeTime));
            size.y = Mathf.Lerp(manager.minPointSize.y, manager.maxPointSize.y, Mathf.PerlinNoise(pid * 20, _relativeTime));
            size.z = Mathf.Lerp(manager.minPointSize.z, manager.maxPointSize.z, Mathf.PerlinNoise(pid * 30, _relativeTime));

        }

        transform.localScale = Vector3.one;
        transform.localScale = new Vector3(size.x / transform.lossyScale.x, size.y / transform.lossyScale.y, size.z / transform.lossyScale.z);
    }

    private void UpdatePointPosition() {
        //var newPos = transform.position
        //    + Random.Range(-movementNoise, movementNoise) * Vector3.right
        //    + Random.Range(-movementNoise, movementNoise) * Vector3.up;

        Vector3 newPos = transform.position
                        + (Mathf.PerlinNoise(pid * 15, Time.time * movementNoiseFrequency) - 0.5f) * 2.0f * movementNoiseAmplitude * Vector3.right
                        + (Mathf.PerlinNoise(pid * 25, Time.time * movementNoiseFrequency) - 0.5f) * 2.0f * movementNoiseAmplitude * Vector3.up;

        newPos.x = Mathf.Clamp(newPos.x, -(manager.width + size.x) * 0.5f, (manager.width - size.x) * 0.5f);
        newPos.y = Mathf.Clamp(newPos.y, -(manager.height + size.y) * 0.5f, (manager.height - size.y) * 0.5f);

        transform.position = newPos;
    }

    public void StartFlickering() {

        _timer = 0;
        HidePoint();
        isFlickering = true;
    }

    public void HidePoint() {

        pointInfoText.gameObject.SetActive(false);
        point.gameObject.SetActive(false);
        velocityVisualizer.gameObject.SetActive(false);
        collider.enabled = false;
    }

    public void ShowPoint() {

        pointInfoText.gameObject.SetActive(true);
        point.gameObject.SetActive(true);
        velocityVisualizer.gameObject.SetActive(true);
        collider.enabled = true;
    }
}
