using System;
using System.Collections;
using System.Collections.Generic;
using Thermal;
using UnityEngine;

namespace Environment
{
    [RequireComponent(typeof(ThermalSignature))]
    public class EnvironmentObject : MonoBehaviour
    {
        [SerializeField] private EnvironmentObjectData data;
        private ThermalSignature _signature;

        private void OnValidate()
        {
            _signature = GetComponent<ThermalSignature>();
            if (data != null && _signature != null)
            {
                _signature.type = data.type;
                _signature.Allegiance = data.allegiance;
            }
        }

        private void Start()
        {
            _signature = GetComponent<ThermalSignature>();
            _signature.type = data.type;
            _signature.Allegiance = data.allegiance;
        }

        private void OnEnable()
        {
            _signature.ThermalInput = data.passiveEnergyProduction;
        }

        private void OnDisable()
        {
            _signature.ThermalInput = 0f;
        }
    }
}
