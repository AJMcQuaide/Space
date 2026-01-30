using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpaceController : MonoBehaviour
{
    static SpaceController instance;
    public static SpaceController Instance {
        get {
            if (instance == null) { instance = FindAnyObjectByType<SpaceController>(); }
            return instance;
        }
    }

    /// <summary>
    /// List of default planets in the solar system to reference or instantiate
    /// </summary>
    [SerializeField]
    GameObject[] DefaultCelestialBodies;

    [SerializeField]
    float[] MassArray;
    //Make method for indexer*

    [SerializeField]
    float[] RadiusArray;

    /// <summary>
    /// 1x = Scale factor seconds
    /// </summary>
    [SerializeField]
    float timeMultiplier;
    public float TimeMultiplier { get { return timeMultiplier; } }

    [SerializeField, Range(0f, 1000f)]
    float gridMultiplier;

    [SerializeField]
    float universalTrailLength;
    public float UniversalTrailLength { get { return universalTrailLength; } set { universalTrailLength = value; } }

    //Testing below
    [SerializeField]
    GameObject grid;

    readonly List<Vector3> initial = new();

    List<Vector3> result = new();

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    /// <summary>
    /// Celestial Body list
    /// </summary>
    public List<CelestialBody> Cb { get; set; } = new();

    /// <summary>
    /// A list of the positions of Celestial bodies which have Warp grid set to true
    /// </summary>
    
    public List<Vector4> CBWarpPos { get; set; } = new();
    /// <summary>
    /// A list of the positions of Celestial bodies which have Warp grid set to true
    /// </summary>
    public List<float> CBWarpMass { get; set; } = new();
    /// <summary>
    /// A list of the max acceleration of Celestial bodies which have Warp grid set to true
    /// </summary>
    public List<float> CBMaxAccel { get; set; } = new();

    [SerializeField]
    GameObject arrowPrefab;
    public GameObject ArrowPrefab { get { return arrowPrefab; } }

    //temp
    [SerializeField]
    bool useGPU;

    [SerializeField]
    float frames = 0;
    public float Frames { get { return frames; } set { frames = value; } }
    bool runOnce = false;

    public int simulationLength;

    float timeCount;

    void Awake()
    {
        //Singleton
        if (Instance != this) { Destroy(gameObject); }
        else
        {
            meshFilter = grid.GetComponent<MeshFilter>();
            meshRenderer = grid.GetComponent<MeshRenderer>();
            meshFilter.sharedMesh.GetVertices(initial);
            result = new List<Vector3>(initial);
            Time.timeScale = timeMultiplier;
        }
    }

    private void Start()
    {
        Debug.Log("Grid Count: " + initial.Count);
        Debug.Log("CB Count: " + Cb.Count);
    }

    void FixedUpdate()
    {
        if (useGPU)
        {
            if (runOnce)
            {
                //Reset the vertices prior to GPU use
                meshFilter.mesh.SetVertices(initial);
                runOnce = false;
            }
            SetShader(meshRenderer.material);
        }
        else
        {
            WarpGrid(meshFilter.mesh);
        }
        meshRenderer.material.SetInt("useGPU", useGPU ? 1 : 0);

        if (Frames < simulationLength)
        {
            Frames++;
        }
    }

    private void Update()
    {
        //FPS();
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
                    offset = (float)CelestialBody.GetAcceleration(difference.magnitude, Cb[y].Mass) * gridMultiplier * difference.normalized;
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
        runOnce = true;
    }

    //Set shader properties
    void SetShader(Material material)
    {
        int CountToWarp = 0;
        CBWarpPos.Clear();
        CBWarpMass.Clear();
        CBMaxAccel.Clear();
        foreach (CelestialBody cb in Cb)
        {
            if (cb.WarpGrid)
            {
                CountToWarp++;
                CBWarpPos.Add(cb.transform.position);
                CBWarpMass.Add((float)cb.Mass);
                CBMaxAccel.Add((float)cb.MaxAcceleration);
            }
        }
        if (CBWarpMass.Count > 0)
        {
            material.SetFloat("_GridMultiplier", gridMultiplier);
            material.SetInt("_ScaleFactor", (int)CelestialBody.S);
            material.SetInt("_CBCount", CountToWarp);

            material.SetVectorArray("_Position", CBWarpPos);
            material.SetFloatArray("_Mass", CBWarpMass);
            material.SetFloatArray("_MaxAcceleration", CBMaxAccel);
        }
    }

    void FPS()
    {
        //FPS count only works with time multiplier of 1
        //Use in Update
        if (timeCount >= 1f)
        {
            //Debug.Log("FPS: " + frames / timeCount);
            timeCount = 0;
            frames = 0;
        }
        else
        {
            timeCount += Time.deltaTime;
            frames++;
        }
    }

    public float GetMass(PlanetType planet)
    {
        float mass = MassArray[(int)planet];
        return mass;
    }

    public float GetDiameter(PlanetType planet)
    {
        float diameter = RadiusArray[(int)planet];
        return diameter;
    }
}
