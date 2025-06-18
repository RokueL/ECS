using UnityEngine;

/// <summary>
/// Orthographic camera controller:
/// - Mouse wheel to zoom in/out
/// - Right mouse button drag to pan
/// Attach this to your Main Camera (with Camera.orthographic = true)
/// </summary>
[RequireComponent(typeof(Camera))]
public class OrthographicCameraController : MonoBehaviour
{
    [Header("Zoom Settings")]
    [Tooltip("Speed multiplier for zooming via mouse wheel")]    
    public float zoomSpeed = 5f;
    [Tooltip("Minimum orthographic size (max zoom in)")]
    public float minZoom = 2f;
    [Tooltip("Maximum orthographic size (max zoom out)")]
    public float maxZoom = 20f;

    [Header("Pan Settings")]
    [Tooltip("Speed multiplier for panning while dragging")]    
     public float panSpeed = 1f;

    private Camera cam;
    private Vector3 dragOrigin;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true; // ensure orthographic mode
    }

    void Update()
    {
        HandleZoom();
        HandlePan();
    }

    /// <summary>
    /// Zoom camera by adjusting orthographicSize based on mouse scroll
    /// </summary>
    private void HandleZoom()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > Mathf.Epsilon)
        {
            float newSize = cam.orthographicSize - scroll * zoomSpeed * Time.deltaTime;
            cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }

    /// <summary>
    /// Pan camera by right-click dragging
    /// </summary>
    private void HandlePan()
    {
        if (Input.GetMouseButtonDown(1))
        {
            // Record where the mouse began dragging in world space
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(1))
        {
            // Calculate current mouse position in world space
            Vector3 currentPos = cam.ScreenToWorldPoint(Input.mousePosition);
            // Determine how far to move camera
            Vector3 difference = dragOrigin - currentPos;
            // Apply pan velocity
            transform.position += difference * panSpeed;
        }
    }
}
