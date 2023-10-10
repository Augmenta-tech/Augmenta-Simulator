using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointBehaviour : MonoBehaviour {

    [Header("Parameters to fill")]
    public TextMesh pointInfoText;
    public Transform point;
    public Transform speedPivot;
    public Transform orientationPivot;
    public new Collider collider;
    public Material velocityMaterial;
    public Material orientationMaterial;
    public Color velocityColor;
    public Color orientationColor;

    public float vectorThickness = 0.03f;
    public float vectorDefaultMagnitude = 0.5f;
    public float textScaleMultiplier = 0.005f;

    [Header("Autofilled Parameters")]
    public PointManager manager;
    public long ageInFrames;
    public float ageInSeconds;

    public float speed;

    public int id;
    public int slotid;
    public Vector3 direction;
    public bool isMovedByMouse;
    public Color pointColor;
    public Vector3 size;
    public bool animateSize;
    public float sizeVariationSpeed;
    public float orientation;
    public Vector3 normalizedVelocity;

    public float movementNoiseAmplitude = 0;
    public float movementNoiseFrequency = 20;

    public float rotationNoiseAmplitude = 0;
    public float rotationNoiseFrequency = 20;

    public float speedThresholdForOrientation = 0.5f;

    public bool isIncorrectDetection = false;
    public bool isFlickering = false;

    private Vector3 _oldPosition;
    private float _timer = 0;

    private Ray _ray;
    private RaycastHit _raycastHit;

    private float _relativeTime = 0;
    private bool _oldPositionIsValid = false;

    private float _speedAngle;
    private float _oldAngle;
    private float _oldVelocityMagnitude;
    private float _orientationOffset = 0;

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

        speedPivot.localScale = Vector3.zero;
        orientationPivot.localScale = new Vector3(vectorThickness, vectorDefaultMagnitude, vectorThickness);

        _timer = 0;
        _relativeTime = 0;

        ageInFrames = 0;
        ageInSeconds = 0;

        UpdatePoint();
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
        UpdatePoint();

        //Udpate text
        pointInfoText.text = "ID : " + id + '\n' + '\n' + "SLOTID : " + slotid;
    }

    private void LateUpdate() {

        ComputeNormalizedVelocity();
        UpdateSpeedVisualization();

        ComputePointOrientation();
        UpdateOrientationVisualization();

        _oldPosition = transform.position;
        _oldPositionIsValid = true;
    }

    public void OnMouseDrag() {

        _ray = manager.camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(_ray, out _raycastHit, Mathf.Infinity, manager.areaLayer)) {

            transform.position = new Vector3(_raycastHit.point.x, 0, _raycastHit.point.z);
        }
    }

    public void UpdatePoint() {

        UpdatePointSize();
        UpdatePointPosition();
        UpdatePointRotation();
    }

    #endregion

    private void ComputeNormalizedVelocity()
    {
        _oldVelocityMagnitude = normalizedVelocity.magnitude;

        if (_oldPositionIsValid) {
            normalizedVelocity = (transform.position - _oldPosition) / Time.deltaTime;
            normalizedVelocity = new Vector3(-normalizedVelocity.x / manager.width, normalizedVelocity.y, normalizedVelocity.z / manager.height);
        } else { 
            normalizedVelocity = Vector3.zero; 
        }
    }

    private void UpdateSpeedVisualization() {

        speedPivot.localScale = new Vector3(vectorThickness * size.x, normalizedVelocity.magnitude * size.y, vectorThickness * size.z);
        speedPivot.localPosition = new Vector3(0, 0, -(size.z + vectorThickness) * 0.5f);

        if (normalizedVelocity.magnitude != 0) {

            _speedAngle = Mathf.Atan2(normalizedVelocity.z, -normalizedVelocity.x) * Mathf.Rad2Deg;
            speedPivot.localRotation = Quaternion.Euler(0, 0, _speedAngle - 90);
        }
        
    }

    private void ComputePointOrientation() {

        if (normalizedVelocity.magnitude >= speedThresholdForOrientation) {
            orientation = _speedAngle;
        } else {
            if(_oldVelocityMagnitude >= speedThresholdForOrientation) {
                //Update offset
                _orientationOffset = orientation - point.transform.localRotation.eulerAngles.z;
            }

            orientation = point.transform.localRotation.eulerAngles.z + _orientationOffset;
        }

        orientation = orientation >= 0 ? orientation : orientation + 360.0f;
    }

    private void UpdateOrientationVisualization() {

        orientationPivot.localScale = new Vector3(vectorThickness * size.x, vectorDefaultMagnitude * size.y, vectorThickness * size.z);
        orientationPivot.localPosition = new Vector3(0, 0, -(size.z + vectorThickness) * 0.5f);
        orientationPivot.localRotation = Quaternion.Euler(0, 0, orientation - 90);
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

        float angle = -Mathf.Atan(displacement.x / displacement.z) * Mathf.Rad2Deg;

        //Debug.Log("Displacement = " + displacement.ToString("F6") + "; tanValue = "+ displacement.x / displacement.z + "; angle(rad) = "+ -Mathf.Atan(displacement.x / displacement.z) + "; angle(deg) = " + angle);

        //Add noise
        angle += (Mathf.PerlinNoise(id * 35, Time.time * rotationNoiseFrequency) - 0.5f) * 2.0f * rotationNoiseAmplitude;

        if (Mathf.Abs(_oldAngle - angle) < 10) {
            point.transform.localRotation = Quaternion.Euler(transform.localRotation.x, transform.localRotation.y, angle);
        }

        _oldAngle = angle;
    }

    public void StartFlickering() {

        _timer = 0;
        HidePoint();
        isFlickering = true;
    }

    public void HidePoint() {

        pointInfoText.gameObject.SetActive(false);
        point.gameObject.SetActive(false);
        speedPivot.gameObject.SetActive(false);
        orientationPivot.gameObject.SetActive(false);
    }

    public void ShowPoint() {

        pointInfoText.gameObject.SetActive(true);
        point.gameObject.SetActive(true);
        speedPivot.gameObject.SetActive(true);
        orientationPivot.gameObject.SetActive(true);
    }

    public void MutePoint(bool mute) {

        if (mute) {
            UpdatePointColor(Color.gray);
            velocityMaterial.SetColor("_BorderColor", Color.gray);
            orientationMaterial.SetColor("_BorderColor", Color.gray);
        } else {
            UpdatePointColor(pointColor);
            velocityMaterial.SetColor("_BorderColor", velocityColor);
            orientationMaterial.SetColor("_BorderColor", orientationColor);
        }

    }
}
