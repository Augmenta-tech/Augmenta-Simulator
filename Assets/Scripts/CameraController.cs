using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    public new Camera camera;

    public float zoomSpeed = 0.5f;
    public float minSize = 1.0f;
    public float dragSpeed = .01f;
    public float rotationSpeed = 1.0f;

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
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCameraRig();
        UpdateCameraFOV();

        if (Input.GetKeyDown(KeyCode.R)) {
            ResetCamera();
        }
    }

    #endregion

    /// <summary>
    /// Update camera field of view according to mouse inputs
    /// </summary>
    void UpdateCameraFOV() {

        if(!EventSystem.current.IsPointerOverGameObject())
            camera.orthographicSize -= Input.mouseScrollDelta.y * zoomSpeed;

        if (camera.orthographicSize < minSize)
            camera.orthographicSize = minSize;
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

            camera.transform.localPosition = dragCameraLocalPositionOrigin - (Input.mousePosition - dragMouseOrigin) * dragSpeed * camera.orthographicSize;
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
}
