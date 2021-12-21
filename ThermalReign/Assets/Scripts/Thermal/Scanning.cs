using System.Collections;
using System.Collections.Generic;
using Thermal;
using UnityEngine;

public class Scanning
{
    public static float CalcVisibleThermalEnergy(ThermalSignature target, Vector3 origin, float strength)
    {
        float distance = Vector3.Distance(target.transform.position, origin);
        return Mathf.Max(target.ThermalOutput - distance / strength, 0);
    }
}
