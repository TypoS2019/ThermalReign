using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ScannerComponentData : ComponentData
{
    [SerializeField] public float strength = 1;
    [SerializeField] public float thermalEnergyRequiredForWeak;
    [SerializeField] public float thermalEnergyRequiredForMedium;
    [SerializeField] public float thermalEnergyRequiredForStrong;
    [SerializeField] public float thermalEclipseStrength = 1;
    [SerializeField] public float thermalEclipseMaxAngle = 90;
    [SerializeField] public AnimationCurve falloff;
}
