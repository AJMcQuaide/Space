using UnityEngine;

public class Planet : CelestialBody
{
    [SerializeField]
    PlanetType planetTyp;
    public PlanetType PlanetTyp { get { return planetTyp; } }

    private void FixedUpdate()
    {
        if (SpaceController.Instance.FrameCounter < 150000)
        {
            UpdateSpeed();
            ApplyAllGravity();
            if (UseRelativeMass)
            {
                RelativeMass = Mass * CalculateRelativeMass(Speed);
            }
            transform.position += Velocity * Time.fixedDeltaTime;
        }
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
