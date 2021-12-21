using System.Collections;
using System.Collections.Generic;
using Ship;
using UnityEngine;
using UnityEngine.PlayerLoop;


[RequireComponent(typeof(ShipMainframe))]
public class ShipComponent : MonoBehaviour
{
    [SerializeField] protected ComponentData componentData;
    [SerializeField, Range(0f, 1f)] private float powerPercentage;
    
    protected ShipMainframe Mainframe;

    protected virtual void Awake()
    {
        Mainframe = GetComponent<ShipMainframe>();
    }
    
    public float ThermalOutput => PowerUsage * (1 - componentData.efficiency);

    public float PowerPercentage
    {
        get => powerPercentage;
        set => powerPercentage = Mathf.Max(0f, Mathf.Min(1f, value));
    }

    protected float CurrentMaxPower => powerPercentage * componentData.maxPowerInput;
    private float _usage;

    protected float PowerUsage
    {
        get => _usage;
        set => _usage = Mathf.Max(0f, Mathf.Min(CurrentMaxPower, value));
    }

    private ShipMainframe _shipMainframe;
}