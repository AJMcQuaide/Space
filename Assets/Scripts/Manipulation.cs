using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;

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

    [SerializeField]
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

    bool show;
    public bool Show
    {
        get { return show; }
        set
        {
            if (show != value)
            {
                show = value;
                ShowObject(show, moveTool);
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
        ShowObject(false, moveTool);
    }

    void Update()
    {
        if (picked != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            isDragging = true;
        }

        if (isDragging)
        {
            MouseDrag();
            cc.IsTracking = false;
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                isDragging = false;
            }
        }
        else
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Show = cc.Picked != null;
            }
        }

        Picked = cc.Picking(1 << 7);
        KeepSize();
    }

    private void LateUpdate()
    {
        if (cc.CameraTrackedObject != null)
        {
            transform.position = cc.CameraTrackedObject.transform.position;
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
    void ShowObject(bool show, GameObject objectToHide)
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
            Vector3 move = dot * moveSensativity * Time.deltaTime * Picked.transform.localPosition.normalized * cc.CamDistance;

            cc.CameraTrackedObject.transform.position += move;
            cc.CameraTrackedObject.Position += new double3((double)move.x, (double)move.y, (double)move.z);
        }
    }
}
