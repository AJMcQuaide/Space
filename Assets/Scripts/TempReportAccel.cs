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
        reportAcceleration = SpaceController.Instance.Cb[0].GetAcceleration((WhatToCheck.transform.position - transform.position).magnitude, WhatToCheck.Mass);
    }
}