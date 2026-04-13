using System;
using UnityEngine;
using Unity.Mathematics;

[RequireComponent(typeof(SphereCollider))]
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
    public const double SD = 1 / S;

    [SerializeField]
    CelestialBodyType thisCelestialBody;
    public CelestialBodyType ThisCelestialBody
    {
        get { return thisCelestialBody; }
        set
        {
            if (thisCelestialBody != value)
            {
                thisCelestialBody = value;
                Debug.LogWarning("Celestial body changed");
                UpdateCelestialBodyParameters(value);
            }
        }
    }

    SpaceController sc;
    public SpaceController Sc
    {
        get
        {
            if (sc == null)
            {
                sc = SpaceController.Instance;
            }
            return sc;
        }
    }

    [Header("Mass in kg")]
    [SerializeField]
    double mass;
    public double Mass { get { return mass; } set { mass = value; } }

    [Header("Radius in meters")]
    [SerializeField]
    float radius;
    public float Radius { get { return radius; } set { radius = value; } }

    public double RelativeMass { get; set; }

    [Header("Speed in m/s, direction is transform.forward")]
    [SerializeField]
    double initialSpeed;
    public double InitialSpeed { get { return initialSpeed; } set { initialSpeed = value; } }

    /// <summary>
    /// Double 3 verison of transform.position to pair wiht and aid in accurate calculations
    /// </summary>
    double3 position;
    public double3 Position { get { return position; } set { position = value; } }

    [Header("Properties")]

    [SerializeField]
    Color trailColor;
    public Color TrailColor { get { return new Color(trailColor.r, trailColor.g, trailColor.b, 1f); } set { trailColor = value; } }

    [SerializeField]
    float trailWidth;
    public float TrailWidth { get { return trailWidth; } set { trailWidth = value; } }

    [SerializeField]
    bool isKinematic;
    public bool IsKinematic { get { return isKinematic; } set { isKinematic = value; } }

    [SerializeField]
    bool warpGrid;
    public bool WarpGrid { get { return warpGrid; } set { warpGrid = value; } }

    [SerializeField]
    bool physicsArrow = true;
    public bool PhysicsArrows { get { return physicsArrow; } }

    [Header("Reference")]
    [SerializeField]
    double lorentzFactor = 1d;
    public double LorentzFactor { get { return lorentzFactor; } }

    [SerializeField]
    float speed;
    public float Speed { get { return speed; } set { speed = value; } }

    /// <summary>
    /// The sum of all accelerations on the object
    /// </summary>
    [SerializeField]
    double3 totalAcceleration;
    public double3 TotalAcceleration { get { return totalAcceleration; } set { totalAcceleration = value; } }

    /// <summary>
    /// Real world velocity. Not scaled for Unity.
    /// </summary>
    double3 velocity;
    public double3 Velocity { get { return velocity; } }

    /// <summary>
    /// The max acceleration is set to the radius of the planet
    /// </summary>
    double maxAcceleration;
    public double MaxAcceleration { get { return maxAcceleration; } set { maxAcceleration = value; } }

    /// <summary>
    /// Schwarzschild radius in m, real world units, not scaled (S) to Unity
    /// </summary>
    [SerializeField]
    double sR;

    /// <summary>
    /// Density in kg/m^3
    /// </summary>
    [SerializeField]
    float density;

    Vector3 previousPosition;
    public Vector3 PreviousPosition { get { return previousPosition; } }

    public bool ContactChecked { get; set; } = false;

    [Header("Unity References")]
    [SerializeField]
    GameObject model;

    private void Start()
    {
        SetProperties();
    }

    private void FixedUpdate()
    {
        if (Sc.Play && Application.isPlaying && Sc.Frames < Sc.simulationLength && IsKinematic == false)
        {
            UpdateSpeed();
            SetPosition();
            UpdateRotation();
            RelativeMass = Mass * LorentzFactor;
        }
    }

    //Set scale and color among other things
    public void SetProperties()
    {
        //Set trail renderer color
        TrailRenderer tr = GetComponent<TrailRenderer>();
        MaterialPropertyBlock trailProperty = new();
        trailProperty.SetColor("_Color", trailColor);
        tr.SetPropertyBlock(trailProperty);
        trailWidth = trailWidth == 0 ? 0.02f : trailWidth;
        tr.widthMultiplier = trailWidth;
        tr.time = Sc.UniversalTrailLength;

        //Set Max acceleration based on mass and radius
        maxAcceleration = GetAcceleration(Radius / S, Mass);

        //Set relative mass equal to mass to start
        RelativeMass = Mass;

        //Starting velocity
        double3 transformForward = new(transform.forward.x, transform.forward.y, transform.forward.z);

        //Clamp initialvelocity and set to Velocity
        velocity = math.clamp(initialSpeed, 0d, c * 0.99999d) * transformForward;

        //Set position double to the transform at start
        Position = sc.Vector3ToDouble3(transform.position);

        //Set previous position equal to starting position
        previousPosition = transform.position;

        //Set sR
        SchwarzschildRadius();

        //Set density
        UpdateDensity();

        //Set Layer
        gameObject.layer = LayerMask.NameToLayer("CelestialBody");
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
        foreach (CelestialBody cb2 in Sc.CelestialBodiesInScene)
        {
            if (cb2 != cb1)
            {
                //Convert to double
                double3 p2 = new(cb2.transform.position.x, cb2.transform.position.y, cb2.transform.position.z);
                double3 p1 = new(cb1.position.x, cb1.position.y, cb1.position.z);
                double3 difference = p2 - p1;

                //Contact and calculation reflection
                if (math.length(difference) < (cb2.Radius * SD + Radius * SD))
                {
                    //Calculate the velocity of the cb being evaluated
                    double3 iv1 = cb1.velocity;
                    double3 iv2 = cb2.velocity;
                    double3 v1 = Reflect(difference, iv1, iv2, cb1.Mass, cb2.Mass);
                    cb1.velocity = v1;
                    //Calculate the velocity of the other cb that was contacted
                    double3 v2 = Reflect(difference, iv2, iv1, cb2.Mass, cb1.Mass);
                    cb2.velocity = v2;
                    cb2.ContactChecked = true;
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
        if (Sc.UseGravity == false)
        {
            return accelerationVector;
        }
        int celestialBodiesEvaluated = 0;

        //Loop this Cb through all other Cb's
        foreach (CelestialBody _otherCB in Sc.CelestialBodiesInScene)
        {
            if (_otherCB != _thisCB && _thisCB.isKinematic == false)
            {
               //Convert to double
                double3 otherCB = new(_otherCB.PreviousPosition.x, _otherCB.PreviousPosition.y, _otherCB.PreviousPosition.z);
                double3 thisCB = new(_thisCB.transform.position.x, _thisCB.transform.position.y, _thisCB.transform.position.z);
                double3 difference = otherCB - thisCB;

                //Acceleration
                celestialBodiesEvaluated++;
                double magnitude = math.length(difference);
                if (magnitude == 0)
                {
                    Debug.LogWarning("When calculating acceleration, the distance between objects was zero and therefore returned zero acceleration");
                    return new double3(0d, 0d, 0d);
                }
                double acceleration = GetAcceleration(magnitude, _otherCB.RelativeMass);
                acceleration = Math.Clamp(acceleration, 0d, _otherCB.MaxAcceleration);
                accelerationVector += acceleration * math.normalize(difference);

                if (math.isnan(accelerationVector).x)
                {
                    Debug.Log("Nan " + difference);
                }
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

        return reflection;
    }

    /// <summary>
    /// Calculate the velocity based on acceleration, in real world units at the end of this frame
    /// </summary>
    /// <param name="totalAcceleration"></param>
    public void SetVelocity()
    {
        velocity += TotalAcceleration * (double)Time.fixedDeltaTime * Sc.TimeScale;
    }

    /// <summary>
    /// Apply gravity. Get the distance traveled based on the velocity and total acceleration. Formula: distance = initial velocity * time + 1/2 * acceleration * time^2
    /// </summary>
    public void SetPosition()
    {
        //Percentage of energy contributing to acceleration
        SetLorentzFactor();

        position += GetPosition();

        //Update the velocity, which is to be used in the next frame and used as "initial velocity"
        SetVelocity();

        previousPosition = transform.position;
        transform.position = new Vector3((float)position.x, (float)position.y, (float)position.z);
    }

    /// <summary>
    /// Positional offset of object due to gravity using formula
    /// </summary>
    double3 GetPosition()
    {
        //Raw offset
        double3 unScaled = (velocity * (double)Time.fixedDeltaTime * Sc.TimeScale) + (0.5f * (TotalAcceleration * Math.Pow((double)Time.fixedDeltaTime * Sc.TimeScale, 2)));
        //Consider reletivity, less and less offset nearing the speed of light as more of the energy goes to mass increase
        return unScaled * SD * (1d / math.pow(LorentzFactor, 3));
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
        float velocityMagnitude = (float)math.length(velocity);
        Speed = velocityMagnitude;
    }

    /// <summary>
    /// Point forward in the direction of velocity
    /// </summary>
    public void UpdateRotation()
    {
        if (math.lengthsq(Velocity) > 0)
        {
            Vector3 velVector = new((float)Velocity.x, (float)Velocity.y, (float)Velocity.z);
            transform.rotation = Quaternion.LookRotation(velVector.normalized, Vector3.up);
        }
    }

    /// <summary>
    /// Calculate the density of the celestrial body
    /// </summary>
    public void UpdateDensity()
    {
        density = (float)Mass / (1.333333f * math.PI * (Radius * Radius * Radius));
    }

    /// <summary>
    /// Set the scale / radius, mass and colliders of a celestial body to a given prefab example
    /// </summary>
    /// <param name="type"></param>
    public void UpdateCelestialBodyParameters(CelestialBodyType type)
    {
        CelestialBody cb = Sc.GetCelestialBodyPrefab((int)type);

        //Set scale/radius
        float scale = (cb.Radius * 2) / (float)S;
        model.transform.localScale = new Vector3(scale, scale, scale);

        //Set collider
        SphereCollider collider = GetComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = scale * 0.5f;

        //Set mass
        mass = cb.mass;

        Debug.LogWarning("Current scale: " + model.transform.parent.gameObject.name + " " + model.transform.localScale + " New scale: " + cb.model.transform.parent.gameObject.name + " " + cb.model.transform.localScale);
    }

    /// <summary>
    /// Calculate the point at which the celestial body becomes a black hole
    /// </summary>
    public void SchwarzschildRadius()
    {
        sR = (2f * G * Mass) / (c * c);
        if (Radius <= sR)
        {
            Debug.LogWarning("Black Hole created for: " + gameObject.name);
        }
    }

    //Add the object to the Celestial body list
    public void Register(CelestialBody celestialBody)
    {
        SpaceController Instance = SpaceController.Instance;
        if (Instance != null)
        {
            SpaceController.Instance.CelestialBodiesInScene.Add(celestialBody);
        }
    }

    //Add the object to the Celestial body list
    public void DeRegister()
    {
        SpaceController Instance = SpaceController.Instance;
        if (Instance != null)
        {
            SpaceController.Instance.CelestialBodiesInScene.Remove(this);
        }
    }

    private void OnDisable()
    {
        DeRegister();
    }

    private void OnEnable()
    {
        Register(this);
    }
}
