using UnityEngine;

public class Highlighter : MonoBehaviour
{
    [SerializeField]
    CameraController cc;
    MeshRenderer mr;

    GameObject picked;
    public GameObject Picked {  get { return picked; } }

    bool show;
    public bool Show
    {
        get { return show; }
        set
        {
            if (value != show)
            {
                show = value;
                ChangeState(picked);
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
        mr = GetComponentInChildren<MeshRenderer>();
        mr.enabled = false;
    }

    void Update()
    {
        picked = cc.Picking(1 << 6);
        SetPosition(picked);
    }

    /// <summary>
    /// Position the highlighter with the reference object, if it exists
    /// </summary>
    void SetPosition(GameObject pick)
    {
        Show = pick != null;
        if (Show)
        {
            transform.position = pick.transform.position;
        }
    }

    /// <summary>
    /// Change the scale and visibility of the highlighter object based on the picked object
    /// </summary>
    /// <param name="pick"></param>
    public void ChangeState(GameObject pick)
    {
        if (pick != null)
        {
            mr.enabled = true;
            float scale;
            scale = pick.GetComponent<CelestialBody>().Radius * (float)CelestialBody.SD * 2f;
            transform.localScale = new Vector3(scale, scale, scale);
        }
        else
        {
            mr.enabled = false;
        }
    }
}
