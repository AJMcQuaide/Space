using UnityEngine;

[RequireComponent (typeof(TrailRenderer))]
public class CelestialBody : MonoBehaviour
{
    public const float G = 0.0000000000667f; //Gravitational constant
    public const float c = 299792458; //Speed of light
    /// <summary>
    /// Scale factor, the length of 1 Unity metere
    /// </summary>
    public const int S = 10000000;

    [Header("Kg")]
    [SerializeField]
    float mass;
    public float Mass //In KG
    {
        get { return mass; }
    }

    [Header("m")]
    [SerializeField]
    float diameter; //Real world diamters in m
    public float Diameter
    {
        get { return diameter; }
        set { diameter = value; }
    }

    [Header("m/s")]
    [SerializeField]
    float startSpeed;
    public float StartSpeed { get { return startSpeed; } set { startSpeed = value; } }

    [Header("kps")]
    [SerializeField]
    public float speed;

    [Header("Properties")]
    [SerializeField]
    Color planetColor;
    public Color PlanetColor
    {
        get { return planetColor; } 
        set { planetColor = value; }
    }

    [SerializeField]
    Color lineColor;

    [SerializeField]
    bool isKinematic;

    [SerializeField]
    bool warpGrid;
    public bool WarpGrid { get { return warpGrid; } }

    [SerializeField]
    bool ignoreOwnType;
    public bool IgnoreOwnType { get { return ignoreOwnType; } set { ignoreOwnType = value; } }

    public Vector3 Velocity { get; set; }

    public float MaxAcceleration { get; set; }

    //Set scale and color among other things
    public void SetProperties()
    {
        //Scale
        transform.localScale = new Vector3(Diameter / S, Diameter / S, Diameter / S);
        //Set Color
        GetComponentInChildren<MeshRenderer>().sharedMaterial.color = PlanetColor;
        //Set trail renderer color
        TrailRenderer tr = GetComponentInChildren<TrailRenderer>(); 
        tr.material.color = lineColor;
        tr.time = 10f;
        //Set Max acceleration based on mass and radius
        MaxAcceleration = GetAcceleration((Diameter * 0.5f) / S, Mass);
        //Set starting speed
        Velocity = (StartSpeed * Time.fixedDeltaTime) * transform.forward;
    }

    //Set the acceleration due to gravity in m/s^2. Units are m, kg. G is gravitational constent.
    public float GetAcceleration(float differenceUnity, float mass)
    {
        //The actual distance in Unity usually incorrect due to scaling, multiplied by the scale factor to make it true
        float r = differenceUnity * S;
        float g = (G * mass) / (r * r);
        return g;
    }

    /// <summary>
    /// Modifies the Velocity of the object
    /// </summary>
    /// <param name="cb"></param>
    public void ApplyGravity(CelestialBody cb)
    {
        if (isKinematic == false)
        {
            //Get the distance r from the celestial body
            Vector3 difference = cb.transform.position - transform.position;
            //Get the current un-clamped acceleration assuming the mass is at a single point
            float currentAcceleration = GetAcceleration(difference.magnitude, cb.mass);
            //Calculate and clamp the acceleration due to gravity
            float acceleration = Mathf.Clamp(currentAcceleration, 0f, MaxAcceleration);
            //Calculate vector offset per frame
            Vector3 deltaPos = acceleration * Time.fixedDeltaTime * difference.normalized;
            //Calculate velocity and take into account scale factor
            Velocity += deltaPos;
        }
    }

    /// <summary>
    /// Apply gravity for all celestial bodies that are non kinematic
    /// </summary>
    public void ApplyAllGravity()
    {
        foreach (CelestialBody cb in SpaceController.Instance.Cb)
        {
            if (cb != this)
            {
                if (ignoreOwnType)
                {
                    if (cb.GetType() != GetType())
                    {
                        ApplyGravity(cb);
                    }
                }
                else
                {
                    ApplyGravity(cb);
                }
            }
        }
    }

    //Add the object to the Celestial body list
    public void Register(CelestialBody celestialBody)
    {
        SpaceController Instance = SpaceController.Instance;
        if (Instance != null)
        {
            SpaceController.Instance.Cb.Add(celestialBody);
        }
    }

    //Add the object to the Celestial body list
    public void DeRegister()
    {
        SpaceController Instance = SpaceController.Instance;
        if (Instance != null)
        {
            SpaceController.Instance.Cb.Remove(this);
        }
    }
}
