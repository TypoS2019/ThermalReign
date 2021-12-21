using System;
using System.Collections;
using System.Collections.Generic;
using Thermal;
using UnityEngine;

public class TargetComputerComponent : ShipComponent
{
    private void OnValidate()
    {
        if (!(componentData is TargetComputerComponentData))
        {
            componentData = null;
        }
    }
    
    public void SelectTarget()
    {
        float angle = Single.MaxValue;
        ThermalSignature newTarget = Mainframe.target;
        foreach (var signature in Mainframe.SignatureInfos)
        {
            if (signature.Value.Strength >= SignatureStrength.Weak && signature.Value.Allegiance != Allegiance.Player)
            {
                float newAngle = Vector3.Angle(signature.Value.LastKnownLocation - transform.position, transform.forward);
                if (newAngle < angle)
                {
                    angle = newAngle;
                    newTarget = signature.Key;
                }
            }
        }
        Mainframe.target = newTarget;
    }
    
    private void OnTarget()
    {
        SelectTarget();
    }
}
