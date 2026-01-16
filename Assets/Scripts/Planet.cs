using UnityEngine;

public class Planet : CelestialBody
{
    private void FixedUpdate()
    {
        if (Time.time < 1000f)
        {
            Speed = Velocity.magnitude * S;
            ApplyAllGravity();
            if (UseRelativeMass)
            {
                RelativeMass *= CalculateRelativeMass(Speed);
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
