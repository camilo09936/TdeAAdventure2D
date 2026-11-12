using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFit : MonoBehaviour
{
    public float levelWidth = 17.24f;
    public float maxSize = 5f;

    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (cam == null) return;
        cam.orthographicSize = Mathf.Min(maxSize, levelWidth * 0.5f / cam.aspect);
    }
}
