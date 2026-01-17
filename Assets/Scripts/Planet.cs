using UnityEngine;

public class Planet : CelestialBody
{
    [SerializeField]
    PlanetType pt;
    public PlanetType Pt { get { return pt; } }

    private void FixedUpdate()
    {
        UpdateSpeed();
        ApplyAllGravity();
        if (UseRelativeMass)
        {
            RelativeMass = Mass * CalculateRelativeMass(Speed);
        }
        transform.position += Velocity * Time.fixedDeltaTime;
    }

    private void OnDisable()
    {
        DeRegister();
    }

    private void OnEnable()
    {        
        SetProperties();
        Register(this);
    }
}
