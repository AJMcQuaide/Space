using UnityEngine;

public class Star : CelestialBody
{
    private void FixedUpdate()
    {
        Speed = Velocity.magnitude * S / Time.fixedDeltaTime;
        ApplyAllGravity();
        if (UseRelativeMass)
        {
            RelativeMass *= CalculateRelativeMass(Speed);
        }
        transform.position += Velocity;
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
