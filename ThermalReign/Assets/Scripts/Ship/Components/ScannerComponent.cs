using System;
using System.Collections;
using System.Collections.Generic;
using Ship;
using Thermal;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class ScannerComponent : ShipComponent
{
    private void OnValidate()
    {
        if (!(componentData is ScannerComponentData))
        {
            componentData = null;
        }
    }

    private void Update()
    {
        UpdateSignatures();
    }

    private void UpdateSignatures()
    {
        ScannerComponentData data = componentData as ScannerComponentData;
        PowerUsage = CurrentMaxPower;
        RemoveOldSignatures();
        CalcVisibleThermalEnergyForSignatures(data);
        // EclipseVisibleThermalEnergyForSignatures(data);
        UpdateInfoForSignatures(data);
    }
    
    private void CalcVisibleThermalEnergyForSignatures(ScannerComponentData data)
    {
        foreach (ThermalSignature signature in ThermalSignature.Signatures)
        {
            if (signature != GetComponent<ThermalSignature>())
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, signature.transform.position - transform.position, out hit))
                {
                    if (!Mainframe.SignatureInfos.ContainsKey(signature))
                    {
                        Mainframe.SignatureInfos.Add(signature, new SignatureInfo());
                    }
                    SignatureInfo info = Mainframe.SignatureInfos[signature];
                    if (hit.transform == signature.transform)
                    {
                        info!.Thermal =
                            Scanning.CalcVisibleThermalEnergy(signature, transform.position,
                                data!.strength * PowerPercentage);
                    }
                    else
                    {
                        info!.Thermal = 0;
                    }
                }
            }
        }
    }

    private void RemoveOldSignatures()
    {
        foreach (var signature in Mainframe.SignatureInfos)
        {
            if (!ThermalSignature.Signatures.Contains(signature.Key))
            {
                Mainframe.SignatureInfos.Remove(signature.Key);
            }
        }
    }

    private void UpdateInfoForSignatures(ScannerComponentData data)
    {
        foreach (var signature in Mainframe.SignatureInfos)
        {
            UpdateInfoForSignature(signature.Key, data);
        }
    }

    private void UpdateInfoForSignature(ThermalSignature signature, ScannerComponentData data)
    {
        SignatureInfo info = Mainframe.SignatureInfos[signature];
        info!.Strength = GetSignatureStrength(info.Thermal, data);
        if (info!.Strength > SignatureStrength.Lost)
        {
            if (info!.Strength >= SignatureStrength.Weak)
            {
                info!.LastTimeSeen = Time.time;
                info!.LastKnownLocation = signature.transform.position;
            }
            if (info!.Strength >= SignatureStrength.Medium)
            {
                info!.Type = signature.type;
                info!.Allegiance = signature.Allegiance;
                info!.SignatureName = signature.name;
                info!.Mass = signature.GetComponent<Rigidbody>()!.mass;
                info!.Velocity = signature.GetComponent<Rigidbody>()!.velocity;
            }
            if (info!.Strength >= SignatureStrength.Strong)
            {
            }
        }
    }

    private void EclipseVisibleThermalEnergyForSignatures(ScannerComponentData data)
    {
        ApplyEclipseToSignatures(CalcThermalEclipseValues(data));
    }

    private void ApplyEclipseToSignatures(Dictionary<ThermalSignature, float> thermalEclipseValues)
    {
        foreach (var eclipseValue in thermalEclipseValues)
        {
            Mainframe.SignatureInfos[eclipseValue.Key].Thermal =
                Mathf.Max(Mainframe.SignatureInfos[eclipseValue.Key].Thermal - eclipseValue.Value, 0);
        }
    }

    private Dictionary<ThermalSignature, float> CalcThermalEclipseValues(ScannerComponentData data)
    {
        Dictionary<ThermalSignature, float> thermalEclipseValues = new Dictionary<ThermalSignature, float>();
        foreach (var signature in Mainframe.SignatureInfos)
        {
            thermalEclipseValues.Add(signature.Key, CalcThermalEclipseValue(signature.Key, data));
        }

        return thermalEclipseValues;
    }

    private float CalcThermalEclipseValue(ThermalSignature signature, ScannerComponentData data)
    {
        SignatureInfo info = Mainframe.SignatureInfos[signature];
        float thermalEclipseValue = 0;
        foreach (var otherSignature in Mainframe.SignatureInfos)
        {
            if (signature != otherSignature.Key && data.thermalEclipseMaxAngle > 0 &&
                otherSignature.Value.Thermal > info.Thermal)
            {
                float angle = Vector3.Angle(info.LastKnownLocation - transform.position,
                    otherSignature.Value.LastKnownLocation - transform.position);
                float anglePercentage = angle / data.thermalEclipseMaxAngle;
                thermalEclipseValue +=
                    Mathf.Max(
                        data.falloff.Evaluate(anglePercentage) *
                        Mathf.Max(otherSignature.Value.Thermal - info.Thermal, 0), 0);
            }
        }

        return thermalEclipseValue;
    }

    private SignatureStrength GetSignatureStrength(float thermalEnergy, ScannerComponentData data)
    {
        if (thermalEnergy > data!.thermalEnergyRequiredForStrong) return SignatureStrength.Strong;
        if (thermalEnergy > data!.thermalEnergyRequiredForMedium) return SignatureStrength.Medium;
        if (thermalEnergy > data!.thermalEnergyRequiredForWeak) return SignatureStrength.Weak;
        return SignatureStrength.Lost;
    }

    void OnDrawGizmosSelected()
    {
        if (Mainframe != null && Mainframe.SignatureInfos != null)
        {
            foreach (var signature in Mainframe.SignatureInfos)
            {
                Handles.Label(signature.Value.LastKnownLocation,
                    "Strength: " + signature.Value.Strength + " Thermal: " + signature.Value.Thermal + " Mass: " +
                    signature.Value.Mass + " Name: " + signature.Key.name);
                Gizmos.color = Color.red;
                Gizmos.DrawRay(signature.Value.LastKnownLocation, signature.Value.Velocity);
                Gizmos.DrawRay(transform.position, signature.Value.LastKnownLocation - transform.position);
            }
        }
    }
}