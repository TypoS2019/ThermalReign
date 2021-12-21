using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ComponentData : ScriptableObject
{
    [SerializeField, Min(0f)] public float maxPowerInput = 100;
    [SerializeField, Range(0f, 1f)] public float efficiency = 0.95f;
}
