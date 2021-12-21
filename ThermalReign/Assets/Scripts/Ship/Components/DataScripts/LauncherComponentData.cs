using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class LauncherComponentData : ComponentData
{
    public float launchTime = 0.1f;
    public float launchCooldown = 1;
    public float launchDistance = 10f;
    public float launchForce = 10f;
    public float reloadCooldown = 20f;
    public int magazineSize = 10;
    public float reloadToPowerRatio = 5;
    public int maxOrders = 5;
    public GameObject projectile;

    private void OnValidate()
    {
        if (projectile.GetComponent<GuidedProjectile>() == null)
        {
            projectile = null;
        }
    }
}
