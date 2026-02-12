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

    Vector3 previousPosition;
    public Vector3 PreviousPosition { get { return previousPosition; } }

    public bool ContactChecked { get; set; } = false;

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
        Velocity = math.clamp(initialVelocity, 0d, c * 0.99999d) * transformForward;

        //Set position double to the transform at start
        double3 transformPosition = new(transform.position.x, transform.position.y, transform.position.z);
        Position = transformPosition;

        //Set previous position equal to starting position
        previousPosition = transform.position;
    }

    /// <summary>
    /// Loop through all Celestial bodies and check for contacts and apply a reflection velocity if needed.
    /// </summary>
    public void SetContact(CelestialBody cb1)
    {
        //Break out of another cb already contacted this and set it's velocity
        if (cb1.ContactChecked == true)
        {
            //Debug.Log(gameObject.name + " Skipped check");
            cb1.ContactChecked = false;
            return;
        }

        //Loop this (cb1) through other (cb2)
        foreach (CelestialBody cb2 in sc.Cb)
        {
            if (cb2 != cb1)
            {
                //Convert to double
                double3 p2 = new(cb2.transform.position.x, cb2.transform.position.y, cb2.transform.position.z);
                double3 p1 = new(cb1.position.x, cb1.position.y, cb1.position.z);
                double3 difference = p2 - p1;

                //Debug
                Debug.DrawLine(transform.position, transform.position + new Vector3((float)Velocity.x * 0.0005f, (float)Velocity.y * 0.0005f, (float)Velocity.z * 0.0005f));

                //Contact and calculation reflection
                if (math.length(difference) < (cb2.Radius * SD + Radius * SD))
                {
                    //Debug.Log("Before: " + gameObject.name + " " + cb1.Velocity + " Cb2: " + cb2.Velocity);

                    //Calculate the velocity of the cb being evaluated
                    double3 iv1 = cb1.Velocity;
                    double3 iv2 = cb2.Velocity;
                    double3 v1 = Reflect(difference, iv1, iv2, cb1.Mass, cb2.Mass);
                    cb1.Velocity = v1;
                    //Calculate the velocity of the other cb that was contacted
                    double3 v2 = Reflect(difference, iv2, iv1, cb2.Mass, cb1.Mass);
                    cb2.Velocity = v2;
                    cb2.ContactChecked = true;

                    //Debug.Log("After: " + gameObject.name + " " + cb1.Velocity + " Cb2: " + cb2.Velocity);
                }
            }
        }
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
    /// Loop through all Celestial bodies and set total acceleration 
    /// </summary>
    public double3 SetAcceleration(CelestialBody _thisCB)
    {
        double3 accelerationVector = double3.zero;
        if (sc.UseGravity == false)
        {
            return accelerationVector;
        }
        int celestialBodiesEvaluated = 0;

        //Loop this Cb through all other Cb's
        foreach (CelestialBody cb in sc.Cb)
        {
            if (cb != _thisCB && _thisCB.isKinematic == false && _thisCB.ignoreOwnType == false || _thisCB.ignoreOwnType && cb.massReference != _thisCB.massReference)
            {
               //Convert to double
                double3 otherCB = new(cb.PreviousPosition.x, cb.PreviousPosition.y, cb.PreviousPosition.z);
                double3 thisCB = new(_thisCB.transform.position.x, _thisCB.transform.position.y, _thisCB.transform.position.z);
                double3 difference = otherCB - thisCB;

                //Acceleration
                celestialBodiesEvaluated++;
                double magnitude = math.length(difference);
                double acceleration = GetAcceleration(magnitude, cb.RelativeMass);
                acceleration = Math.Clamp(acceleration, 0d, cb.MaxAcceleration);
                accelerationVector += acceleration * math.normalize(difference);

                //Debug.Log("Frames: " + sc.Frames + " Object: " + gameObject.name + " Has distance of: " + magnitude + " Other: " + otherCB + " This: " + thisCB);
            }
        }
        return accelerationVector;
    }

    /// <summary>
    /// Calculate the reflection vector on collision of a rigid sphere
    /// </summary>
    /// <param name="n"></param>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="m1"></param>
    /// <param name="m2"></param>
    /// <returns></returns>
    public double3 Reflect(double3 n, double3 v1, double3 v2, double m1, double m2)
    {
        n = math.normalize(n);
        double3 vDelta = v1 - v2;
        double vDeltaDotN = math.dot(vDelta, math.normalize(n));
        double massDelta = (2 * m2) / (m1 + m2);
        double3 reflection = v1 - (massDelta * vDeltaDotN * n);

        //if (gameObject.name == "A")
        //{
        //    Debug.Log("");
        //}
        //Debug.Log(gameObject.name + " Input velocity: " + v1 + " reflection: " + reflection);

        return reflection;
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
        ////Check for contacts
        //SetContact();

        ////Get the total acceleration and contacts
        //SetAcceleration();

        //Percentage of energy contributing to acceleration
        SetLorentzFactor();
        double relativity = 1d / math.pow(LorentzFactor, 3);

        //Distance due to acceleration formula.
        double3 distance = (Velocity * (double)Time.fixedDeltaTime * sc.TimeScale) + (0.5f * (TotalAcceleration * Math.Pow((double)Time.fixedDeltaTime * sc.TimeScale, 2)));
        distance = distance * SD * relativity;

        //Scale the result
        position += distance;

        //Update the velocity, which is to be used in the next frame and used as "initial velocity"
        SetVelocity();

        previousPosition = transform.position;
        transform.position = new Vector3((float)position.x, (float)position.y, (float)position.z);

        //Debug.Log("Frames: " + sc.Frames + " Object: " + gameObject.name + " Moved");
    }

    public void GravityArrow()
    {
        //Point arrow at average gravity
        if (math.lengthsq(TotalAcceleration) != 0f)
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
