using System;
using UnityEngine;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Callbacks;

public class CelestialBody : MonoBehaviour
{
    /// <summary>
    /// Gravitational constant
    /// </summary>
    public const double G = 0.0000000000667f;
    /// <summary>
    /// Speed of light
    /// </summary>
    public const double c = 299792458;
    /// <summary>
    /// Scale factor, the length of 1 Unity meter
    /// </summary>
    public const double S = 10000000;
    /// <summary>
    /// Scale factor in decimel form, the length of 1 meter in Unity
    /// </summary>
    public const double SD = 0.0000001f;

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
    double mass;
    public double Mass { get { return mass; } set { mass = value; } }

    //Show in game
    [SerializeField]
    double lorentzFactor = 1d;
    public double LorentzFactor { get { return lorentzFactor; } }

    public double RelativeMass { get; set; }

    [Header("meters")]
    [SerializeField]
    float radius;
    public float Radius { get { return radius; } set { radius = value; } }

    [Header("In real world m/s")]
    [SerializeField]
    double initialVelocity;

    //Show in game
    [SerializeField]
    float speed;
    public float Speed { get { return speed; } set { speed = value; } }

    /// <summary>
    /// The sum of all accelerations on the object
    /// </summary>
    [SerializeField]
    double3 totalAcceleration;
    public double3 TotalAcceleration { get { return totalAcceleration; } set { totalAcceleration = value; } }

    double3 position;
    public double3 Position { get { return position; } set { position = value; } }

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
    public bool IsKinematic { get { return isKinematic; } }

    [SerializeField]
    bool warpGrid;
    public bool WarpGrid { get { return warpGrid; } }

    [SerializeField]
    bool ignoreOwnType;

    /// <summary>
    /// Real world velocity. Not scaled for Unity.
    /// </summary>
    public double3 Velocity { get; set; }

    /// <summary>
    /// The max acceleration is set to the radius of the planet
    /// </summary>
    double maxAcceleration;
    public double MaxAcceleration { get { return maxAcceleration; } set { maxAcceleration = value; } }

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
        float scale = (Radius * 2) / (float)S;
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
        double3 transformForward = new(transform.forward.x, transform.forward.y, transform.forward.z);

        //Clamp initialvelocity and set to Velocity
        Velocity = math.clamp(initialVelocity, 0d, c-1d) * transformForward;

