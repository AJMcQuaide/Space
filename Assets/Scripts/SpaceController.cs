using System.Collections.Generic;
using UnityEngine;

public class SpaceController : MonoBehaviour
{
    static SpaceController instance;
    public static SpaceController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<SpaceController>();
            }
            return instance;
        }
    }

    [SerializeField]
    MeshRenderer mr;

    [SerializeField]
    CelestialBody Reference;

    [SerializeField]
    float timeMultiplier; //1 second = TimeMultiplier seconds
    public float TimeMultiplier { get { return timeMultiplier; } }

    [SerializeField, Range(0f, 1000f)]
    float gridMultiplier; //Adjust the scale gravity warp of the grid

    //Testing below
    [SerializeField]
    GameObject grid;

    List<Vector3> initial = new List<Vector3>();

    List<Vector3> result;

    MeshFilter meshFilter;

    public List<CelestialBody> Cb { get; set; } = new List<CelestialBody>();

    void Awake()
    {
        //Singleton
        if (Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            //Get the local mesh
            meshFilter = grid.GetComponent<MeshFilter>();
            meshFilter.sharedMesh.GetVertices(initial);
            result = new List<Vector3>(initial);
        }

        Time.timeScale = timeMultiplier;
    }

    void LateUpdate()
    {
        WarpGrid();
        //Debug.Log("FPS " + 1f / Time.deltaTime);
        //Debug.Log("CB List: " + Cb.Count);
    }

    //Apply a warp to then grid to show the effects of gravity
    void WarpGrid()
    {
        Vector3 offset;
        //For each vertex in the grid
        for (int i = 0; i < initial.Count; i++)
        {
            Vector3 totalOffset = Vector3.zero;
            //For each celestial body
            for (int y = 0; y < Cb.Count; y++)
            {
                if (Cb[y].WarpGrid)
                {
                    //Distance Vector from the mesh vertex to the celestial body
                    Vector3 difference = Cb[y].transform.position - grid.transform.TransformPoint(initial[i]);
                    //Warp the mesh using the acceleration due to gravity at the vertex of all celestial bodies
                    offset = Cb[y].GetAcceleration(difference.magnitude, Cb[y].Mass) * gridMultiplier * difference.normalized;
                    if (offset.sqrMagnitude > difference.sqrMagnitude)
                    {
                        offset = difference;
                    }
                    //Combine
                    totalOffset += offset;
                    result[i] = initial[i] + totalOffset;
                }
            }
        }
        //Set the gravity distortion
        meshFilter.mesh.SetVertices(result);
    }
}
