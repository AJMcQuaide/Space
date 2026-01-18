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

    /// <summary>
    /// List of default planets in the solar system to reference or instantiate
    /// </summary>
    [SerializeField]
    GameObject[] DefaultPlanets;

    [SerializeField]
    MeshRenderer meshRenderer;

    /// <summary>
    /// 1x = Scale factor seconds
    /// </summary>
    [SerializeField]
    float timeMultiplier;
    public float TimeMultiplier { get { return timeMultiplier; } }

    [SerializeField, Range(0f, 1000f)]
    float gridMultiplier;

    [SerializeField]
    float trailLength;
    public float TrailLength { get { return trailLength; } set { trailLength = value; } }

    //Testing below
    [SerializeField]
    GameObject grid;

    List<Vector3> initial = new List<Vector3>();

    List<Vector3> result;

    MeshFilter meshFilter;

    public List<CelestialBody> Cb { get; set; } = new List<CelestialBody>();

    [SerializeField]
    GameObject arrowPrefab;
    public GameObject ArrowPrefab { get { return arrowPrefab; } }

    //temp
    [SerializeField]
    int frameCounter = 0;
    public int FrameCounter { get { return frameCounter; } set { frameCounter = value; } }

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

    void FixedUpdate()
    {
        WarpGrid(meshFilter.mesh);

        frameCounter++;
    }

    //Apply a warp to then grid to show the effects of gravity
    void WarpGrid(Mesh mesh)
    {
        if (mesh == null)
        {
            return;
        }
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
        mesh.SetVertices(result);
    }
}
