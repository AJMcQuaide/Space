using UnityEngine;

public class Star : CelestialBody
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
