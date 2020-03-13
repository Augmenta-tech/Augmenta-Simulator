using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public float zoomSpeed = 0.5f;
    public float minSize = 1.0f;
    public float dragSpeed = .01f;

    private new Camera camera;
    private Vector3 dragMouseOrigin;
    private Vector3 dragCameraOrigin;

	#region MonoBehaviour Implementation
	// Start is called before the first frame update
	void Start()
    {
        camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCameraPosition();
        UpdateCameraSize();

        if (Input.GetKeyDown(KeyCode.R)) {
            transform.position = -10 * Vector3.forward;
        }
    }

    #endregion

    void UpdateCameraSize() {

        if(!EventSystem.current.IsPointerOverGameObject())
            camera.orthographicSize -= Input.mouseScrollDelta.y * zoomSpeed;

        if (camera.orthographicSize < minSize)
            camera.orthographicSize = minSize;
    }

    void UpdateCameraPosition() {

        if (Input.GetMouseButtonDown(2)) {
            dragMouseOrigin = Input.mousePosition;
            dragCameraOrigin = transform.position;
            return;
        }

        if (Input.GetMouseButton(2)) {

            transform.position = dragCameraOrigin - (Input.mousePosition - dragMouseOrigin) * dragSpeed * camera.orthographicSize * 0.2f;
        }
    }
}
