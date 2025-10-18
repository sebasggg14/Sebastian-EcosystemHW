using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class NoPostCameraBehaviour : MonoBehaviour
{
    [SerializeField] 
    private Camera baseCamera;

    private Camera overlayCamera;

    void Awake()
    {
        overlayCamera = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (baseCamera == null || overlayCamera == null)
            return;

        // Copy transform to perfectly match
        transform.position = baseCamera.transform.position;
        transform.rotation = baseCamera.transform.rotation;

        // Copy projection settings
        overlayCamera.orthographic = baseCamera.orthographic;
        overlayCamera.fieldOfView = baseCamera.fieldOfView;
        overlayCamera.orthographicSize = baseCamera.orthographicSize;
        overlayCamera.nearClipPlane = baseCamera.nearClipPlane;
        overlayCamera.farClipPlane = baseCamera.farClipPlane;
    }
}
