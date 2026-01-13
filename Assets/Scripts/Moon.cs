using UnityEngine;

public class Moon : CelestialBody
{
    [SerializeField]
    CelestialBody Target;

    private void Awake()
    {

    }

    private void FixedUpdate()
    {
        if (Time.time < 60)
        {
        //Display speed and convert from m to km
        speed = Velocity.magnitude / Time.fixedDeltaTime;

        ApplyAllGravity();
        transform.position += Velocity;

        Debug.Log("Time: " + Time.time);
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
