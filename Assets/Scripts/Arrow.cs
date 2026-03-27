using System.Drawing;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField]
    GameObject head;

    [SerializeField]
    GameObject tail;

    [SerializeField]
    ArrowType arrowType;

    public float size = 0.5f;

    public ArrowType ArrowType {  get { return arrowType; } set { arrowType = value; } }

    void Start()
    {
        head.GetComponent<MeshRenderer>().material.color = SpaceController.Instance.arrowColors[(int)arrowType];
        tail.GetComponent<MeshRenderer>().material.color = SpaceController.Instance.arrowColors[(int)arrowType];
    }
}