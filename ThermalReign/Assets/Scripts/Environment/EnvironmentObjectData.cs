using System.Collections;
using System.Collections.Generic;
using Thermal;
using UnityEngine;

namespace Environment
{
    [CreateAssetMenu]
    public class EnvironmentObjectData : ScriptableObject
    {
        public SignatureType type;
        public Allegiance allegiance;
        public float passiveEnergyProduction;
    }
}

