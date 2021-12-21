using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class PointDefenceComponentData : ComponentData
{
    public float range;
    [Range(0, 90)]public float maxElevation;
    [Range(-70, 90)]public float minElevation;

    private void OnValidate()
    {
        if (minElevation >= maxElevation)
        {
            maxElevation = minElevation + 1f;
        }

    }
}
