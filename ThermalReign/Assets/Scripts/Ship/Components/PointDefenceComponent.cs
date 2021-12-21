using System;
using System.Collections;
using System.Collections.Generic;
using Thermal;
using UnityEditor;
using UnityEngine;

public class PointDefenceComponent : ShipComponent
{
    [SerializeField] private Transform turretTransform;

    private void OnValidate()
    {
        if (!(componentData is PointDefenceComponentData))
        {
            componentData = null;
        }
    }

    private void Start()
    {
        StartCoroutine(Idle(componentData as PointDefenceComponentData));
        StartCoroutine(Targeting(componentData as PointDefenceComponentData));
    }
    
    private IEnumerator Targeting(PointDefenceComponentData data)
    {
        // while (true)
        // {
        //     ThermalSignature potentialTarget;
        //     float smallestDistance = Single.PositiveInfinity;
        //     foreach (var signature in Mainframe.SignatureInfos)
        //     {
        //         float distance = Vector3.Distance(turretTransform.position, signature.Value.LastKnownLocation);
        //         if (signature.Value.Type == SignatureType.GuidedProjectile && distance < smallestDistance && distance <= data.range)
        //         {
        //             smallestDistance = distance;
        //             potentialTarget = signature.Key;
        //         }
        //     }
        //     yield break;
        // }
        yield break;
    }

    private IEnumerator Idle(PointDefenceComponentData data)
    {
        yield break;
    }

    private IEnumerator Track(PointDefenceComponentData data)
    {
        yield break;
    }
    
    private IEnumerator Return(PointDefenceComponentData data)
    {
        yield break;
    }

    private bool IsPositionWithinElevationRange(Vector3 position)
    {
        var targetDirection = position - turretTransform.position;
        targetDirection = turretTransform.InverseTransformDirection(targetDirection);
        var angleOnY = Mathf.Atan2( targetDirection.z, targetDirection.x ) * Mathf.Rad2Deg;
        
        
        return false;
    }
    
    // private void RotateBase()
    // {
    //     // TODO: Turret needs to rotate the long way around if the aimpoint gets behind
    //     // it and traversal limits prevent it from taking the shortest rotation.
    //     if (turretBase != null)
    //     {
    //         // Note, the local conversion has to come from the parent.
    //         Vector3 localTargetPos = transform.InverseTransformPoint(aimPoint);
    //         localTargetPos.y = 0.0f;
    //
    //         // Clamp target rotation by creating a limited rotation to the target.
    //         // Use different clamps depending if the target is to the left or right of the turret.
    //         Vector3 clampedLocalVec2Target = localTargetPos;
    //         
    //
    //         // Create new rotation towards the target in local space.
    //         Quaternion rotationGoal = Quaternion.LookRotation(clampedLocalVec2Target);
    //         Quaternion newRotation = Quaternion.RotateTowards(turretBase.localRotation, rotationGoal, turnRate * Time.deltaTime);
    //
    //         // Set the new rotation of the base.
    //         turretBase.localRotation = newRotation;
    //     }
    // }
    
    private void OnDrawGizmos()
    {
        PointDefenceComponentData data = componentData as PointDefenceComponentData;
        Gizmos.DrawRay(turretTransform.position, turretTransform.forward * data.range);
        Quaternion minAngle = Quaternion.AngleAxis(data.minElevation, turretTransform.forward);
        Quaternion maxAngle = Quaternion.AngleAxis(data.maxElevation, turretTransform.forward);
        
        Gizmos.DrawRay(turretTransform.position, minAngle * turretTransform.forward * data.range);
        Gizmos.DrawRay(turretTransform.position, maxAngle * turretTransform.forward * data.range);

        var from = Quaternion.AngleAxis(data.minElevation, turretTransform.right) * turretTransform.forward;
        
        // Handles.color = new Color(0, 1, 0, .2f);
        // Handles.DrawSolidArc(turretTransform.position, turretTransform.up, from, data.maxElevation * 2, data.range);
        //
    }
}
