using UnityEngine;

public class Arrow : MonoBehaviour
{
    public Color Color;

    [SerializeField]
    GameObject head;

    [SerializeField]
    GameObject tail;

    void Start()
    {
        head.GetComponent<MeshRenderer>().material.color = Color;
        tail.GetComponent<MeshRenderer>().material.color = Color;
    }
}
