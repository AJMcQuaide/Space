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
    public GameObject MoveTool {  get { return moveTool; } }

    [SerializeField]
    GameObject rotationTool;
    public GameObject RotationTool { get { return rotationTool; } }

    Vector3Variable rotationAxis = null;

    [SerializeField, Range(0.01f, 0.50f)]
    float moveSensativity;

    [SerializeField, Range(5f, 30f)]
    float rotateSensativity;

    Vector3 normalScale = new(1f, 1f, 1f);
    Vector3 increasedScale = new(1.25f, 1.25f, 1.25f);

    bool isDragging = false;
    public bool IsDragging { get { return isDragging; } }

    /// <summary>
    /// Change between move tool and rotation tool
    /// </summary>
    bool moveToolActive = true;
    public bool MoveToolActive
    {
        get { return moveToolActive; }
        set { moveToolActive = value; }
    }

    [SerializeField]
    GameObject picked;
    /// <summary>
    /// Stores the current picked object
    /// </summary>
    public GameObject Picked
    {
        get { return picked; }
        set 
        {
            if (value != picked && isDragging == false)
            {
                //Debug.Log("1");
                if (value != null)
                {
                    //Debug.Log("Increased");
                    if (picked != null)
                    {
                        //Debug.Log("Normal 1");
                        picked.transform.localScale = normalScale;
                    }
                    picked = value;
                    picked.transform.localScale = increasedScale;
                }
                else
                {
                    //Debug.Log("Normal 2");
                    picked.transform.localScale = normalScale;
                    picked = value;
                }
            }
        }
    }

    /// <summary>
    /// The hit location the Picking tool, updated every frame as an output of the Picking tool
    /// </summary>
    Vector3 hitPos;

    /// <summary>
    /// The hit location the Picking tool, frozen when you click the mouse on the obejct and not updated until clicking again
    /// </summary>
    Vector3 hitPosFrozen;

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
        Picked = cc.Picking(1 << 7, out hitPos);
        KeepApparentSizeOnScreen();

        if (SpaceController.Instance.InPlayMode == false)
        {
            if (isDragging)
            {
                if (MoveToolActive)
                {
                    DragObject();
                }
                else
                {
                    RotateObject();
                }
            }
            //If you click, and the camera is tracking an object, and the game is paused or stopped
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                //Check if there is a picked manipulation tool object if the tool is visible
                if (Picked != null)
                {
                    isDragging = true;
                    //Stop the camera from tracking the object on drag to reposition
                    cc.TrackObject = false;
                    //Grab the clicked location once there is a click
                    hitPosFrozen = hitPos;
                }
                //Check if there is a picked celestial body which can show/hide the manipulation tools
                else if (cc.Picked != null)
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
            //Any click release cancels dragging operation
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                if (moveToolActive == false)
                {
                    //Update the velocity of the celestiabl body after using rotation tool
                    //cc.CameraTrackedObject.ResetVelocity();
                }
                isDragging = false;
                rotationAxis = null;
            }
        }
        else
        {
            HideTools();
        }

        //Debug
        //if (cc.CameraTrackedObject != null)
        //{
        //    Debug.DrawLine(cc.CameraTrackedObject.transform.position, cc.CameraTrackedObject.transform.position + hitPosFrozen, Color.red);
        //}
        //Debug.LogWarning("Mouse Position: " + Mouse.current.delta.ReadValue().magnitude);
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
        float dot = MouseDragAngleDotProduct(picked.transform.localPosition);
        if (dot != 0)
        {
            //Drag the object based on the dot product (drag direction vs the tool's arrow), and the direction the arrow points (it's local space locations)
            Vector3 move = cc.CamDistance * dot * moveSensativity * Time.deltaTime * Picked.transform.localPosition.normalized;

            cc.CameraTrackedObject.transform.position += move;
            cc.CameraTrackedObject.Position += new double3((double)move.x, (double)move.y, (double)move.z);
        }
    }

    /// <summary>
    /// Compare the angle of the mouse drag, to the 'flattened' local Vector of the object, and return the dot product
    /// </summary>
    /// <param name="mousePos"></param>
    float MouseDragAngleDotProduct(Vector3 mousePos)
    {
        //The local vector3 is projected onto the camera plane, the projection still exists in 3D space so it moves around with the camera
        Vector3 project = Vector3.ProjectOnPlane(mousePos, cc.Cam.transform.forward).normalized;

        //The projection is transformed local to the camera space
        Vector3 projectTransform = cc.Cam.transform.InverseTransformDirection(project);

        //Flatten the Vector to make it 2D
        Vector2 projectTransform2D = new(projectTransform.x, projectTransform.y);

        //Compare the click and drag of the mouse, to the axis to see if you are dragging in the direction of the axis or not
        return Vector2.Dot(Inputs.Instance.MouseDrag, projectTransform2D);
    }

    /// <summary>
    /// Rotate the object with click and drag
    /// </summary>
    void RotateObject()
    {
        //Create cross product from camera foward, hitPos, and the output goes into the method.
        Vector3 cross = Vector3.Cross(cc.transform.forward, hitPosFrozen).normalized;
        //Get the rotation vector stored in the picked object
        if (rotationAxis == null)
        {
            rotationAxis = picked.GetComponent<Vector3Variable>();
        }

        ///Start The following code is designed to make sure the rotation rings pull in the intended direction in all viewpoints or orientations
        //The dot product that helps get the right orientiaton, so that pushing/pulling goes in the right direction
        float orientation = Vector3.Dot(cc.transform.forward, Picked.transform.up);
        float flipOrientation = orientation < 0 ? 1 : -1;
        //Only the "Y" ring needs fliped opposite of the others
        if (rotationAxis.Value == Vector3.up)
        {
            flipOrientation = -flipOrientation;
        }
        ///End

        //The local click position on the object, compared to mouse drag
        float dot = MouseDragAngleDotProduct(cross);

        //Rotate the object based on the vector3 information stored in the object, in this case it stores rotation information for X, Y, and Z
        Vector3 rotation = (dot * flipOrientation * rotateSensativity * Mouse.current.delta.ReadValue().magnitude * Time.deltaTime) * rotationAxis.Value;

        //rotationTool.transform.Rotate(rotation);
        rotationTool.transform.Rotate(rotation);

        //Update the velocity of the celestiabl body after using rotation tool
        cc.CameraTrackedObject.ResetVelocity(cc.CameraTrackedObject.Speed);

        ////Debug
        ////Debug.Log("Rotation value " + rotation);
        //Debug.DrawLine(picked.transform.position, picked.transform.position + hitPosFrozen, Color.red);
        //Debug.DrawLine(picked.transform.position + hitPosFrozen, picked.transform.position + hitPosFrozen + cross, Color.yellow);
        //Debug.DrawLine(picked.transform.position, picked.transform.position + picked.transform.forward, Color.blue);
        ////Debug.LogWarning("Dot mouse drag and cross: " + dot);
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

        //Set the direction tool to the direction (velocity) of the celestial body the camera is tracking
        CelestialBody cb = cc.CameraTrackedObject;
        Vector3 cbDirection = new ((float)cb.Velocity.x, (float)cb.Velocity.y, (float)cb.Velocity.z);
        if (cbDirection.sqrMagnitude > 0)
        {
            Quaternion look = Quaternion.LookRotation(cbDirection.normalized, Vector3.up);
            rotationTool.transform.rotation = look;
        }
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
