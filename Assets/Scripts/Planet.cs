using UnityEditor.Search;
using UnityEngine;

public class Planet : CelestialBody
{
    private void Start()
    {

    }

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
