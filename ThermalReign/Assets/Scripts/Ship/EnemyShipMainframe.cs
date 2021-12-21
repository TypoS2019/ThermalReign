using System;
using System.Collections;
using System.Collections.Generic;
using Ship;
using Thermal;
using UnityEngine;
[RequireComponent(typeof(ThermalSignature), typeof(ShipHull))]
public class EnemyShipMainframe : ShipMainframe
{
    [SerializeField] private List<Transform> waypoints;

    private LauncherComponent _launcherComponent;
    private ReactionControlComponent _reactionControlComponent;
    private ScannerComponent _scannerComponent;
    private Rigidbody _rigidbody;
    
    private float _targetMaxThermalOutput = 30;
    // private float _decisionCooldown = 1f;
    [SerializeField] private float destinationPrecision = 100;
    [SerializeField] private float aimPrecision = 0.001f;
    [SerializeField] private float targetVelocity = 20f;
    [SerializeField] private float rotateSpeed = 1f;
    [SerializeField] private float scanTime = 2f;
    private Vector3 _destination;
    private Queue<Vector3> _waypointQueue;
    
    protected override void Awake()
    {
        base.Awake();
        _launcherComponent = GetComponent<LauncherComponent>();
        _reactionControlComponent = GetComponent<ReactionControlComponent>();
        _scannerComponent = GetComponent<ScannerComponent>();
        _rigidbody = GetComponent<Rigidbody>();
        _waypointQueue = new Queue<Vector3>();
        waypoints.ForEach(t => _waypointQueue.Enqueue(t.position));
    }

    protected override void Start()
    {
        base.Start();
        StartCoroutine(Patrol());
        StartCoroutine(Stationary());
    }

    protected override void Update()
    {
        base.Update();
    }

    private IEnumerator Stationary()
    {
        _reactionControlComponent.directionalDampening = true;
        _reactionControlComponent.angularDampening = true;
        while (true)
        {
            if (Vector3.Distance(_destination, transform.position) >= destinationPrecision)
            {
                StartCoroutine(Maneuver());
                yield break;
            }
            yield return null;
        }
    }
    
    private IEnumerator Maneuver()
    {
        _reactionControlComponent.directionalDampening = true;
        _reactionControlComponent.angularDampening = false;
        while (true)
        {
            // Vector3 target = Vector3.zero;
            // Vector3 adjustment = Vector3.zero;
            // Vector3 localAngularV = transform.InverseTransformDirection(_rigidbody.velocity);
            // if (localAngularV.x < target.x)
            // {
            //     adjustment.x = 1;
            // }
            // if (localAngularV.y < target.y)
            // {
            //     adjustment.y = 1;
            // }
            // _reactionControlComponent.Rotate(adjustment);

            RotateToWardsVector(_destination);
            _reactionControlComponent.Strafe(Vector3.zero);

            if (Vector3.Distance(_destination, transform.position) <= destinationPrecision)
            {
                StartCoroutine(Stationary());
                yield break;
            }
            if (IsLookingTowardsVector(_destination))
            {
                StartCoroutine(Cruise());
                yield break;
            }
            yield return null;
        }
    }

    private IEnumerator Cruise()
    {
        _reactionControlComponent.directionalDampening = false;
        _reactionControlComponent.angularDampening = true;
        while (true)
        {
            _reactionControlComponent.Strafe(Vector3.zero);
            Vector3 localV = transform.InverseTransformDirection(_rigidbody.velocity);
            if (localV.z < targetVelocity)
            {
                _reactionControlComponent.directionalDampening = true;
                _reactionControlComponent.Strafe(new Vector3(0,0, 1f));
            }
            else
            {
                _reactionControlComponent.directionalDampening = false;
            }
            if (Vector3.Distance(_destination, transform.position) <= destinationPrecision)
            {
                StartCoroutine(Stationary());
                yield break;
            }
            if (!IsLookingTowardsVector(_destination))
            {
                StartCoroutine(Maneuver());
                yield break;
            }
            yield return null;
        }
    }
    
    private IEnumerator Patrol()
    {
        _destination = _waypointQueue.Dequeue();
        _waypointQueue.Enqueue(_destination);
        while (true)
        {
            foreach (var signature in SignatureInfos)
            {
                if (signature.Value.Strength > SignatureStrength.Lost && signature.Value.Allegiance == Allegiance.Player && signature.Value.Type == SignatureType.Ship)
                {
                    StartCoroutine(Attack(signature.Key, signature.Value));
                    yield break;
                }
            }
            if (Vector3.Distance(_destination, transform.position) <= destinationPrecision)
            {
                _destination = _waypointQueue.Dequeue();
                _waypointQueue.Enqueue(_destination);
            }
            yield return null;
        }
    }

    private IEnumerator Search(ThermalSignature target, SignatureInfo targetInfo)
    {
        _destination = targetInfo.LastKnownLocation;
        while (true)
        {
            if (targetInfo.Strength > SignatureStrength.Lost)
            {
                StartCoroutine(Attack(target, targetInfo));
                yield break;
            }
            if (Vector3.Distance(targetInfo.LastKnownLocation, transform.position) <= destinationPrecision)
            {
                StartCoroutine(Scan(target, targetInfo));
                yield break;
            }
            yield return null;
        }
    }

    private IEnumerator Scan(ThermalSignature target, SignatureInfo targetInfo)
    {
        var time = Time.time;
        while (true)
        {
            if (time + scanTime < Time.time)
            {
                StartCoroutine(Patrol());
                yield break;
            }
            if (targetInfo.Strength > SignatureStrength.Lost)
            {
                StartCoroutine(Attack(target, targetInfo));
                yield break;
            }
            yield return null;
        }
    }
    
    private IEnumerator Attack(ThermalSignature target, SignatureInfo targetInfo)
    {
        _destination = transform.position;
        while (true)
        {
            _launcherComponent.AddLaunchOrder(target);
            if (targetInfo.Strength == SignatureStrength.Lost)
            {
                StartCoroutine(Search(target, targetInfo));
                yield break;
            }
            yield return null;
        }
    }
    
    private bool EvaluateSignature(SignatureInfo info)
    {
        if (info.Strength > SignatureStrength.Lost && info.Allegiance == Allegiance.Player && info.Type == SignatureType.Ship)
        {
            if (Signature.ThermalOutput >= _targetMaxThermalOutput)
            {
                if (info.Strength == SignatureStrength.Strong)
                {
                    return true;
                }
                return false;
            }
            return true;
        }
        return false;
    }

    private bool IsLookingTowardsVector(Vector3 vector)
    {
        var targetRotation = Quaternion.LookRotation(-(transform.position - vector).normalized);
        return (1 - Mathf.Abs(Quaternion.Dot(transform.rotation, targetRotation)) <= aimPrecision);
    }
    
    private void RotateToWardsVector(Vector3 newPosition)
    {
        var targetRotation = Quaternion.LookRotation(-(transform.position - newPosition).normalized);
        _rigidbody.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime));
    }
}
