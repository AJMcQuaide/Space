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

    Vector3 normalScale = new (1f, 1f, 1f);
    Vector3 increasedScale = new (1.25f, 1.25f, 1.25f);

    bool isDragging = false;

    GameObject picked;
    public GameObject Picked
    {
        get { return picked; }
        set 
        {
            if (value != picked)
            {
                if (value == null | picked != null)
                {
                    picked.transform.localScale = normalScale;
                    picked = value;
                }
                else
                {
                    picked = value;
                    picked.transform.localScale = increasedScale;
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
            Debug.LogWarning("Dragging");
        }

        Picked = cc.Picking(1 << 7);
        transform.position = cc.Target.position;
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

    void MouseDrag()
    {
        //The move tool axis is projected onto the camera plane, the projection still is 3D and exists in 3D space
        Vector3 project = Vector3.ProjectOnPlane(picked.transform.localPosition, cc.Cam.transform.forward).normalized;

        //The projection is transformed local to the camera so that when the camera moves around the vector doesnt change
        Vector3 projectTransform = cc.Cam.transform.InverseTransformDirection(project);

        //Remove the Z element, once it is transformed to the camera it essentially becomes 2D
        Vector2 projectTransform2D = new(projectTransform.x, projectTransform.y);

        //Compare the click and drag of the mouse, to the axis to see if you are dragging in the direction of the axis or not
        float dot = Vector2.Dot(projectTransform2D, Inputs.Instance.MouseClickDrag());
    }
}
