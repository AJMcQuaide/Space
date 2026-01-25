using System;
using UnityEngine;

[ExecuteAlways]
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

    [SerializeField]
    PlanetType massReference;
    public PlanetType MassReference { get { return massReference; } set { massReference = value; } }

    [SerializeField]
    PlanetType diameterReference;
    public PlanetType DiameterReference { get { return diameterReference; } set { diameterReference = value; } }

    public SpaceController sc { get; set; }

    [SerializeField]
    GameObject model;

    [Header("Kg")]
    [SerializeField]
    float mass;
    public float Mass { get { return mass; } set { mass = value; } }

    //Show in game
    float massIncrease;

    public float RelativeMass { get; set; }

    [Header("meters")]
    [SerializeField]
    float radius;
    public float Radius { get { return radius; } set { radius = value; } }

    [Header("m/s")]
    [SerializeField]
    float startSpeed;
    public float StartSpeed { get { return startSpeed; } set { startSpeed = value; } }

    //Show in game
    float speed;
    public float Speed { get { return speed; } set { speed = value; } }

    /// <summary>
    /// The sum of all accelerations on the object
    /// </summary>
    [SerializeField]
    float acceleration;
    public float Acceleration { get { return acceleration; } set { acceleration = value; } }

    [Header("Properties")]
    [SerializeField]
    Color planetColor;
    public Color PlanetColor {  get { return planetColor; } }

    [SerializeField]
    Color trailColor;

    [SerializeField]
    float trailWidth;

    [SerializeField]
    bool isKinematic;

    [SerializeField]
    bool warpGrid;
    public bool WarpGrid { get { return warpGrid; } }

    [SerializeField]
    bool ignoreOwnType;

    [SerializeField]
    bool useRelativeMass = true;
    public bool UseRelativeMass { get { return useRelativeMass; } }

    public Vector3 Velocity { get; set; }

    /// <summary>
    /// The max acceleration is set to the radius of the planet
    /// </summary>
    float maxAcceleration;
    public float MaxAcceleration { get { return maxAcceleration; } set { maxAcceleration = value; } }

    GameObject arrowClone;

    [SerializeField]
    bool showGravityArrow = true;

    [SerializeField, Range(0.1f, 10f)]
    public float gravityArrowSize = 1f;

    /// <summary>
    /// The total gravity vectors added together for all cb's acting on this
    /// </summary>
    Vector3 totalGravity = Vector3.zero;

    //Set scale and color among other things
    public void SetProperties()
    {
        //Get space controller reference
        sc = SpaceController.Instance;

        //Get reference to use for mass
        if (MassReference != PlanetType.Custom)
        {
            Mass = sc.GetMass(MassReference);
        }

        //Get reference to use for diameter
        if (DiameterReference != PlanetType.Custom)
        {
            Radius = sc.GetDiameter(DiameterReference);
        }

        //Set the scale of the model
        float scale = (Radius * 2) / S;
        model.transform.localScale = new Vector3(scale, scale, scale);

        //Set Color
        MaterialPropertyBlock colorProperty = new();
        colorProperty.SetColor("_Color", planetColor);
        model.GetComponent<MeshRenderer>().SetPropertyBlock(colorProperty);

        //Set trail renderer color
        TrailRenderer tr = GetComponent<TrailRenderer>();
        MaterialPropertyBlock trailProperty = new();
        trailProperty.SetColor("_Color", trailColor);
        tr.SetPropertyBlock(trailProperty);
        tr.widthMultiplier = trailWidth;

        //Set Max acceleration based on mass and radius
        maxAcceleration = GetAcceleration(Radius / S, Mass);

        //Set starting speed, clamp to the speed of light
        StartSpeed = Mathf.Clamp(StartSpeed, 0f, c / S);
        Velocity = StartSpeed * transform.forward;

        //Set relative mass equal to mass to start
        RelativeMass = Mass;
    }

    //Set the acceleration due to gravity in m/s^2. Units are m, kg. G is gravitational constent.
    public static float GetAcceleration(float differenceUnity, float mass)
    {
        float r = differenceUnity * S;
        float g = (G * mass) / (r * r);
        return g;
    }

    /// <summary>
    /// Modifies the Velocity of this object based on another
    /// </summary>
    /// <param name="cb"></param>
    public void ApplyGravity(CelestialBody cb)
    {
        if (isKinematic == false)
        {
            //Use masss or relative mass
            float mass = cb.useRelativeMass ? cb.RelativeMass : cb.Mass;
            //Get the distance r from the celestial body
            Vector3 difference = cb.transform.position - transform.position;
            //Get the current un-clamped acceleration assuming the mass is at a single point
            float preClampAcceleration = GetAcceleration(difference.magnitude, mass);
            //Calculate and clamp the acceleration due to gravity for one celestial body, clamp to the MaxAcceleration
            float accel = Mathf.Clamp(preClampAcceleration, 0f, cb.MaxAcceleration);
            Acceleration += accel;
            //Calculate vector offset per frame
            Vector3 deltaPos = accel * difference.normalized;
            if (showGravityArrow)
            {
                totalGravity += deltaPos;
            }
            //Calculate velocity in m/s without scale factor
            Velocity += deltaPos * Time.fixedDeltaTime;
        }
    }

    /// <summary>
    /// Apply gravity for all celestial bodies that are non kinematic
    /// </summary>
    public void ApplyAllGravity() {
        //Reset per-frame variables
        Acceleration = 0f;
        totalGravity = Vector3.zero;
        //For each celestial body
        foreach (CelestialBody cb in SpaceController.Instance.Cb) {
            if (cb != this && isKinematic == false) {
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
        //Point arrow at average gravity
        if (showGravityArrow && totalGravity.sqrMagnitude > 0.001f)
        {
            //Get average of all gravity vectors, less this
            totalGravity /= SpaceController.Instance.Cb.Count - 1f;
            Vector3 dir = totalGravity.normalized;
            Vector3 offset = dir * 0.1f;
            float scaledDiameter = (Radius * 2) / S;
            Vector3 start = 0.5f * scaledDiameter * dir + offset + transform.position;
            GravityArrow(start, dir);
        }
    }

    /// <summary>
    /// Return percentage of mass increase due to speed
    /// </summary>
    /// <param name="celestialBody"></param>
    public float CalculateRelativeMass(float speed)
    {   
        if (speed >= c)
        {
            Debug.LogWarning(gameObject.name + " input speed faster than the speed of light");
            return 1;
        }
        float speedSquared = speed * speed;
        float lightSquared = c * c;
        float pct = 1f / Mathf.Sqrt(1f - (speedSquared / lightSquared));
        massIncrease = pct;
        return pct;
    }

    /// <summary>
    /// Calculate the speed, clamp to the speed of light
    /// </summary>
    public void UpdateSpeed()
    {
        Speed = Velocity.magnitude * S;
        Speed = Mathf.Clamp(Speed, 0f, c);
    }

    /// <summary>
    /// Place  an arrow on the model to present the direction of gravity
    /// </summary>
    public void GravityArrow(Vector3 startPoint, Vector3 direction)
    {
        if (arrowClone == null)
        {
            arrowClone = Instantiate(SpaceController.Instance.ArrowPrefab);
            arrowClone.transform.SetParent(transform, false);
        }
        Quaternion lookAt = Quaternion.LookRotation(direction, Vector3.up);
        arrowClone.transform.SetPositionAndRotation(startPoint, lookAt);
        arrowClone.transform.localScale = new Vector3(gravityArrowSize, gravityArrowSize, gravityArrowSize);
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
