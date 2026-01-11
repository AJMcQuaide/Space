using UnityEngine;

public class Moon : CelestialBody
{
    [SerializeField]
    CelestialBody Target;

    private void Awake()
    {
        Velocity = StartSpeed * Time.fixedDeltaTime * transform.forward;
    }

    private void FixedUpdate()
    {
        ApplyGravity(Target);
        transform.position += Velocity;

        Speed = Velocity.magnitude / c;
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
