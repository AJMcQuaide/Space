using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera cam;

    [SerializeField]
    bool attachToTarget;

    [SerializeField]
    GameObject target;

    Vector3 wideViewPos;
    Quaternion wideViewRot;

    [SerializeField]
    float followDistance = 1f;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null) { Debug.LogWarning("No camera"); }

        wideViewPos = transform.position;
        wideViewRot = transform.rotation;
    }

    void Update()
    {
        if (attachToTarget)
        {
            FollowObject(target);
        }
        else
        {
            cam.transform.position = wideViewPos;
            cam.transform.rotation = wideViewRot;
        }
    }

    void FollowObject(GameObject target)
    {
        cam.transform.position = target.transform.up * followDistance + target.transform.position;
        Quaternion rotation = Quaternion.LookRotation(target.transform.position - cam.transform.position, Vector3.up);
        cam.transform.rotation = rotation;
    }
}
