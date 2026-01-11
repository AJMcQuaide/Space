using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    public const float G = 0.0000000000667f; //Gravitational constant
    public const float c = 299792458; //Speed of light

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

    public int ScaleFactor
    {
        get { return SpaceController.Instance.ScaleFactor; }
    }
    public int TimeMultiplier
    {
        get { return SpaceController.Instance.TimeMultiplier; }
    }

    [Header("m/s")]
    [SerializeField]
    float startSpeed;
    public float StartSpeed { get { return startSpeed; } set { startSpeed = value; } }

    [Header("Pct of Speed of light")]
    [SerializeField]
    public float Speed;

    [Header("Properties")]
    [SerializeField]
    Color color;
    public Color Color
    {
        get { return color; } 
        set { color = value; }
    }

    [SerializeField]
    Color lineColor;

    [SerializeField]
    bool isKinematic;

    public Vector3 Velocity { get; set; }

    //Set scale and color among other things
    public void SetProperties()
    {
        //Scale
        transform.localScale = new Vector3(Diameter / ScaleFactor, Diameter / ScaleFactor, Diameter / ScaleFactor);
        //Set Color
        GetComponentInChildren<MeshRenderer>().sharedMaterial.color = Color;
        GetComponent<TrailRenderer>().material.color = lineColor;
    }

    //Set the acceleration due to gravity in m/s^2. Units are m, kg. G is gravitational constent.
    public float GetAcceleration(float differenceUnity, float mass)
    {
        //The actual distance in Unity usually incorrect due to scaling, multiplied by the scale factor to make it true
        float r = differenceUnity * ScaleFactor;
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
            //Cap the acceleration due to gravity at the surface of the celestial body
            float maxAcceleration = GetAcceleration((cb.Diameter * 0.5f) / ScaleFactor, cb.mass);
            //Get the current un-clamped acceleration assuming the mass is at a single point
            float currentAcceleration = GetAcceleration(difference.magnitude, cb.mass);
            //Calculate and clamp the acceleration due to gravity
            float acceleration = Mathf.Clamp(currentAcceleration, 0f, maxAcceleration);
            //Calculate vector offset per frame
            Vector3 deltaPos = acceleration * TimeMultiplier * Time.fixedDeltaTime * difference.normalized;
            //Calculate velocity and take into account scale factor
            Velocity += (deltaPos / ScaleFactor);
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
