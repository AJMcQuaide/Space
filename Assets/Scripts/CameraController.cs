using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class CameraController : MonoBehaviour
{
    Camera cam;

    [SerializeField]
    GameObject target;

    /// <summary>
    /// Camera radius/distance target from focus target
    /// </summary>
    float r = 0;
    float rSmooth = 0;
    float xPos = 0;
    float yPos = 0;

    Vector2 mouseDelta = Vector2.zero;
    bool leftMouseHeld = false;
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

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null) { Debug.LogWarning("No camera"); }
        if (r == 0f) r = 5f;
        rSmooth = r;
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
    }

    private void Update()
    {
        GetInput();
        Picking();
    }

    void FixedUpdate()
    {
        GetMouseDrag();

        cam.transform.position = target.transform.position + CameraOrbit();
        cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, LookAtTarget(target.transform.position), 1f);
    }

    Vector3 CameraOrbit()
    {
        rSmooth = Mathf.Lerp(rSmooth, r, Time.fixedDeltaTime * 10f);
        float x = rSmooth * Mathf.Cos(yPos) * Mathf.Cos(xPos);
        float y = rSmooth * Mathf.Sin(yPos);
        float z = rSmooth * Mathf.Cos(yPos) * Mathf.Sin(xPos);
        Vector3 orbit = new(x, y, z);
        //Debug.DrawLine(target.transform.position, target.transform.position + orbit, Color.yellow);

        return orbit;
    }

    Quaternion LookAtTarget(Vector3 target)
    {
        Quaternion rotation = Quaternion.LookRotation(target - cam.transform.position, Vector3.up);
        return rotation;
    }

    public void GetMouseDrag()
    {
        if (Mouse.current != null)
        {                
            if (leftMouseHeld)
            {
                Vector2 unscaledMouseDelta = MousePos - prevMousePos;
                mouseDelta = 0.002f * rotateSensativity * unscaledMouseDelta;
            
                xPos -= mouseDelta.x;
                yPos -= mouseDelta.y;
                float yMax = verticalRotationMax * Mathf.Deg2Rad;
                yPos = Mathf.Clamp(yPos, -yMax, yMax);
                //Debug.Log("xPos: " +  xPos * Mathf.Rad2Deg + " yPos: " + yPos * Mathf.Rad2Deg);
            }
        }
        else { Debug.LogWarning("No Mouse!"); }
    }

    public void GetInput()
    {
        //Left mouse is pressed
        leftMouseHeld = Mouse.current.leftButton.isPressed;

        //Mouse wheel
        mouseWheelOutput = Mouse.current.scroll.ReadValue().y;
        r -= mouseWheelOutput * Time.fixedDeltaTime * zoomSensativity;
        r = Mathf.Clamp(r, 2f, 20f);

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
            if (highlightClone.activeSelf == false)
            {
                highlightClone.SetActive(true);
                float scale = hit.collider.GetComponent<CelestialBody>().Radius;
                highlightClone.transform.localScale = new Vector3(scale, scale, scale);
            }
            Debug.Log("Hit celestial body!");
        }
        else
        {
            //highlightClone.transform.position = transform.position;
            if (highlightClone.activeSelf == true)
            {
                highlightClone.SetActive(false);
            }
            Debug.Log("No hit");
        }

        Debug.DrawLine(transform.position, transform.position + z, Color.yellow);
        Debug.DrawLine(transform.position + z, transform.position + total, Color.red);
        Debug.DrawRay(transform.position, total, Color.blue);
    }
}
