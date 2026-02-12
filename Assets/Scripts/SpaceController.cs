using JetBrains.Annotations;
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

    bool useGPU = true;

    [SerializeField]
    float frames = 0;
    public float Frames { get { return frames; } set { frames = value; } }
    bool runOnce = false;

    /// <summary>
    /// How many frames the simulation lasts
    /// </summary>
    public int simulationLength;

    /// <summary>
    /// The multiplier for time in the simulation
    /// </summary>
    public double TimeScale;

    float timeCount;

    [SerializeField]
    bool contactsEnabled;
    public bool ContactsEnabled { get { return contactsEnabled; } }

    [SerializeField]
    bool useGravity;
    public bool UseGravity { get { return useGravity; } }

    void Awake()
    {
        //Singleton
        if (Instance != this) { Destroy(gameObject); }
        else
        {
            //Set Mesh if using CPU
            meshFilter = grid.GetComponent<MeshFilter>();
            meshRenderer = grid.GetComponent<MeshRenderer>();
            meshFilter.sharedMesh.GetVertices(initial);
            result = new List<Vector3>(initial);

            //Set Fixed Update
            Time.fixedDeltaTime = 0.01f;
        }
    }

    private void Start()
    {
        Debug.Log("Grid Count: " + initial.Count);
        Debug.Log("CB Count: " + Cb.Count);
    }

    void FixedUpdate()
    {
        //Set acceleration and contacts for all Cb's
        SetPhysics();

        //Grid Warp
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

        //FPS
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

    /// <summary>
    /// Set the shader properties for warp grid
    /// </summary>
    /// <param name="material"></param>
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

    /// <summary>
    /// Set physics properties of all celestrial bodies
    /// </summary>
    public void SetPhysics()
    {
        for (int i = 0; i < Cb.Count; i++)
        {
            //Determine overall acceleration based on all celestial bodies
            Cb[i].TotalAcceleration = Cb[i].SetAcceleration(Cb[i]);

            //Alter velocity of this object, and the object contacted if there is contact
            Cb[i].SetContact(Cb[i]);
        }
    }

    /// <summary>
    /// Calculate app FPS
    /// </summary>
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
