using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Thermal;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ship
{
    [RequireComponent(typeof(ThermalSignature), typeof(ShipHull))]
    public class ShipMainframe : MonoBehaviour
    {
        protected List<ShipComponent> Components;
        protected ShipHull Hull;
        
        public ThermalSignature target;
        public ThermalSignature Signature
        {
            get;
            private set;
        }
        
        public Dictionary<ThermalSignature, SignatureInfo> SignatureInfos;

        protected virtual void Awake()
        {
            Components = GetComponents<ShipComponent>().ToList();
            Signature = GetComponent<ThermalSignature>();
            SignatureInfos = new Dictionary<ThermalSignature, SignatureInfo>();
        }

        protected virtual void Start()
        {
            Signature.type = SignatureType.Ship;
        }

        protected virtual void Update()
        {
            UpdateSignature();
        }

        private void UpdateSignature()
        {
            float totalThermalOutput = 0;
            foreach (ShipComponent system in Components)
            {
                totalThermalOutput += system.ThermalOutput;
            }
            Signature.ThermalInput = totalThermalOutput;
        }

        private void SelectTarget()
        {
            float angle = Single.MaxValue;
            ThermalSignature newTarget = target;
            foreach (var signature in SignatureInfos)
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
            target = newTarget;
        }
    }
}
