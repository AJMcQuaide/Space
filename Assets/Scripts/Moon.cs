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
        ApplyAllGravity();

        //Display speed
        speed = Velocity.magnitude * ScaleFactor / 1000f;
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
