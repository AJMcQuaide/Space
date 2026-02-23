using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class CameraController : MonoBehaviour
{
    Transform cam;

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

    private void Awake()
    {
        cam = GetComponent<Camera>().transform;
        if (cam == null) { Debug.LogWarning("No camera"); }
        if (r == 0f) r = 5f;
        rSmooth = r;
    }

    private void Update()
    {
        GetInput();
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
        Debug.DrawLine(target.transform.position, target.transform.position + orbit, Color.yellow);

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
        r -= mouseWheelOutput * 0.005f * zoomSensativity;
        r = Mathf.Clamp(r, 2f, 20f);

        //Mouse position on screen
        prevMousePos = MousePos;
        MousePos = Mouse.current.position.ReadValue();
    }

    public float NormalizeFloat(float min, float max, float current)
    {
        return (current - min) / (max - min);
    }
}
