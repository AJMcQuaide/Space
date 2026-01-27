using UnityEngine;

public class Planet : CelestialBody
{
    private void Start()
    {

    }

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
            Debug.Log("Velocity: " + Velocity.x + " Position:" + transform.position.x);
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
