using System;
using System.Collections;
using System.Collections.Generic;
using Ship;
using Thermal;
using Unity.VisualScripting;
using UnityEngine;

public class LauncherComponent : ShipComponent
{
    [SerializeField] private Transform origin;

    [SerializeField] private int _magazine = 0;
    
    private Queue<ThermalSignature> _launchOrders;
    private bool _launch = true;
    public String state;
    private void OnValidate()
    {
        
        if (!(componentData is LauncherComponentData))
        {
            componentData = null;
        }
    }

    private void Start()
    {
        _launchOrders = new Queue<ThermalSignature>();
        LauncherComponentData data = componentData as LauncherComponentData;
        StartCoroutine(Ready(data));
    }

    public void AddLaunchOrder(ThermalSignature target)
    {
        LauncherComponentData data = componentData as LauncherComponentData;
        if (_launchOrders.Count < data.maxOrders)
        {
            _launchOrders.Enqueue(target);
        }
    }

    private IEnumerator Ready(LauncherComponentData data)
    {
        state = "ready";
        
        PowerUsage = 0;
        while (true)
        {
            if (PowerPercentage <= 0)
            {
                StartCoroutine(Disabled(data));
                yield break;
            }
            if (_magazine <= 0)
            {
                StartCoroutine(Reload(data));
                yield break;
            }
            if (_launchOrders.Count > 0 && _launch)
            {
                _magazine--;
                StartCoroutine(Deploy(data, _launchOrders.Dequeue()));
                yield break;
            }
            yield return null;
        }
    }

    private IEnumerator Deploy(LauncherComponentData data, ThermalSignature target)
    {
        state = "deploy";

        if (PowerPercentage <= 0)
        {
            StartCoroutine(Disabled(data));
            yield break;
        }
        PowerUsage = CurrentMaxPower;
        yield return new WaitForSeconds(data.launchTime);
        PowerUsage = 0;
        GuidedProjectile guidedProjectile = DeployProjectile(data);
        StartCoroutine(LaunchWhenEnoughDistance(guidedProjectile, target, data!.launchDistance));
        yield return new WaitForSeconds(data.launchCooldown / PowerPercentage);
        StartCoroutine(Ready(data));
    }
    
    private IEnumerator Reload(LauncherComponentData data)
    {
        state = "reload";
        if (PowerPercentage <= 0)
        {
            StartCoroutine(Disabled(data));
            yield break;
        }
        float percentage = 1;
        if (data.reloadToPowerRatio > CurrentMaxPower)
        {
            percentage = CurrentMaxPower / data.reloadToPowerRatio;
        }
        PowerUsage = data.reloadToPowerRatio * percentage;
        yield return new WaitForSeconds(data.reloadCooldown / percentage);
        _magazine = data.magazineSize;
        StartCoroutine(Ready(data));
    }

    private IEnumerator Disabled(LauncherComponentData data)
    {
        state = "disabled";
        yield return new WaitUntil((() => PowerPercentage > 0));
        StartCoroutine(Ready(data));
    }
    
    private IEnumerator LaunchWhenEnoughDistance(GuidedProjectile guidedProjectile, ThermalSignature target, float launchDistance)
    {
        yield return new WaitUntil((() =>
            Vector3.Distance(guidedProjectile.transform.position, origin.position) >= launchDistance));
        guidedProjectile.Launch(target);
    } 
    
    private void OnFire()
    {
        AddLaunchOrder(Mainframe.target);
    }

    private GuidedProjectile DeployProjectile(LauncherComponentData data)
    {
        GuidedProjectile guidedProjectile = Instantiate(data.projectile, origin.position, origin.rotation).GetComponent<GuidedProjectile>();
        Rigidbody rb = guidedProjectile.GetComponent<Rigidbody>();
        rb.velocity = GetComponent<Rigidbody>()!.velocity;
        rb.AddForce(origin.forward * data.launchForce, ForceMode.Impulse);
        guidedProjectile.GetComponent<ThermalSignature>().Allegiance = Mainframe.Signature.Allegiance;
        return guidedProjectile;
    }
}
