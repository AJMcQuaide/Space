using UnityEngine;

public class Moon : CelestialBody
{
    private void FixedUpdate()
    {
        if (Application.isPlaying && sc.Frames < SpaceController.Instance.simulationLength && IsKinematic == false)
        {
            UpdateSpeed();
            SetPosition();
            RelativeMass = Mass * LorentzFactor;
            if (ShowGravityArrow)
            {
                GravityArrow();
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
