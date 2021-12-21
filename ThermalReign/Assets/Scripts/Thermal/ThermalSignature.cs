using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEditor;

namespace Thermal
{
    public class ThermalSignature : MonoBehaviour
    {
        public static List<ThermalSignature> Signatures = new List<ThermalSignature>();
        
        [SerializeField] private float thermalDecreaseRate = 1;
        [SerializeField] public SignatureType type = SignatureType.Unknown;
        [SerializeField] public Allegiance Allegiance = Allegiance.Unknown;
        
        private float _thermalInput;
        public float ThermalInput
        {
            get => _thermalInput;
            set => _thermalInput = Mathf.Max(value, 0f);
        }
    
        public float ThermalOutput {get; private set; }

        private void OnEnable()
        {
            Signatures.Add(this);
        }

        private void OnDisable()
        {
            Signatures.Remove(this);
        }

        private void Update()
        {
            ThermalOutput += ThermalInput * Time.deltaTime;
            ThermalOutput -= Mathf.Max(CalculateEmissionDecrease(ThermalOutput),0);
            ThermalOutput = Mathf.Max(ThermalOutput, 0);
        }
        
        private float CalculateEmissionDecrease(float currentValue)
        {
            if (currentValue > 0)
            {
                return Mathf.Max(Mathf.Sqrt(thermalDecreaseRate * currentValue) * Time.deltaTime, 0);
            }
            return 0;
        }

        private void OnDrawGizmosSelected()
        {
            Handles.Label(transform.position, "Thermal: " + ThermalOutput);
        }
    }
    
    public class SignatureInfo
    {
        public Vector3 LastKnownLocation;
        public float LastTimeSeen;
        public SignatureStrength Strength;
        public string SignatureName;
        public float Mass;
        public Vector3 Velocity;
        public float Thermal;
        public SignatureType Type = SignatureType.Unknown;
        public Allegiance Allegiance = Allegiance.Unknown;
    }
    
    public enum SignatureStrength 
    {
        Lost = 0,
        Weak = 1,
        Medium = 2,
        Strong = 3
    }

    public enum SignatureType
    {
        Ship,
        GuidedProjectile,
        Projectile,
        CelestialObject,
        Debris,
        Unknown
    }

    public enum Allegiance
    {
        Player,
        Enemy,
        Neutral,
        Unknown
    }
}