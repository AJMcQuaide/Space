using UnityEngine;

public class Planet : CelestialBody
{
    private void Awake()
    {
        //sc = SpaceController.Instance;
    }

    private void FixedUpdate()
    {
        Debug.LogWarning("Using non CB");
        //if (sc.Play && Application.isPlaying && sc.Frames < sc.simulationLength && IsKinematic == false)
        //{
        //    UpdateSpeed();
        //    SetPosition();
        //    RelativeMass = Mass * LorentzFactor;
        //    if (PhysicsArrows)
        //    {
        //        PositionArrow(TotalAcceleration, Velocity);
        //    }
        //}
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
