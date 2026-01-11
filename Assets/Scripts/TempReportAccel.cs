using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempReportAccel : CelestialBody
{
    [SerializeField]
    float acceleration;

    [SerializeField]
    CelestialBody WhatToCheck;

    void Update()
    {
        acceleration = GetAcceleration((WhatToCheck.transform.position - transform.position).magnitude, WhatToCheck.Mass);
    }
}