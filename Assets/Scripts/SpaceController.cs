using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.Mathematics;
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
    /// Celestial body prefabs of various planets, moons or stars
    /// </summary>
    [SerializeField]
    GameObject[] CelestialBodyPrefabs;

    /// <summary>
    /// Celestial Bodies that are loaded/instantiated into the scene
    /// </summary>
    public List<CelestialBody> CelestialBodiesInScene { get; set; } = new();

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
    /// A list of the positions of Celestial bodies which have Warp grid set to true
    /// </summary>
    public List<Vector4> CBWarpPos { get; set; } = new();
    /// <summary>
    /// A list of the mass of Celestial bodies which have Warp grid set to true
    /// </summary>
    public List<float> CBWarpMass { get; set; } = new();
    /// <summary>
    /// A list of the max acceleration of Celestial bodies which have Warp grid set to true
    /// </summary>
    public List<float> CBMaxAccel { get; set; } = new();

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

    bool inPlayMode;
    public bool InPlayMode { get { return inPlayMode; } set { inPlayMode = value; } }

    string savePath;

    [SerializeField, Range(1, 10)]
    float _outlineThickness;
    public float _OutlineThickness {  get { return _outlineThickness; } }

    /// <summary>
    /// The game object which deals with moving and rotating celestial bodies
    /// </summary>
    [SerializeField]
    Manipulation objectManipulation;
    public Manipulation ObjectManipulation { get { return objectManipulation; } }

    //FPS
    [SerializeField]
    TextMeshProUGUI FPS_Text;

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
            if (ObjectManipulation == null)
            {
                Debug.LogWarning("No Manipulation object - searching for one");
                objectManipulation = FindAnyObjectByType<Manipulation>();
            }
        }
    }

    private void Start()
    {
        Debug.Log("Cb Count: " + CelestialBodiesInScene.Count);
        Debug.Log("Save Path: " + savePath);
    }

    void FixedUpdate()
    {
        //Set acceleration and contacts for all Cb's
        SetPhysics();

        //Grid Warp
        SetShader(meshRenderer.material);

        //FPS
        if (Frames < simulationLength && InPlayMode)
        {
            Frames++;
        }
    }

    private void Update()
    {
        FPS();
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
        foreach (CelestialBody cb in CelestialBodiesInScene)
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
        for (int i = 0; i < CelestialBodiesInScene.Count; i++)
        {
            //Determine overall acceleration based on all celestial bodies
            CelestialBodiesInScene[i].TotalAcceleration = CelestialBodiesInScene[i].SetAcceleration(CelestialBodiesInScene[i]);

            //Alter velocity of this object, and the object contacted if there is contact
            CelestialBodiesInScene[i].SetContact(CelestialBodiesInScene[i]);
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
            int fps = (int)(frames / timeCount);
            FPS_Text.text = fps.ToString();
            timeCount = 0;
            frames = 0;
        }
        else
        {
            timeCount += Time.deltaTime;
            frames++;
        }
    }

    ///// <summary>
    ///// Start and pause the simulation
    ///// </summary>
    //public void PlayPause()
    //{
    //    play = !play;
    //}

    /// <summary>
    /// Remove all Celestial Bodies from the app
    /// </summary>
    public void Clear()
    {
        for (int i = CelestialBodiesInScene.Count - 1; i >= 0 ; i--)
        {
            Destroy(CelestialBodiesInScene[i].gameObject);
        }
    }

    /// <summary>
    /// Return a CelestialBody prefab which contains properties. For example, return sun prefab, so that the sun's mass and radius can be read.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public CelestialBody GetCelestialBodyPrefab(int index)
    {
        return CelestialBodyPrefabs[index].GetComponent<CelestialBody>();
    }

    public double3 Vector3ToDouble3(Vector3 vector3)
    {
        double3 double3 = new(vector3.x, vector3.y, vector3.z);
        return double3;
    }

    /// <summary>
    /// Save the current state of the app
    /// </summary>
    /// 
    public void Save()
    {
        Debug.Log("Save App");
        using (var bWriter = new BinaryWriter(File.Open(savePath, FileMode.Create)))
        {
            GameDataWriter writer = new GameDataWriter(bWriter);
            writer.Write(CelestialBodiesInScene.Count);
            for (int i = 0; i < CelestialBodiesInScene.Count; i++)
            {
                CelestialBody cb = CelestialBodiesInScene[i];
                Transform t = cb.transform;

                //Save what type of celestial body this is
                int body = (int)cb.ThisCelestialBody;
                writer.Write(body);

                //Save Name
                writer.Write(cb.gameObject.name);

                //Position and Rotation
                writer.Write(t.position);
                writer.Write(t.rotation);

                //Velocity which will be used as starting velocity
                writer.Write(cb.Velocity);

                //Trail
                writer.Write(cb.TrailColor);
                writer.Write(cb.TrailWidth);

                //Properties
                writer.Write(cb.IsKinematic);
                writer.Write(cb.WarpGrid);
            }
        }
    }

    /// <summary>
    /// Load the saved app
    /// </summary>
    public void Load()
    {
        if (File.Exists(savePath))
        {
            Debug.Log("Load App");
            Clear();

            using (var bReader = new BinaryReader(File.Open(savePath, FileMode.Open)))
            {
                GameDataReader reader = new(bReader);
                int count = reader.ReadInt32();

                for (int i = 0; i < count; i++)
                {
                    //Instantiate and set Radius reference
                    int body = reader.ReadInt32();
                    CelestialBody cb = Instantiate(CelestialBodyPrefabs[body].GetComponent<CelestialBody>());

                    //Load Name
                    cb.gameObject.name = reader.ReadString();

                    //Location and rotation of planet
                    cb.transform.position = reader.ReadVector3();
                    cb.Position = Vector3ToDouble3(cb.transform.position);

                    cb.transform.rotation = reader.ReadQuaternion();

                    //Set initial velocity
                    double3 vel = reader.ReadDouble3();
                    cb.Speed = (float)math.length(vel);

                    //Set Trail
                    cb.TrailColor = reader.ReadColor();
                    cb.TrailWidth = reader.ReadFloat();

                    //Set properties
                    cb.IsKinematic = reader.ReadBool();
                    cb.WarpGrid = reader.ReadBool();
                }
            }
        }
        else
        {
            Debug.Log("No save file to load");
        }
    }
}
