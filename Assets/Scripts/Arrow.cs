using Unity.Mathematics;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField]
    ArrowType arrowType;

    CelestialBody body;

    double3 kinematicProperty;

    MeshRenderer mr;

    [SerializeField, Range(0f, 2f)]
    float size;

    bool show;
    public bool Show
    {
        get { return show; }
        set
        {
            if (show != value)
            {
                show = value;
                ShowHide(value);
            }
        }
    }

    void Start()
    {
        body = transform.parent.GetComponent<CelestialBody>();
        mr = GetComponent<MeshRenderer>();
        if (body == null )
        {
            Debug.LogWarning("No celestial body found by arrow");
        }
        kinematicProperty = arrowType == ArrowType.Acceleration ? body.TotalAcceleration : body.Velocity;
        Show = false;
        ShowHide(false);
    }

    private void FixedUpdate()
    {
        kinematicProperty = arrowType == ArrowType.Acceleration ? body.TotalAcceleration : body.Velocity;
        Show = math.lengthsq(kinematicProperty) > 0d ? true : false;
        if (Show)
        {
            SetArrowTransform(kinematicProperty);
        }
    }

    void SetArrowTransform(double3 kinematicProperty)
    {
        Vector3 Vector = new((float)kinematicProperty.x, (float)kinematicProperty.y, (float)kinematicProperty.z);
        Vector3 dir = Vector.normalized;
        Vector3 offset = dir * 0.1f;
        float rad = body.Radius * (float)CelestialBody.SD;
        Vector3 pos = rad * dir + offset + body.gameObject.transform.position;
        Quaternion lookAt = Quaternion.LookRotation(dir, Vector3.up);
        transform.SetPositionAndRotation(pos, lookAt);
    }

    void ShowHide(bool show)
    {
        mr.enabled = show;
        mr.material.color = arrowType == ArrowType.Velocity ? Color.grey : Color.white;
        transform.localScale = new Vector3(size, size, size);
    }
}