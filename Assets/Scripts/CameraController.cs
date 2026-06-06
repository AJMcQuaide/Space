using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    Camera cam;
    public Camera Cam { get { return cam; } }

    [SerializeField]
    Vector3 target;
    public Vector3 Target { get { return target; } set { target = value; } }

    [SerializeField]
    CelestialBody cameraTrackedObject;
    public CelestialBody CameraTrackedObject
    {
        get { return cameraTrackedObject; } 
        set
        {
            if (cameraTrackedObject != value)
            {
                if (cameraTrackedObject != null)
                {
                    cameraTrackedObject.Selected = false;
                }
                cameraTrackedObject = value;
                cameraTrackedObject.Selected = true;
            }
        }
    }

    /// <summary>
    /// Camera tracking of object
    /// </summary>
    
    bool isTracking;
    /// <summary>
    /// Camera tracking of object
    /// </summary>
    public bool IsTracking
    {
        get { return isTracking; }
        set
        {
            if (value != isTracking)
            {
                isTracking = value;
            }
        }
    }

    /// <summary>
    /// Camera radius/distance target from focus target
    /// </summary>
    float camDistance = 0;
    public float CamDistance { get { return camDistance; } }

    /// <summary>
    /// Camera radius/distance target from focus target smoothed
    /// </summary>
    float camDistanceSmooth = 0;

    [SerializeField, Range(0.1f, 10f)]
    float zoomSensativity;
    [SerializeField, Range(0.02f, 0.06f)]
    float rotateSensativity;
    [SerializeField, Range(0, 90f)]
    float verticalRotationMax;
    [SerializeField]

    /// <summary>
    /// The 2d mouse drag input per frame, that is then turned into the 3d orbit Vector3
    /// </summary>
    Vector2 camPosInput = Vector2.zero;

    Vector3 pickPos;

    [SerializeField]
    Highlighter objectHighlight;
    public Highlighter ObjectHighlight { get { return objectHighlight; } }

    [SerializeField]
    GameObject picked;
    public GameObject Picked { get { return picked; } set { picked = value; } }

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null) { Debug.LogWarning("No camera"); }
        if (camDistance == 0f) camDistance = 5f;
        camDistanceSmooth = camDistance;

        //Set the camera to Vector3.zero by default
        target = Vector3.zero;

        if (objectHighlight ==  null)
        {
            objectHighlight = FindAnyObjectByType<Highlighter>();
            Debug.Log("Found missing objectHighlighter in " + GetType());
        }
    }

    private void Update()
    {
        CameraDistance(zoomSensativity);
        SetPickPos();

        //Go to object when clicked on specified layer (Celestial Body)
        Picked = Picking(1 << 6);
        if (Picked != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            IsTracking = true;
            CameraTrackedObject = Picked.GetComponent<CelestialBody>();
        }
        if (IsTracking)
        {
            Target = CameraTrackedObject.transform.position;
        }
    }

    void FixedUpdate()
    {
        //Lerp
        transform.position = Vector3.Lerp(transform.position, target + CameraOrbitPos(), Time.deltaTime * 25f);

        //No Slerp
        transform.rotation = Quaternion.LookRotation(target - transform.position, Vector3.up);
    }

    Vector3 CameraOrbitPos()
    {
        camPosInput -= Inputs.Instance.MiddleMouseDragDir * rotateSensativity;
        float yMax = verticalRotationMax * Mathf.Deg2Rad;
        camPosInput.y = Mathf.Clamp(camPosInput.y, -yMax, yMax);

        camDistanceSmooth = Mathf.Lerp(camDistanceSmooth, camDistance, Time.fixedDeltaTime * 10f);
        float x = camDistanceSmooth * Mathf.Cos(camPosInput.y) * Mathf.Cos(camPosInput.x);
        float y = camDistanceSmooth * Mathf.Sin(camPosInput.y);
        float z = camDistanceSmooth * Mathf.Cos(camPosInput.y) * Mathf.Sin(camPosInput.x);
        Vector3 orbit = new(x, y, z);

        return orbit;
    }


    /// <summary>
    /// Return a range between 0 and 1
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="current"></param>
    /// <returns></returns>
    public float NormalizeFloat(float min, float max, float current)
    {
        return (current - min) / (max - min);
    }

    /// <summary>
    /// Shoot a ray from this camear's designated pick position, and return the object on the given layer
    /// </summary>
    public GameObject Picking(LayerMask mask)
    {
        if (Physics.Raycast(transform.position, pickPos, out RaycastHit hit, cam.farClipPlane, mask))
        {
            return hit.collider.gameObject;
        }
        else
        {
            return null;
        }

        ////Debug the picking
        //Debug.DrawLine(transform.position, transform.position + z, Color.yellow);
        //Debug.DrawLine(transform.position + z, transform.position + total, Color.red);
        //Debug.DrawRay(transform.position, total, Color.blue);
    }

    /// <summary>
    /// Set this Camera Controller's pick position, the point at which the raycast starts from
    /// </summary>
    void SetPickPos()
    {
        //float near = cam.nearClipPlane;
        float far = cam.farClipPlane;
        float fov = Camera.VerticalToHorizontalFieldOfView(cam.fieldOfView, cam.aspect);

        //What is the width of the far plane or near plane and compare that to the width of the screensize?
        float farWidth = far * Mathf.Tan(Mathf.Deg2Rad * fov * 0.5f);
        float ratio = farWidth / Screen.width * 2f;

        float a = (Mouse.current.position.ReadValue().x - (Screen.width * 0.5f)) * ratio;
        float b = (Mouse.current.position.ReadValue().y - (Screen.height * 0.5f)) * ratio;
        Vector3 z = transform.forward * far;
        Vector3 y = transform.right * a;
        Vector3 x = transform.up * b;

        pickPos = x + y + z;
        //Debug.Log("raycast target: " +  total + " Mouse pos x: " + a + " Mouse pos y: " + b);
    }

    /// <summary>
    /// Camera distance from object controlled by inputs (not orbit data just distance)
    /// </summary>
    /// <param name="sensitivity"></param>
    public void CameraDistance(float sensitivity)
    {
        //Mouse wheel
        camDistance -= Mouse.current.scroll.ReadValue().y * Time.fixedDeltaTime * sensitivity;
        camDistance = Mathf.Clamp(camDistance, 2f, 20f);
    }
}
