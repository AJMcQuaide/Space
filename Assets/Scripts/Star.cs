using UnityEngine;

public class Star : CelestialBody
{
    private void FixedUpdate()
    {
        if (Application.isPlaying && SpaceController.Instance.Frames < 50)
        {
            UpdateSpeed();
            SetPosition(TotalAcceleration);
            if (UseRelativeMass)
            {
                RelativeMass = Mass * CalculateRelativeMass(Speed);
            }
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
