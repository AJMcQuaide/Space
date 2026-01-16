using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempReportAccel : CelestialBody
{
    [SerializeField]
    float reportAcceleration;

    [SerializeField]
    CelestialBody WhatToCheck;

    void Update()
    {
        reportAcceleration = GetAcceleration((WhatToCheck.transform.position - transform.position).magnitude, WhatToCheck.Mass);
    }
}