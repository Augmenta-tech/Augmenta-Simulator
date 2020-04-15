using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointBehaviour : MonoBehaviour {

    [Header("Parameters to fill")]
    public TextMesh pointInfoText;
    public Transform point;
    public Transform velocityVisualizer;
    public new Collider collider;

    public float velocityThickness = 0.015f;
    public float textScaleMultiplier = 0.005f;

    [Header("Autofilled Parameters")]
    public PointManager manager;
    public long ageInFrames;
    public float ageInSeconds;

    public float speed;

    public int id;
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

    public float rotationNoiseAmplitude = 0;
    public float rotationNoiseFrequency = 20;

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

        //Get velocity
        if (isIncorrectDetection) {
            direction = Vector3.zero;
            speed = 0.0f;
        } else {
            direction = Random.onUnitSphere;
            direction.y = 0;
            direction = direction.normalized;
        }

        velocityVisualizer.localScale = Vector3.zero;

        _timer = 0;
        _relativeTime = 0;

        ageInFrames = 0;
        ageInSeconds = 0;

        UpdatePointSize();
        UpdatePointPosition();
        UpdatePointRotation();
    }

    private void Update() {

        //Handle IncorrectDetection points
        if (isIncorrectDetection) {
            _timer += Time.deltaTime;
            if (_timer > manager.incorrectDetectionDuration) {
                manager.RemoveIncorrectPoint(id);
            }
        }

        //Handle flickering
        if (isFlickering) {
            _timer += Time.deltaTime;
            if (_timer > manager.pointFlickeringDuration) {
                manager.StopFlickering(id);
            }
        }

        ageInFrames++;
        ageInSeconds += Time.deltaTime;

        //Update point
        UpdatePointSize();
        UpdatePointPosition();
        UpdatePointRotation();

        //Udpate text
        pointInfoText.text = "ID : " + id + '\n' + '\n' + "OID : " + oid;
    }

    private void LateUpdate() {

        ComputeNormalizedVelocity();
        UpdateVelocityVisualizer();

    }

    public void OnMouseDrag() {

        _ray = manager.camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(_ray, out _raycastHit, Mathf.Infinity, manager.areaLayer)) {

            transform.position = new Vector3(_raycastHit.point.x, 0, _raycastHit.point.z);
        }
    }

    #endregion

    private void ComputeNormalizedVelocity()
    {
        if (_oldPositionIsValid) {
            normalizedVelocity = (transform.position - _oldPosition) / Time.deltaTime;
            normalizedVelocity = new Vector3(-normalizedVelocity.x / manager.width, 0, normalizedVelocity.z / manager.height);
        } else { 
            normalizedVelocity = Vector3.zero; 
        }
        _oldPosition = transform.position;
        _oldPositionIsValid = true;
    }

    private void UpdateVelocityVisualizer() {

        float angle = Mathf.Atan2(normalizedVelocity.z, -normalizedVelocity.x) * Mathf.Rad2Deg;
        if (float.IsNaN(angle))
            return;

        velocityVisualizer.localPosition = new Vector3(0, 0, -(size.z + velocityThickness) * 0.5f);
        velocityVisualizer.localRotation = Quaternion.Euler(0, 0, angle - 90);
        velocityVisualizer.localScale = new Vector3(velocityThickness, normalizedVelocity.magnitude, velocityThickness);
    }

    public void UpdatePointColor(Color color)
    {
        point.GetComponent<MeshRenderer>().material.SetColor("_BorderColor", color);
    }

    void UpdatePointSize() {

        if (animateSize) {

            _relativeTime += Time.deltaTime * sizeVariationSpeed;
            size.x = Mathf.Lerp(manager.minPointSize.x, manager.maxPointSize.x, Mathf.PerlinNoise(id * 10, _relativeTime));
            size.y = Mathf.Lerp(manager.minPointSize.y, manager.maxPointSize.y, Mathf.PerlinNoise(id * 20, _relativeTime));
            size.z = Mathf.Lerp(manager.minPointSize.z, manager.maxPointSize.z, Mathf.PerlinNoise(id * 30, _relativeTime));

        }

        //Enforce that width >= height
        size.x = Mathf.Max(size.x, size.y);

        point.transform.localScale = Vector3.one;
        point.transform.localScale = new Vector3(size.x / point.transform.lossyScale.x, size.y / point.transform.lossyScale.y, size.z / point.transform.lossyScale.z);

        float textScale = Mathf.Min(size.x, size.y) * textScaleMultiplier;
        pointInfoText.transform.localScale = Vector3.one;
        pointInfoText.transform.localScale = new Vector3(textScale / transform.lossyScale.x, textScale / transform.lossyScale.y, textScaleMultiplier);
    }

    private void UpdatePointPosition() {


        //Move along velocity
        Vector3 newPos = transform.position + direction * speed * Time.deltaTime;

        //Add noise
        newPos += (Mathf.PerlinNoise(id * 15, Time.time * movementNoiseFrequency) - 0.5f) * 2.0f * movementNoiseAmplitude * Vector3.right
                + (Mathf.PerlinNoise(id * 25, Time.time * movementNoiseFrequency) - 0.5f) * 2.0f * movementNoiseAmplitude * Vector3.forward;

        //Rebound on borders
        if (newPos.x <= -(manager.width  - size.x) * 0.5f || newPos.x >= (manager.width  - size.x) * 0.5f) { direction.x *= -1; }
        if (newPos.z <= -(manager.height - size.y) * 0.5f || newPos.z >= (manager.height - size.y) * 0.5f) { direction.z *= -1; }

        //Clamp in area
        newPos.x = Mathf.Clamp(newPos.x, -(manager.width - size.x) * 0.5f, (manager.width - size.x) * 0.5f);
        newPos.z = Mathf.Clamp(newPos.z, -(manager.height - size.y) * 0.5f, (manager.height - size.y) * 0.5f);

        //Update height according to point height
        newPos.y = size.z * 0.5f;

        transform.position = newPos;
    }

    private void UpdatePointRotation() {

        //Angle from displacement
        Vector3 displacement = transform.position - _oldPosition;
        float angle = -Mathf.Atan2(displacement.x, displacement.z) * Mathf.Rad2Deg;

        //Add noise
        angle += (Mathf.PerlinNoise(id * 35, Time.time * rotationNoiseFrequency) - 0.5f) * 2.0f * rotationNoiseAmplitude;

        point.transform.localRotation = Quaternion.Euler(transform.localRotation.x, transform.localRotation.y, angle);
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
    }

    public void ShowPoint() {

        pointInfoText.gameObject.SetActive(true);
        point.gameObject.SetActive(true);
        velocityVisualizer.gameObject.SetActive(true);
    }
}
