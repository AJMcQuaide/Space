using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera cam;

    [SerializeField]
    bool attachToTarget;

    [SerializeField]
    bool rotate;

    [SerializeField, Range(0f, 1f)]
    float rotateSpeed = 0.1f;

    [SerializeField]
    GameObject target;

    Vector3 wideViewPos;
    Quaternion wideViewRot;

    [SerializeField]
    float followDistance = 1f;

    float timer = 0;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null) { Debug.LogWarning("No camera"); }

        wideViewPos = transform.position;
        wideViewRot = transform.rotation;
    }

    void FixedUpdate()
    {
        if (attachToTarget)
        {
            FollowObject(target);
        }
        else if (rotate)
        {
            Rotate();
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
        cam.transform.rotation = LookAtTarget(target.transform.position);
    }

    void Rotate()
    {
        //float t = Time.time * rotateSpeed;
        timer += -Time.deltaTime * rotateSpeed;
        Vector3 offset = new Vector3(Mathf.Sin(timer), 0.2f, Mathf.Cos(timer)) * followDistance;

        //Debug.DrawLine(target.transform.position, target.transform.position + offset);

        cam.transform.rotation = LookAtTarget(target.transform.position);
        cam.transform.position = target.transform.position + offset;

        //cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, LookAtTarget(target.transform.position), Time.deltaTime * 25f);
        //cam.transform.position = Vector3.Lerp(cam.transform.position, target.transform.position + offset, Time.deltaTime * 25f);
    }

    Quaternion LookAtTarget(Vector3 target)
    {
        Quaternion rotation = Quaternion.LookRotation(target - cam.transform.position, Vector3.up);
        return rotation;
    }
}
