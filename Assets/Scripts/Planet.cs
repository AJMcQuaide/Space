using UnityEngine;

public class Planet : CelestialBody
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
