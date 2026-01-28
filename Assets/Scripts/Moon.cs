using UnityEditor.Profiling;
using UnityEngine;

public class Moon : CelestialBody
{
    private void FixedUpdate()
    {
        if (Application.isPlaying && SpaceController.Instance.Frames < 50)
        {
            UpdateSpeed();
            SetPosition();
            if (ShowGravityArrow)
            {
                GravityArrow(TotalAcceleration);
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
