using UnityEngine;

public class Moon : CelestialBody
{
    private void Awake()
    {
        gravityArrowSize = 1f;
    }
    private void FixedUpdate()
    {
        if (Application.isPlaying)
        {
            UpdateSpeed();
            TotalGravity();
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
