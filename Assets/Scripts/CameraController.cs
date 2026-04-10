using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    Camera cam;

    [SerializeField]
    Transform target;

    [SerializeField]
    Vector3 defaultCameraPos;

    /// <summary>
    /// Camera radius/distance target from focus target
    /// </summary>
    float rad = 0;
    /// <summary>
    /// Camera radius/distance target from focus target smoothed
    /// </summary>
    float radSmoothed = 0;

    float xPos = 0;
    float yPos = 0;

    Vector2 mouseDelta = Vector2.zero;
    float mouseWheelOutput = 0;
    Vector2 MousePos = Vector2.zero;
    Vector2 prevMousePos = Vector2.zero;

    [SerializeField, Range(0.1f, 10f)]
    float zoomSensativity;
    [SerializeField, Range(0.1f, 10f)]
    float rotateSensativity;
    [SerializeField, Range(0, 90f)]
    float verticalRotationMax;
    [SerializeField]

    GameObject highlightPrefab;
    GameObject highlightClone;

    /// <summary>
    /// Current resolution of the screen
    /// </summary>
    Vector2 screenSize;

    CelestialBody pickedCB;
    CelestialBody previousPickedCB;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null) { Debug.LogWarning("No camera"); }
        if (rad == 0f) rad = 5f;
        radSmoothed = rad;
        if (highlightPrefab != null)
        {
            highlightClone = Instantiate(highlightPrefab, transform.position, Quaternion.identity);
            if (highlightClone != null)
            {
                highlightClone.SetActive(false);
                Debug.Log("Highlight active status: " + highlightClone.activeSelf);
            }
            else { Debug.LogError("Highlight prefab was not instantiated"); }
        }
        else { Debug.LogError("Missing highlight prefab"); }

        //Set the camera to Vector3.zero by default
        target = new GameObject().transform;
        target.transform.position = defaultCameraPos;
    }

    private void Update()
    {
        GetInput();
        Picking();
    }

    void FixedUpdate()
    {
        GetMouseDrag();

        //Lerp
        transform.position = Vector3.Lerp(transform.position, target.position + CameraOrbit(), Time.deltaTime * 25f);

        //No Slerp
        transform.rotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
    }

    Vector3 CameraOrbit()
    {
        radSmoothed = Mathf.Lerp(radSmoothed, rad, Time.fixedDeltaTime * 10f);
        float x = radSmoothed * Mathf.Cos(yPos) * Mathf.Cos(xPos);
        float y = radSmoothed * Mathf.Sin(yPos);
        float z = radSmoothed * Mathf.Cos(yPos) * Mathf.Sin(xPos);
        Vector3 orbit = new(x, y, z);

        return orbit;
    }

    public void GetMouseDrag()
    {
        if (Mouse.current != null)
        {                
            if (Mouse.current.leftButton.isPressed)
            {
                Vector2 unscaledMouseDelta = MousePos - prevMousePos;
                mouseDelta = 0.002f * rotateSensativity * unscaledMouseDelta;
            
                xPos -= mouseDelta.x;
                yPos -= mouseDelta.y;
                float yMax = verticalRotationMax * Mathf.Deg2Rad;
                yPos = Mathf.Clamp(yPos, -yMax, yMax);
            }
        }
        else { Debug.LogWarning("No Mouse!"); }
    }

    public void GetInput()
    {
        //Mouse wheel
        mouseWheelOutput = Mouse.current.scroll.ReadValue().y;
        rad -= mouseWheelOutput * Time.fixedDeltaTime * zoomSensativity;
        rad = Mathf.Clamp(rad, 2f, 20f);

        //Mouse position on screen
        prevMousePos = MousePos;
        MousePos = Mouse.current.position.ReadValue();
        if (screenSize != null)
        {
            MousePos -= screenSize * 0.5f;
        }
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
    /// Detect if the mouse is hovering over a celestial body
    /// </summary>
    public void Picking()
    {
        //float near = cam.nearClipPlane;
        float far = cam.farClipPlane;
        float fov = Camera.VerticalToHorizontalFieldOfView(cam.fieldOfView, cam.aspect);
        screenSize = new Vector2(Screen.width, Screen.height);

        //What is the width of the far plane or near plane and compare that to the width of the screensize?
        float farWidth = far * Mathf.Tan(Mathf.Deg2Rad * fov * 0.5f);
        float ratio = farWidth / screenSize.x * 2f;

        float a = MousePos.x * ratio;
        float b = MousePos.y * ratio;
        Vector3 z = transform.forward * far;
        Vector3 y = transform.right * a;
        Vector3 x = transform.up * b;

        Vector3 total = x + y + z;
        //Debug.Log("raycast target: " +  total + " Mouse pos x: " + a + " Mouse pos y: " + b);

        if (Physics.Raycast(transform.position, total, out RaycastHit hit, total.magnitude, 1<<6))
        {
            highlightClone.transform.position = hit.collider.transform.position;
            pickedCB = hit.collider.GetComponent<CelestialBody>();
            if (pickedCB != previousPickedCB)
            {
                float scale;
                scale = pickedCB.Radius * (float)CelestialBody.SD * 2f;
                highlightClone.transform.localScale = new Vector3(scale, scale, scale);
            }

            if (highlightClone.activeSelf == false)
            {
                highlightClone.SetActive(true);
            }
            else
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    target = hit.collider.gameObject.transform;
                }
            }
            previousPickedCB = pickedCB;
        }
        else
        {
            //highlightClone.transform.position = transform.position;
            if (highlightClone.activeSelf == true)
            {
                highlightClone.SetActive(false);
            }
        }

        ////Debug the picking
        //Debug.DrawLine(transform.position, transform.position + z, Color.yellow);
        //Debug.DrawLine(transform.position + z, transform.position + total, Color.red);
        //Debug.DrawRay(transform.position, total, Color.blue);
    }
}
