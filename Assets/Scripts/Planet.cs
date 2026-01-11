using UnityEngine;

public class Planet : CelestialBody
{

    private void Start()
    {

    }

    private void FixedUpdate()
    {
        ApplyAllGravity();
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
