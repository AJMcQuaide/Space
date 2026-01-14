using UnityEngine;

[RequireComponent (typeof(TrailRenderer))]
public class CelestialBody : MonoBehaviour
{
    /// <summary>
    /// Gravitational constant
    /// </summary>
    public const float G = 0.0000000000667f;
    /// <summary>
    /// Speed of light
    /// </summary>
    public const float c = 299792458;
    /// <summary>
    /// Scale factor, the length of 1 Unity metere
    /// </summary>
    public const int S = 10000000;

    [Header("Kg")]
    [SerializeField]
    float mass;
    public float Mass { get { return mass; } set { mass = value; } }

    public float RelativeMass { get; set; }

    [Header("scaled meters")]
    [SerializeField]
    float diameter;
    public float Diameter { get { return diameter; } set { diameter = value; } }

    [Header("m/s")]
    [SerializeField]
    float startSpeed;
    public float StartSpeed { get { return startSpeed; } set { startSpeed = value; } }

    [Header("m/s")]
    [SerializeField]
    float speed;
    public float Speed { get { return speed; } set { speed = value; } }

    [Header("Properties")]
    [SerializeField]
    Color planetColor;
    public Color PlanetColor {  get { return planetColor; } }

    [SerializeField]
    Color lineColor;

    [SerializeField]
    bool isKinematic;

    [SerializeField]
    bool warpGrid;
    public bool WarpGrid { get { return warpGrid; } }

    [SerializeField]
    bool ignoreOwnType;

    [SerializeField]
    bool useRelativeMass;
    public bool UseRelativeMass { get { return useRelativeMass; } }

    public Vector3 Velocity { get; set; }

    public float MaxAcceleration { get; set; }

    //Set scale and color among other things
    public void SetProperties()
    {
        //Scale
        transform.localScale = new Vector3(Diameter / S, Diameter / S, Diameter / S);
        //Set Color
        GetComponentInChildren<MeshRenderer>().material.color = PlanetColor;
        //Set trail renderer color
        TrailRenderer tr = GetComponentInChildren<TrailRenderer>(); 
        tr.material.color = lineColor;
        tr.time = SpaceController.Instance.TrailLength;
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
            float mass = useRelativeMass ? cb.RelativeMass : cb.Mass;
            //Get the distance r from the celestial body
            Vector3 difference = cb.transform.position - transform.position;
            //Get the current un-clamped acceleration assuming the mass is at a single point
            float currentAcceleration = GetAcceleration(difference.magnitude, mass);
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
    public void ApplyAllGravity() {
        foreach (CelestialBody cb in SpaceController.Instance.Cb) {
            if (cb != this) {
                if (ignoreOwnType) {
                    if (cb.GetType() != GetType()) {
                        ApplyGravity(cb);
                    }
                }
                else{
                    ApplyGravity(cb);
                }
            }
        }
    }

    /// <summary>
    /// Return percentage of mass increase due to speed
    /// </summary>
    /// <param name="celestialBody"></param>
    public float CalculateRelativeMass(float speed)
    {
        float pct = 1f / Mathf.Sqrt(1f - (speed * speed) / (c * c));
        Debug.Log("Mass increased by " + pct);
        return pct;
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
