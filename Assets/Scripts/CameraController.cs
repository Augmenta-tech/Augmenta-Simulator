using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    [Header("Camera Control")]
    public new Camera camera;

    public float zoomSpeed = 0.5f;
    public float minSize = 1.0f;
    public float dragSpeed = .01f;
    public float rotationSpeed = 1.0f;

    [Header("UI Control")]
    public GameObject orthoUI;
    public GameObject perspUI;

    private Vector3 dragMouseOrigin;
    private Vector3 dragCameraLocalPositionOrigin;
    private Quaternion dragRigOrientationOrigin;

    private float cameraStartingSize;
    private Vector3 cameraStartingLocalPosition;
    private Quaternion rigStartingRotation;

	#region MonoBehaviour Implementation

    void Start() {

        cameraStartingLocalPosition = camera.transform.localPosition;
        rigStartingRotation = transform.rotation;
        cameraStartingSize = camera.orthographicSize;

        ResetCamera();
        SwitchToOrthographic();
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

        camera.transform.localPosition = cameraStartingLocalPosition;
        transform.rotation = rigStartingRotation;

        //Set size to match area
        var manager = FindObjectOfType<PointManager>();

        if (manager) {
            camera.orthographicSize = manager.height / 1.5f;
        } else {
            camera.orthographicSize = cameraStartingSize;
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

        orthoUI.SetActive(false);
        perspUI.SetActive(true);
    }

    /// <summary>
    /// Switch camera to orthographic projection
    /// </summary>
    public void SwitchToOrthographic() {

        camera.orthographic = true;

        perspUI.SetActive(false);
        orthoUI.SetActive(true);
    }

}
