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

    [SerializeField]
    GameObject rotationTool;

    //[SerializeField]
    //GameObject rotateTool;

    [SerializeField, Range(1f, 2f)]
    float moveSensativity;

    Vector3 normalScale = new(1f, 1f, 1f);
    Vector3 increasedScale = new(1.25f, 1.25f, 1.25f);

    bool isDragging = false;

    /// <summary>
    /// Change between move tool and rotation tool
    /// </summary>
    bool moveToolActive = true;
    public bool MoveToolActive
    {
        get
        { return moveToolActive; }
        set
        {

            moveToolActive = value;
        }
    }

    [SerializeField]
    GameObject picked;
    /// <summary>
    /// Returns whatever object the mouse pointer is hovering over, as long as it is on the specified layer
    /// </summary>
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

    void Start()
    {
        if (cc == null)
        {
            cc = FindAnyObjectByType<CameraController>();
            Debug.Log("Found missing Camera Controller in " + GetType());
        }
        HideTools();
    }

    void Update()
    {
        Picked = cc.Picking(1 << 7);
        KeepApparentSizeOnScreen();

        //If you click, and the camera is tracking an object, and the game is paused or stopped
        if (Mouse.current.leftButton.wasPressedThisFrame && SpaceController.Instance.InPlayMode == false)
        {
            //If the above also coresponds with hovering over a object on the appropriate layer
            if (cc.Picked != null)
            {
                if (MoveToolActive)
                {
                    ShowMoveTool();
                }
                else
                {
                    ShowDirectionTool();
                }
            }
            //If not picking up an object, or clicking outside of one, then hide the manipulation tools
            else
            {
                HideTools();
            }
        }

        //Previous code below*********************

        //if (picked != null && Mouse.current.leftButton.wasPressedThisFrame && Keyboard.current.leftShiftKey.IsPressed())
        //{
        //    isDragging = true;

        //    //Pause when dragging objects if in Play mode
        //    if (SpaceController.Instance.InPlayMode)
        //    {
        //        UIController.Instance.PlayPauseButton();
        //    }
        //}

        //if (isDragging)
        //{
        //    //Perform the move action
        //    if (MoveToolActive)
        //    {
        //        DragObject();
        //    }
        //    //Perform the rotation action
        //    else
        //    {
        //        //RotationTool
        //    }

        //        cc.IsTracking = false;
        //    if (Mouse.current.leftButton.wasReleasedThisFrame)
        //    {
        //        isDragging = false;
        //    }
        //}
        //else
        //{
        //    if (Mouse.current.leftButton.wasPressedThisFrame)
        //    {

        //    }
        //}


    }

    private void LateUpdate()
    {
        //Set position to the camera tracked object
        if (cc.CameraTrackedObject != null)
        {
            transform.position = cc.CameraTrackedObject.transform.position;
        }
    }

    /// <summary>
    /// Keep the aparent size of the object the same regardless of camera distance
    /// </summary>
    public void KeepApparentSizeOnScreen()
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
    /// Move the object based the movement of the mouse compared to the manipulation axis
    /// </summary>
    void DragObject()
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
            Vector3 move = cc.CamDistance * dot * moveSensativity * Time.deltaTime * Picked.transform.localPosition.normalized;

            cc.CameraTrackedObject.transform.position += move;
            cc.CameraTrackedObject.Position += new double3((double)move.x, (double)move.y, (double)move.z);
        }
    }

    /// <summary>
    /// Switch to MoveTool mode
    /// </summary>
    public void ShowMoveTool()
    {
        ShowObject(true, moveTool);
        ShowObject(false, rotationTool);
        MoveToolActive = true;
    }

    /// <summary>
    /// Switch to DirectionTool mode
    /// </summary>
    public void ShowDirectionTool()
    {
        ShowObject(false, moveTool);
        ShowObject(true, rotationTool);
        MoveToolActive = false;
    }

    /// <summary>
    /// Hide all manipulation tools
    /// </summary>
    public void HideTools()
    {
        ShowObject(false, moveTool);
        ShowObject(false, rotationTool);
    }
}
