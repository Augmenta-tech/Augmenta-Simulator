using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    [Header("Camera Control")]
    public new Camera camera;
    public VideoOutputManager videoOutputManager;

    public float zoomSpeed = 0.5f;
    public float minSize = 1.0f;
    public float dragSpeed = .01f;
    public float rotationSpeed = 1.0f;

    [Header("UI Control")]
    public GameObject orthoUI;
    public GameObject perspUI;
    public RectTransform genUIScrollView;

    private Vector3 dragMouseOrigin;
    private Vector3 dragCameraLocalPositionOrigin;
    private Quaternion dragRigOrientationOrigin;

    private float cameraStartingSize;
    private float cameraLastPerspectiveDistance;

    private PointManager _manager;
    private PointManager manager {
        get { if(!_manager)
                _manager = FindObjectOfType<PointManager>();
            return _manager;
        }
        set { _manager = value; }
    }

    private float _defaultDistance = 5.0f;

    //PlayerPrefs
    private string cameraOrthographicKey = "CameraOrthographic";
    private string cameraPositionXKey = "CameraPositionX";
    private string cameraPositionYKey = "CameraPositionY";
    private string cameraPositionZKey = "CameraPositionZ";
    private string cameraOrientationXKey = "CameraOrientationX";
    private string cameraOrientationYKey = "CameraOrientationY";
    private string cameraOrientationZKey = "CameraOrientationZ";

    #region MonoBehaviour Implementation

    void Start() {

        cameraStartingSize = camera.orthographicSize;

        ResetCamera();
        SwitchToPerspective();

        LoadPlayerPrefs();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCameraRig();
        UpdateCameraFOV();

        if (Input.GetKeyDown(KeyCode.R)) {
            ResetCamera();
        }

        if (Input.GetKeyDown(KeyCode.P)) {
            SwitchCameraProjection();
        }
    }

    private void OnDisable() {

        SavePlayerPrefs();
    }

    #endregion

    /// <summary>
    /// Update camera field of view according to mouse inputs
    /// </summary>
    void UpdateCameraFOV() {

        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (camera.orthographic) {
            camera.orthographicSize -= Input.mouseScrollDelta.y * zoomSpeed;
        } else {
            Vector3 cameraLocalPosition = camera.transform.localPosition;
            cameraLocalPosition.y -= Input.mouseScrollDelta.y * zoomSpeed;
            camera.transform.localPosition = cameraLocalPosition;
        }

        if (camera.orthographicSize < minSize)
            camera.orthographicSize = minSize;

        if (camera.transform.localPosition.y < minSize)
            camera.transform.localPosition = new Vector3(camera.transform.localPosition.x, minSize, camera.transform.localPosition.z);
    }

    /// <summary>
    /// Move and rotate camera rig according to inputs
    /// </summary>
    void UpdateCameraRig() {

        UpdateCameraRigPosition();
        UpdateCameraRigRotation();
    }

    void UpdateCameraRigPosition() {

        if (Input.GetMouseButtonDown(2)) {
            dragMouseOrigin = Input.mousePosition;
            dragCameraLocalPositionOrigin = camera.transform.localPosition;
            return;
        }

        if (Input.GetMouseButton(2)) {

            Vector3 offset = Input.mousePosition - dragMouseOrigin;
            float tmp = offset.y;
            offset.y = offset.z;
            offset.z = tmp;
            camera.transform.localPosition = dragCameraLocalPositionOrigin - offset * dragSpeed * camera.orthographicSize;
        }
    }

    void UpdateCameraRigRotation() {

        if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftAlt)) {
            dragMouseOrigin = Input.mousePosition;
            dragRigOrientationOrigin = transform.rotation;
            return;
        }

        if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftAlt)) {

            transform.rotation = Quaternion.Euler(dragRigOrientationOrigin.eulerAngles.x - (Input.mousePosition.y - dragMouseOrigin.y) * rotationSpeed,
                                                  dragRigOrientationOrigin.eulerAngles.y + (Input.mousePosition.x - dragMouseOrigin.x) * rotationSpeed,
                                                  0);
        }

    }

    /// <summary>
    /// Reset camera to its starting position and field of view
    /// </summary>
    void ResetCamera() {

        if (manager) {
            camera.orthographicSize = manager.height / 1.5f;
        } else {
            camera.orthographicSize = cameraStartingSize;
        }

        SetCameraToDefaultPosition();

        transform.rotation = Quaternion.Euler(Vector3.zero);

        camera.transform.localPosition = new Vector3(camera.transform.localPosition.x,
                                            Mathf.Max(manager.width, manager.height) * 0.5f + _defaultDistance,
                                            camera.transform.localPosition.z);

        cameraLastPerspectiveDistance = camera.transform.localPosition.y;
    }

    /// <summary>
    /// Compute default camera position centering the scene
    /// </summary>
    /// <returns></returns>
    void SetCameraToDefaultPosition() {

        camera.transform.localPosition = Vector3.up * _defaultDistance;

        Vector3 offsetCenter = Vector3.zero;

        if (genUIScrollView.gameObject.activeInHierarchy)
            offsetCenter = camera.ScreenToWorldPoint(new Vector3((Screen.width - genUIScrollView.sizeDelta.x) * 0.5f + genUIScrollView.sizeDelta.x,
                                                                  0.5f * Screen.height,
                                                                  _defaultDistance));

        camera.transform.localPosition = new Vector3(- offsetCenter.x, _defaultDistance, 0); 
    }

    /// <summary>
    /// Set the camera distance according to the area size (to avoid clipping)
    /// </summary>
    public void SetDistanceFromAreaSize() {

        if (camera.orthographic) {
            camera.transform.localPosition = new Vector3(camera.transform.localPosition.x,
                                                        Mathf.Max(manager.width, manager.height) * 0.5f + 10.0f,
                                                        camera.transform.localPosition.z);
        }

    }

    /// <summary>
    /// Switch camera projection between orthographic and perspective
    /// </summary>
    public void SwitchCameraProjection() {

        if (camera.orthographic) {
            SwitchToPerspective();
        } else {
            SwitchToOrthographic();
        }
    }

    /// <summary>
    /// Switch camera to perspective projection
    /// </summary>
    public void SwitchToPerspective() {

        camera.orthographic = false;
        videoOutputManager.spoutCameraOrthographic = false;

        orthoUI.SetActive(false);
        perspUI.SetActive(true);

        camera.transform.localPosition = new Vector3(camera.transform.localPosition.x,
                                cameraLastPerspectiveDistance,
                                camera.transform.localPosition.z);
    }

    /// <summary>
    /// Switch camera to orthographic projection
    /// </summary>
    public void SwitchToOrthographic() {

        camera.orthographic = true;
        videoOutputManager.spoutCameraOrthographic = true;

        perspUI.SetActive(false);
        orthoUI.SetActive(true);

        cameraLastPerspectiveDistance = camera.transform.localPosition.y;

        SetDistanceFromAreaSize();
    }

    /// <summary>
    /// Load PlayerPrefs
    /// </summary>
    void LoadPlayerPrefs() {

        //Camera projection type
        if (PlayerPrefs.HasKey(cameraOrthographicKey)) {
            if(PlayerPrefs.GetInt(cameraOrthographicKey) == 1) {
                SwitchToOrthographic();
            } else {
                SwitchToPerspective();
            }
        }

        //Camera position
        if(PlayerPrefs.HasKey(cameraPositionXKey) && PlayerPrefs.HasKey(cameraPositionYKey) && PlayerPrefs.HasKey(cameraPositionZKey))
            camera.transform.localPosition = new Vector3(PlayerPrefs.GetFloat(cameraPositionXKey), 
                                                         PlayerPrefs.GetFloat(cameraPositionYKey),
                                                         PlayerPrefs.GetFloat(cameraPositionZKey));

        //Camera orientation
        if (PlayerPrefs.HasKey(cameraOrientationXKey) && PlayerPrefs.HasKey(cameraOrientationYKey) && PlayerPrefs.HasKey(cameraOrientationZKey))
            transform.rotation = Quaternion.Euler(PlayerPrefs.GetFloat(cameraOrientationXKey),
                                                  PlayerPrefs.GetFloat(cameraOrientationYKey),
                                                  PlayerPrefs.GetFloat(cameraOrientationZKey));

    }

    /// <summary>
    /// Save to PlayerPrefs
    /// </summary>
    void SavePlayerPrefs() {

        //Camera projection type
        if (camera.orthographic) {
            PlayerPrefs.SetInt(cameraOrthographicKey, 1);
        } else {
            PlayerPrefs.SetInt(cameraOrthographicKey, 0);
        }

        //Camera position
        PlayerPrefs.SetFloat(cameraPositionXKey, camera.transform.localPosition.x);
        PlayerPrefs.SetFloat(cameraPositionYKey, camera.transform.localPosition.y);
        PlayerPrefs.SetFloat(cameraPositionZKey, camera.transform.localPosition.z);

        //Camera orientation
        PlayerPrefs.SetFloat(cameraOrientationXKey, transform.rotation.eulerAngles.x);
        PlayerPrefs.SetFloat(cameraOrientationYKey, transform.rotation.eulerAngles.y);
        PlayerPrefs.SetFloat(cameraOrientationZKey, transform.rotation.eulerAngles.z);
    }
}
