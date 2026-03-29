using System.Collections.Generic;
using System.IO;
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

    [SerializeField]
    float frames = 0;
    public float Frames { get { return frames; } set { frames = value; } }

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

    [SerializeField]
    float physicsTimeStep;

    [SerializeField]
    bool play;
    public bool Play { get { return play; } set { play = value; } }

    string savePath;

    public Color[] arrowColors = new Color[(int)ArrowType.Count];

    void Awake()
    {
        //Singleton
        if (Instance != this) { Destroy(gameObject); }
        else
        {
            meshRenderer = grid.GetComponent<MeshRenderer>();

            //Set Fixed Update
            if (physicsTimeStep == 0)
            {
                physicsTimeStep = 0.01f;
            }
            Time.fixedDeltaTime = physicsTimeStep;
            savePath = Path.Combine(Application.persistentDataPath, "saveFile");
        }
        Debug.Log("Cb Count: " + Cb.Count);
    }

    void FixedUpdate()
    {
        //Set acceleration and contacts for all Cb's
        SetPhysics();

        //Grid Warp
        SetShader(meshRenderer.material);

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

    /// <summary>
    /// Get the real world mass in kg of a input planet
    /// </summary>
    /// <param name="planet"></param>
    /// <returns></returns>
    public float GetMass(PlanetType planet)
    {
        float mass = MassArray[(int)planet];
        return mass;
    }

    /// <summary>
    /// Get the real world radius in m of a input planet
    /// </summary>
    /// <param name="planet"></param>
    /// <returns></returns>
    public float GetDiameter(PlanetType planet)
    {
        float diameter = RadiusArray[(int)planet];
        return diameter;
    }

    /// <summary>
    /// Start and pause the simulation
    /// </summary>
    public void PlayPause()
    {
        play = !play;
    }

    /// <summary>
    /// Remove all Celestial Bodies from the app
    /// </summary>
    public void Clear()
    {
        for (int i = Cb.Count - 1; i >= 0 ; i--)
        {
            Debug.Log("Called destroy: " + i + " Cb Count: " + Cb.Count);
            Destroy(Cb[i].gameObject);
        }
    }

    /// <summary>
    /// Save the current state of the app
    /// </summary>
    /// 
    public void Save()
    {
        using (var bWriter = new BinaryWriter(File.Open(savePath, FileMode.Create)))
        {
            GameDataWriter writer = new GameDataWriter(bWriter);
            writer.Write(Cb.Count);
            for (int i = 0; i < Cb.Count; i++)
            {
                CelestialBody body = Cb[i];
                Transform t = body.transform;

                //PlanetType Enum reference
                writer.Write((int)body.MassReference);
                writer.Write((int)body.RadiusReference);

                //Planet property multipliers
                writer.Write(body.MassMultiplier);
                writer.Write(body.RadiusMultiplier);

                //Position
                writer.Write(t.position);

                //Rotation
                writer.Write(t.rotation);

                //Velocity which will be used as starting velocity
                writer.Write(body.Velocity);

                //Planet Color
                writer.Write(body.PlanetColor);

                //Trail
                writer.Write(body.TrailColor);
                writer.Write(body.TrailWidth);

                //Properties
                writer.Write(body.IsKinematic);
                writer.Write(body.IgnoreOwnType);
                writer.Write(body.WarpGrid);
            }
        }
    }

    /// <summary>
    /// Load the saved app
    /// </summary>
    public void Load()
    {
        Clear();
        //using (var bReader = new BinaryReader(File.Open(savePath, FileMode.Open)))
        //{
        //    GameDataReader reader = new GameDataReader(bReader);
        //    int count = reader.ReadInt32();
        //    for (int i = 0; i < count; i++)
        //    {
        //        CelestialBody cb = Instantiate(DefaultCelestialBodies[(int)PlanetType.EnterManually].GetComponent<CelestialBody>());
        //        //Planet information such as radius, and mass
        //        PlanetType mass = (PlanetType)reader.ReadInt32();
        //        cb.MassReference = mass;
        //        PlanetType rad = (PlanetType)reader.ReadInt32();
        //        cb.RadiusReference = rad;

        //        //Planet property multipliers
        //        cb.MassMultiplier = reader.ReadFloat();
        //        cb.RadiusMultiplier = reader.ReadFloat();

        //        //Location and rotation of planet
        //        cb.transform.position = reader.ReadVector3();
        //        cb.transform.rotation = reader.ReadQuaternion();

        //        //Set initial velocity
        //        cb.InitialVelocity = reader.ReadDouble();

        //        //Set Planet color
        //        cb.PlanetColor = reader.ReadColor();

        //        //Set Trail
        //        cb.TrailColor = reader.ReadColor();
        //        cb.TrailWidth = reader.ReadFloat();

        //        //Set properties
        //        cb.IsKinematic = reader.ReadBool();
        //        cb.IgnoreOwnType = reader.ReadBool();
        //        cb.WarpGrid = reader.ReadBool();
        //    }
        //}
    }
}
