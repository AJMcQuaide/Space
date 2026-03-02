using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    Transform target;

    void Start()
    {
        target = FindAnyObjectByType<CameraController>().transform;
    }

    void Update()
    {
        transform.LookAt(target);
    }
}
