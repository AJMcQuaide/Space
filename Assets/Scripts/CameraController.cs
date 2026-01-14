using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera cam;

    [SerializeField]
    GameObject target;

    [SerializeField]
    float followDistance = 1f;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null) { Debug.LogWarning("No camera"); }
    }

    void Update()
    {
        FollowObject(target);
    }

    void FollowObject(GameObject target)
    {
        cam.transform.position = target.transform.forward * followDistance + target.transform.position;
        Quaternion rotation = Quaternion.LookRotation(-target.transform.forward, Vector3.up);
        cam.transform.rotation = rotation;
    }
}
