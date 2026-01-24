using System;
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

    [SerializeField]
    float massIncrease;

    public float RelativeMass { get; set; }

    [Header("meters")]
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

    /// <summary>
    /// The max acceleration is set to the radius of the planet
    /// </summary>
    [SerializeField]
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
        //Scale
        transform.localScale = new Vector3(Diameter / S, Diameter / S, Diameter / S);
        //Set Color
        GetComponentInChildren<MeshRenderer>().material.color = PlanetColor;
        //Set trail renderer color
        TrailRenderer tr = GetComponentInChildren<TrailRenderer>(); 
        tr.material.color = lineColor;
        tr.time = SpaceController.Instance.TrailLength * SpaceController.Instance.TimeMultiplier;
        tr.widthMultiplier = 0.1f;
        //Set Max acceleration based on mass and radius
        maxAcceleration = GetAcceleration(Diameter * 0.5f / S, Mass);
        //Set starting speed, clamp to the speed of light
        StartSpeed = Mathf.Clamp(StartSpeed, 0f, c / S);
        Velocity = StartSpeed * transform.forward;
        //Set relative mass
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
        if (showGravityArrow && totalGravity != Vector3.zero)
        {
            //Get average of all gravity vectors, less this
            totalGravity /= SpaceController.Instance.Cb.Count - 1f;
            Vector3 start = 0.5f * (Diameter / S) * totalGravity.normalized + transform.position;
            Vector3 dir = totalGravity.normalized;
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
