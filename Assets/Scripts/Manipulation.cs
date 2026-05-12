using UnityEngine;

/// <summary>
/// For manipulation of game objects in runtime
/// </summary>
public class Manipulation : MonoBehaviour
{
    [SerializeField]
    float size;

    [SerializeField]
    CameraController cc;

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
        KeepSize();
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
}
