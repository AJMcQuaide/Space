using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempReportAccel : MonoBehaviour
{
    [SerializeField]
    float reportAcceleration;

    [SerializeField]
    CelestialBody WhatToCheck;

    void Update()
    {
        if (WhatToCheck != null)
        {
            reportAcceleration = (float)CelestialBody.GetAcceleration((WhatToCheck.transform.position - transform.position).magnitude, WhatToCheck.Mass);
        }
    }
}