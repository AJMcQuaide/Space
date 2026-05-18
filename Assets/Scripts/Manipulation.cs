using TMPro;
using UnityEditor.Experimental.GraphView;
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

    //The 3 axis of manipulation
    [SerializeField]
    GameObject moveTool;

    //[SerializeField]
    //GameObject rotateTool;

    Vector3 normalScale = new (1f, 1f, 1f);
    Vector3 increasedScale = new (1.25f, 1.25f, 1.25f);

    [SerializeField]
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
                HideThis(hide);
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
        transform.position = cc.Target.position;
    }

    void Update()
    {
        Picked = cc.Picking(1 << 7);
        //Set the visibility of the tool
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Hide = picked != null;
        }

        if (Hide == false)
        {
            KeepSize();
            if (picked != null)
            {
                picked.transform.localScale = increasedScale;
            }
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
    void HideThis(bool show)
    {
        moveTool.SetActive(show);
    }
}
