using System;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

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
    /// Scale factor, the length of 1 Unity meter
    /// </summary>
    public const int S = 10000000;
    /// <summary>
    /// Scale factor in decimel form, the length of 1 meter in Unity
    /// </summary>
    public const float SD = 0.0000001f;
    /// <summary>
    /// Scale factor  of time
    /// </summary>
    public const int T = 1000;

    [SerializeField]
    PlanetType massReference;
    public PlanetType MassReference { get { return massReference; } set { massReference = value; } }

    [SerializeField]
    float massMultiplier;

    [SerializeField]
    PlanetType radiusReference;
    public PlanetType RadiusReference { get { return radiusReference; } set { radiusReference = value; } }

    [SerializeField]
    float radiusMultiplier;

    public SpaceController sc { get; set; }

    [SerializeField]
    GameObject model;

    [Header("Kg")]
    [SerializeField]
    float mass;
    public float Mass { get { return mass; } set { mass = value; } }

    //Show in game
    [SerializeField]
    float massIncrease;

    public float RelativeMass { get; set; }

    [Header("meters")]
    [SerializeField]
    float radius;
    public float Radius { get { return radius; } set { radius = value; } }

    [Header("In real world m/s")]
    [SerializeField]
    float initialVelocity;
    public float InitialVelocity { get { return initialVelocity; } set { initialVelocity = value; } }

    //Show in game
    [SerializeField]
    float speed;
    public float Speed { get { return speed; } set { speed = value; } }

    /// <summary>
    /// The sum of all accelerations on the object
    /// </summary>
    [SerializeField]
    Vector3 totalAcceleration;
    public Vector3 TotalAcceleration { get { return totalAcceleration; } set { totalAcceleration = value; } }

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

    /// <summary>
    /// Real world velocity. Not scaled for Unity.
    /// </summary>
    public Vector3 Velocity { get; set; }
    public Vector3 TotalPosition { get; set; }

    /// <summary>
    /// The max acceleration is set to the radius of the planet
    /// </summary>
    float maxAcceleration;
    public float MaxAcceleration { get { return maxAcceleration; } set { maxAcceleration = value; } }

    GameObject arrowClone;

    [SerializeField]
    bool showGravityArrow = true;
    public bool ShowGravityArrow { get { return showGravityArrow; } }

    [SerializeField, Range(0.1f, 10f)]
    float gravityArrowSize = 1f;

    /// <summary>
    /// The total gravity vectors added together for all cb's acting on this
    /// </summary>
    //Vector3 totalGravity = Vector3.zero;

    //Set scale and color among other things
    public void SetProperties()
    {
        //Get space controller reference
        sc = SpaceController.Instance;

        //Get reference to use for mass
        if (MassReference != PlanetType.EnterManually)
        {
            if (massMultiplier ==  0) { massMultiplier = 1; }
            Mass = massMultiplier * sc.GetMass(MassReference);
        }

        //Get reference to use for diameter
        if (RadiusReference != PlanetType.EnterManually)
        {
            if (radiusMultiplier == 0) { radiusMultiplier = 1; }
            Radius = radiusMultiplier * sc.GetDiameter(RadiusReference);
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
        tr.time = sc.UniversalTrailLength;

        //Set Max acceleration based on mass and radius
        maxAcceleration = GetAcceleration(Radius / S, Mass);

        //Set relative mass equal to mass to start
        RelativeMass = Mass;

        //Starting velocity
        Velocity = initialVelocity * transform.forward;
    }

    /// <summary>
    /// Get the acceleration due to gravity at a particular position, with respect to a given mass. (Formula: gravitational constant * mass / radius^2)
    /// </summary>
    /// <param name="differenceUnity"></param>
    /// <param name="mass"></param>
    /// <returns></returns>
    public static float GetAcceleration(float differenceUnity, float mass)
    {
        float r = differenceUnity * S;
        float g = (G * mass) / (r * r);
        return g;
    }

    /// <summary>
    /// Get the mean acceleration due to all relevent celestial bodies, with the direction and magnitude 
    /// </summary>
    public void SetTotalAcceleration()
    {
        Vector3 totalAcceleration = Vector3.zero;
        int celestialBodiesEvaluated = 0;
        foreach (CelestialBody cb in sc.Cb)
        {
            if (cb == this || isKinematic == true || ignoreOwnType && cb.GetType() == this.GetType())
            {
                break;
            }
            celestialBodiesEvaluated++;
            Vector3 difference = cb.transform.position - transform.position;
            float magnitude = difference.magnitude;
            float mass = cb.useRelativeMass ? cb.RelativeMass : cb.Mass;
            totalAcceleration += GetAcceleration(magnitude, mass) * difference.normalized;
        }
        if (celestialBodiesEvaluated != 0)
        {
            TotalAcceleration = totalAcceleration / celestialBodiesEvaluated;
        }

        //Temp manually set acceleration
        //TotalAcceleration = new Vector3(10, 0, 0);
    }

    /// <summary>
    /// Calculate the velocity based on acceleration, in real world units.
    /// </summary>
    /// <param name="totalAcceleration"></param>
    public void SetVelocity()
    {
        Velocity += TotalAcceleration * Time.fixedDeltaTime * T;
    }

    /// <summary>
    /// Apply gravity. Get the distance traveled based on the velocity and total acceleration. Formula: distance = initial velocity * time + 1/2 * acceleration * time^2
    /// </summary>
    public void SetPosition()
    {
        //Get the total acceleration
        SetTotalAcceleration();

        //Distance due to acceleration formula.
        Vector3 distance = (Velocity * Time.fixedDeltaTime * T) + (0.5f * (TotalAcceleration * Mathf.Pow(Time.fixedDeltaTime * T, 2)));

        //Update the velocity figure after determining distance. This is not used in this method to calculate position.
        SetVelocity();

        //Scale the result
        distance *= SD;
        transform.position += distance;
    }

    public void GravityArrow(Vector3 totalAcceleration)
    {
        //Point arrow at average gravity
        if (totalAcceleration.sqrMagnitude > 0.0001f)
        {
            //Get average of all gravity vectors, less this
            Vector3 dir = totalAcceleration.normalized;
            Vector3 offset = dir * 0.1f;
            float scaledDiameter = Radius / S;
            Vector3 start = scaledDiameter * dir + offset + transform.position;
            if (arrowClone == null)
            {
                arrowClone = Instantiate(SpaceController.Instance.ArrowPrefab);
                arrowClone.transform.SetParent(transform, false);
            }
            Quaternion lookAt = Quaternion.LookRotation(dir, Vector3.up);
            arrowClone.transform.SetPositionAndRotation(start, lookAt);
            arrowClone.transform.localScale = new Vector3(gravityArrowSize, gravityArrowSize, gravityArrowSize);
        }
    }

    ///// <summary>
    ///// Modifies the Velocity of this object based on another (cb)
    ///// </summary>
    ///// <param name="cb"></param>
    //public void ApplyGravity(CelestialBody cb)
    //{
    //    if (isKinematic == false && gameObject.name == "Pluto")
    //    {
    //        //Use masss or relative mass
    //        float mass = cb.useRelativeMass ? cb.RelativeMass : cb.Mass;
    //        //Get the distance r from the celestial body
    //        Vector3 difference = cb.transform.position - transform.position;
    //        //Get the current un-clamped acceleration assuming the mass is at a single point
    //        float preClampAcceleration = GetAcceleration(difference.magnitude, mass);
    //        //Calculate and clamp the acceleration due to gravity for one celestial body, clamp to the MaxAcceleration
    //        float accel = Mathf.Clamp(preClampAcceleration, 0f, cb.MaxAcceleration);
    //        accel = 10f;
    //        TotalAcceleration += accel;

    //        Velocity += GetVelocity(accel) * difference.normalized;

    //        //Calculate vector offset per frame. This is the acceleration per second, per second
    //        //Vector3 deltaPos = GetDeltaPos(accel, difference.normalized);

    //        sc.Frames++;

    //        if (showGravityArrow)
    //        {
    //            //totalGravity += deltaPos;
    //        }
    //        //TotalPosition += deltaPos;
    //    }
    //}

    ///// <summary>
    ///// Apply gravity for all celestial bodies that are non kinematic
    ///// </summary>
    //public void TotalGravity() {
    //    //Reset per-frame variables
    //    TotalAcceleration = 0f;
    //    totalGravity = Vector3.zero;
    //    TotalPosition = Vector3.zero;
    //    //For each celestial body
    //    foreach (CelestialBody cb in SpaceController.Instance.Cb) {
    //        if (cb != this && isKinematic == false) {
    //            if (ignoreOwnType) {
    //                if (cb.GetType() != GetType()) {
    //                    ApplyGravity(cb);
    //                }
    //            }
    //            else{
    //                ApplyGravity(cb);
    //            }
    //        }

    //    }
    //    //Point arrow at average gravity
    //    if (showGravityArrow && totalGravity.sqrMagnitude > 0.001f)
    //    {
    //        //Get average of all gravity vectors, less this
    //        totalGravity /= SpaceController.Instance.Cb.Count - 1f;
    //        Vector3 dir = totalGravity.normalized;
    //        Vector3 offset = dir * 0.1f;
    //        float scaledDiameter = (Radius * 2) / S;
    //        Vector3 start = 0.5f * scaledDiameter * dir + offset + transform.position;
    //        GravityArrow(start, dir);
    //    }
    //}

    /// <summary>
    /// Return percentage of mass increase due to speed
    /// </summary>
    /// <param name="celestialBody"></param>
    public float GetRelativeMass(float speed)
    {   
        //if (speed >= c)
        //{
        //    Debug.LogWarning(gameObject.name + " input speed faster than the speed of light");
        //    return massIncrease;
        //}
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
        Speed = Velocity.magnitude;
        //Speed = Mathf.Clamp(Speed, 0f, c);
    }

    ///// <summary>
    ///// Place  an arrow on the model to present the direction of gravity
    ///// </summary>
    //public void GravityArrow(Vector3 startPoint, Vector3 direction)
    //{
    //    if (arrowClone == null)
    //    {
    //        arrowClone = Instantiate(SpaceController.Instance.ArrowPrefab);
    //        arrowClone.transform.SetParent(transform, false);
    //    }
    //    Quaternion lookAt = Quaternion.LookRotation(direction, Vector3.up);
    //    arrowClone.transform.SetPositionAndRotation(startPoint, lookAt);
    //    arrowClone.transform.localScale = new Vector3(gravityArrowSize, gravityArrowSize, gravityArrowSize);
    //}    

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
