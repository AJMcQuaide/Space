using UnityEngine;

public class Star : CelestialBody
{
    private void FixedUpdate()
    {
        if (Application.isPlaying && SpaceController.Instance.Frames < SpaceController.Instance.simulationLength)
        {
            UpdateSpeed();
            SetPosition();
            if (ShowGravityArrow)
            {
                GravityArrow();
            }
            if (UseRelativeMass)
            {
                RelativeMass = Mass * GetRelativeMass(Speed);
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