        //Set position double to the transform at start
        double3 transformPosition = new(transform.position.x, transform.position.y, transform.position.z);
        Position = transformPosition;
    }

    /// <summary>
    /// Get the acceleration due to gravity at a particular position, with respect to a given mass. (Formula: gravitational constant * mass / radius^2)
    /// </summary>
    /// <param name="differenceUnity"></param>
    /// <param name="mass"></param>
    /// <returns></returns>
    public static double GetAcceleration(double differenceUnity, double mass)
    {
        double r = differenceUnity * S;
        double g = (G * mass) / (r * r);
        return g;
    }

    /// <summary>
    /// Get the mean acceleration due to all relevent celestial bodies, with the direction and magnitude 
    /// </summary>
    public void SetTotalAcceleration()
    {
        double3 totalAcceleration = double3.zero;
        int celestialBodiesEvaluated = 0;
        foreach (CelestialBody cb in sc.Cb)
        {
            if (cb != this && isKinematic == false && ignoreOwnType == false || ignoreOwnType && cb.GetType() != this.GetType())
            {
                celestialBodiesEvaluated++;
                double3 cbTransformPosition = new(cb.transform.position.x, cb.transform.position.y, cb.transform.position.z);
                double3 transformPosition = new(transform.position.x, transform.position.y, transform.position.z);
                double3 difference = cbTransformPosition - transformPosition;
                double magnitude = math.length(difference);
                double acceleration = GetAcceleration(magnitude, cb.RelativeMass);
                acceleration = Math.Clamp(acceleration, 0d, cb.MaxAcceleration);
                totalAcceleration += acceleration * math.normalize(difference);
            }
        }
        if (celestialBodiesEvaluated != 0)
        {
            TotalAcceleration = totalAcceleration / celestialBodiesEvaluated;
        }

        //Temp
        TotalAcceleration = new double3(100d, 0d, 0d);
    }

    /// <summary>
    /// Calculate the velocity based on acceleration, in real world units at the end of this frame
    /// </summary>
    /// <param name="totalAcceleration"></param>
    public void SetVelocity()
    {
        Velocity += TotalAcceleration * (double)Time.fixedDeltaTime * sc.TimeScale;
    }

    /// <summary>
    /// Apply gravity. Get the distance traveled based on the velocity and total acceleration. Formula: distance = initial velocity * time + 1/2 * acceleration * time^2
    /// </summary>
    public void SetPosition()
    {
        //Get the total acceleration
        SetTotalAcceleration();

        SetLorentzFactor();
        //Percentage of energy contributing to acceleration
        double relativity = 1d / math.pow(LorentzFactor, 3);

        //Distance due to acceleration formula.
        double3 distance = (Velocity * (double)Time.fixedDeltaTime * sc.TimeScale) + (0.5f * (TotalAcceleration * Math.Pow((double)Time.fixedDeltaTime * sc.TimeScale, 2)));

        //Scale the result
        position += distance * SD * relativity;

        //Update the velocity, which is to be used in the next frame and used as "initial velocity"
        SetVelocity();

        transform.position = new Vector3((float)position.x, (float)position.y, (float)position.z);
    }

    ///// <summary>
    ///// Defunct method
    ///// </summary>
    //public double3 RelativePosition()
    //{
    //    //Time
    //    double t = (double)Time.fixedDeltaTime * sc.TimeScale;
    //    //Speed of light squared divided by acceleration
    //    double c2a = c * c / math.length(TotalAcceleration);
    //    //Acceleration times time, containing the direction of acceleration
    //    double3 at = TotalAcceleration * t;
    //    //Velocity initial times lorentz factor
    //    double3 v0l = Velocity * lorentzFactor;
    //    //v0l plus at
    //    double3 v0latc = (v0l + at) / c;

    //    double3 squared = math.pow(v0latc, 2d);

    //    double3 plus1 = 1d + squared;

    //    double3 x = math.sqrt(plus1);

    //    SetLorentzFactor();
    //    double3 y = x - LorentzFactor;

    //    //Enter into the formula for relavtive position given initial velocity, and constant acceleration
    //    double3 formula = c2a * y;
    //    Debug.Log("Frame: " + sc.Frames  + "  Name: " + gameObject.name  + "  t: " + t + "  c2a: " + c2a + "  v initial: " + Velocity + "  at: " + at + "  v0l: " + v0l + "  v0latc: " + v0latc + "  RETURN: " + formula*SD + "  Velocity: " + Velocity + "  Total Acceleration: " + TotalAcceleration + "  Squaured: " + squared + "  Plus1: " + plus1 + "  x: "  + x + "  y: " + y + "  Lorentz Factor: " + lorentzFactor);

    //    return formula * SD;
    //}

    public void GravityArrow()
    {
        //Point arrow at average gravity
        if (math.lengthsq(TotalAcceleration) > 0.0001f)
        {
            Vector3 totalAcceleration = new((float)TotalAcceleration.x, (float)TotalAcceleration.y, (float)TotalAcceleration.z);
            Vector3 dir = totalAcceleration.normalized;
            Vector3 offset = dir * 0.1f;
            float scaledDiameter = Radius / (float)S;
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

    /// <summary>
    /// Mass increase due to speed
    /// </summary>
    public void SetLorentzFactor()
    {
        double speedSquared = Speed * Speed;
        double lightSquared = c * c;
        double pct = 1f / Math.Sqrt(1f - (speedSquared / lightSquared));
        lorentzFactor = math.clamp(pct, 1, double.MaxValue);
    }

    /// <summary>
    /// Calculate the speed, clamp to the speed of light
    /// </summary>
    public void UpdateSpeed()
    {
        float velocityMagnitude = (float)math.length(Velocity);
        Speed = velocityMagnitude;
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
