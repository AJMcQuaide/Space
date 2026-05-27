using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// For manipulation of game objects in runtime
/// </summary>
public class Manipulation : MonoBehaviour
{
    [SerializeField]
    float size;

    [SerializeField]
    CameraController cc;

    [SerializeField]
    GameObject moveTool;

    //[SerializeField]
    //GameObject rotateTool;

    [SerializeField, Range(1f, 10f)]
    float moveSensativity = 5f;

    Vector3 normalScale = new (1f, 1f, 1f);
    Vector3 increasedScale = new (1.25f, 1.25f, 1.25f);

    bool isDragging = false;

    GameObject picked;
    public GameObject Picked
    {
        get { return picked; }
        set 
        {
            if (value != picked && isDragging == false)
            {
                if (value != null)
                {
                    if (picked != null)
                    {
                        picked.transform.localScale = normalScale;
                    }
                    picked = value;
                    picked.transform.localScale = increasedScale;

                }
                else
                {
                    picked.transform.localScale = normalScale;
                    picked = value;
                }
            }
        }
    }

    bool hide;
    public bool Hide
    {
        get { return hide; }
        set
        {
            if (hide != value)
            {
                hide = value;
                HideThis(!hide, moveTool);
            }
        }
    }

    void Start()
    {
        if (cc == null)
        {
            cc = FindAnyObjectByType<CameraController>();
            Debug.Log("Found missing Camera Controller in " + GetType());
        }
    }

    void Update()
    {
        //Set the visibility of the tool
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Hide = cc.ObjectHighlight.Picked == null;
        }

        if (isDragging)
        {
            MouseDrag();
            cc.IsTracking = false;
        }

        Picked = cc.Picking(1 << 7);
        if (Hide == false)
        {
            KeepSize();
            if (picked != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                isDragging = true;
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }
    }

    private void FixedUpdate()
    {
        if (cc.IsTracking)
        {
            transform.position = cc.TrackedObject.position;
        }
    }

    /// <summary>
    /// Keep the aparent size of the object the same regardless of camera distance
    /// </summary>
    public void KeepSize()
    {
        float distance = (transform.position - cc.transform.position).magnitude;
        float scale = distance / size;
        transform.localScale = new Vector3(scale, scale, scale);
    }

    /// <summary>
    /// Hide the mesh renderer(s) for this object
    /// </summary>
    void HideThis(bool show, GameObject objectToHide)
    {
        objectToHide.SetActive(show);
    }

    /// <summary>
    /// Move or rotate the object based the movement of the mouse compared to the manipulation axis
    /// </summary>
    void MouseDrag()
    {
        //The move tool axis is projected onto the camera plane, the projection still exists in 3D space so it moves around with the camera
        Vector3 project = Vector3.ProjectOnPlane(picked.transform.localPosition, cc.Cam.transform.forward).normalized;

        //The projection is transformed local to the camera space
        Vector3 projectTransform = cc.Cam.transform.InverseTransformDirection(project);

        //Flatten the Vector to make it 2D
        Vector2 projectTransform2D = new(projectTransform.x, projectTransform.y);

        //Compare the click and drag of the mouse, to the axis to see if you are dragging in the direction of the axis or not
        float dot = Vector2.Dot(Inputs.Instance.LeftMouseDragDir, projectTransform2D);

        if (dot != 0)
        {
            //Drag the object based on the dot product (drag direction vs the tool's arrow), and the direction the arrow points (it's local space locations)
            Vector3 move = dot * moveSensativity * Time.deltaTime * Picked.transform.localPosition.normalized;
            cc.TrackedObject.position += move;
        }
        //transform.position of the source target object, += the picked axis location (aka direction), and multiply by dot.
        //And, disasble the camera rotate? Should the right mouse control the 
    }
}
