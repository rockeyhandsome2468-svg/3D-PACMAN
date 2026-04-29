using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float zoomSpeed = 5f;

    private float yaw = 45f;
    private float pitch = 35f;
    private float distance = 20f;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam != null) 
        { 
            cam.orthographic = true; 
            cam.orthographicSize = 7f; // Adjust this value to zoom in or out
        }
        FindTarget();
    }

    void FindTarget()
    {
        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null) 
        {
            target = pc.transform;
            Vector3 offset = Quaternion.Euler(pitch, yaw, 0) * new Vector3(0, 0, -distance);
            transform.position = target.position + offset; // Snap instantly!
            transform.LookAt(target.position + Vector3.up * 0.5f);
        }
    }

    void LateUpdate()
    {
        // Automatically find the player if not assigned or if the old player was hidden
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            FindTarget();
            return;
        }

        // Mouse rotation (Left or Right click)
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * rotationSpeed;
            pitch -= Input.GetAxis("Mouse Y") * rotationSpeed;
            pitch = Mathf.Clamp(pitch, 10f, 85f);
        }

        // Mouse scroll zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f && cam != null)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, 3f, 15f);
        }

        Vector3 offset = Quaternion.Euler(pitch, yaw, 0) * new Vector3(0, 0, -distance);
        Vector3 desired = target.position + offset;
        
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 0.5f);
    }
}