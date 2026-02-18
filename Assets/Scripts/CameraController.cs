using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    Camera cam;

    [SerializeField]
    bool rotate;

    [SerializeField, Range(0f, 1f)]
    float rotateSpeed = 0.1f;

    [SerializeField]
    GameObject target;

    //Temp, leave the position that was last in the editor
    Vector3 wideViewPos;
    Quaternion wideViewRot;

    [SerializeField]
    float followDistance = 1f;

    float timer = 0;

    Vector2 mousePosOnClick = Vector2.zero;
    Vector2 mouseDragDelta = Vector2.zero;

    float multiplier = 0.01f;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null) { Debug.LogWarning("No camera"); }

        //Temp, leave the position that was last in the editor
        wideViewPos = transform.position;
        wideViewRot = transform.rotation;
    }

    private void Update()
    {
        Zoom();
    }

    void FixedUpdate()
    {
        //if (rotate)
        //{
        //    Rotate();
        //}
        //else
        //{
        //    //Temp, leave the position that was last in the editor
        //    cam.transform.position = wideViewPos;
        //    cam.transform.rotation = wideViewRot;
        //}

        if (Input.GetMouseButton(0))
        {
            if (mousePosOnClick == Vector2.zero)
            {
                mousePosOnClick = Mouse.current.position.ReadValue();
            }
            MouseDrag();
        }
        else
        {
            mouseDragDelta = Vector2.zero;
            mousePosOnClick = Vector2.zero;
        }

        cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, LookAtTarget(target.transform.position), Time.deltaTime * 25f);
        cam.transform.position = Vector3.Lerp(cam.transform.position, target.transform.position + Rotate(), Time.deltaTime * 25f);

        //Debug
        Debug.Log("Mouse Delta: " + mouseDragDelta);
        //Debug.Log("First click: " + mousePosOnClick);
    }

    Vector3 Rotate()
    {
        //float t = Time.time * rotateSpeed;
        timer += Time.deltaTime * rotateSpeed;
        Vector3 offset = new Vector3(Mathf.Sin(timer), 0.2f, Mathf.Cos(timer)) * followDistance;
        return offset;
    }

    Quaternion LookAtTarget(Vector3 target)
    {
        Quaternion rotation = Quaternion.LookRotation(target - cam.transform.position, Vector3.up);
        return rotation;
    }

    public void Zoom()
    {
        if (Mouse.current != null)
        {
            Vector2 mouseWheel = Mouse.current.scroll.ReadValue();
            followDistance += mouseWheel.y * 0.00833f;
            followDistance = Mathf.Clamp(followDistance, 2f, 15f);
        }
        else { Debug.LogError("No Mouse"); }
    }

    public void MouseDrag()
    {
        Vector2 unscaledMouseDelta = mousePosOnClick - Mouse.current.position.ReadValue();
        mouseDragDelta = unscaledMouseDelta * multiplier;
    }
}
