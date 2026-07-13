using UnityEngine;

/// <summary>
/// Stores a vector 3 to be used when attached to an object. Used to tell manipulation tools which way to rotate or move objects.
/// </summary>
public class Vector3Variable : MonoBehaviour
{
    [SerializeField]
    Vector3 value;
    public Vector3 Value { get { return value; } }

    private void Start()
    {
        if (value == Vector3.zero)
        {
            Debug.LogWarning("Stored varible is set to zero");
        }    
    }
}
