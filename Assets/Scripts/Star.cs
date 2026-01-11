using UnityEngine;

public class Star : CelestialBody
{

    private void Start()
    {

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
