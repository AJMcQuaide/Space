using UnityEngine;

public class Planet : CelestialBody
{
    private void Start()
    {

    }

    private void FixedUpdate()
    {
        if (Application.isPlaying)
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
