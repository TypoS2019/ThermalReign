using System;
using System.Collections;
using System.Collections.Generic;
using Thermal;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(ThermalSignature))]
public class GuidedProjectile : MonoBehaviour
{
    private ThermalSignature _target;
    private Vector3 _targetLastKnownLocation;

    private Rigidbody _rigidbody;
    private Vector3 _movement;
    private ThermalSignature _signature;
    
    private float _aimPrecision = 0.001f;
    private float _rotateSpeed = 3;
    private float _forwardForce = 10;
    private float _stabilizeForce = 4;
    private float _detonateDistance = 50;
    private float _scanningStrength = 100;
    private float _minimumVisibleThermalEnergy = 0.1f;
    private bool _armed = false;
    
    [SerializeField] private float _fuel = 100000f;
    [SerializeField] private float _angularFuel = 1000f;
    [SerializeField] private List<ParticleSystem> detonationEffects;
    [SerializeField] private ParticleSystem exhaustParticleSystem;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _signature = GetComponent<ThermalSignature>();
        _movement = Vector3.zero;
    }

    private void Start()
    {
        _signature.type = SignatureType.GuidedProjectile;
    }

    private void FixedUpdate()
    {
        _rigidbody.AddRelativeForce(_movement);
        _signature.ThermalInput = _movement.magnitude;
        _fuel -= _movement.magnitude;
        if (_fuel <= 0)
        {
            StartCoroutine(Detonate());
        }
        exhaustParticleSystem.Emit(Mathf.CeilToInt(_movement.z * 100));
    }

    public void Launch(ThermalSignature target)
    {
        _target = target;
        _armed = false;
        StartCoroutine(Aim());
        StartCoroutine(Scan());
    }

    private IEnumerator Scan()
    {
        while (true)
        {
            RaycastHit hit;
                if (Physics.Raycast(transform.position, _target.transform.position - transform.position, out hit))
                {
                    float visibleThermalEnergy =
                        Scanning.CalcVisibleThermalEnergy(_target, transform.position, _scanningStrength);
                    if (visibleThermalEnergy > _minimumVisibleThermalEnergy && hit.transform == _target.transform)
                    {
                        _targetLastKnownLocation = _target.transform.position;
                    }
                }
                yield return null;
        }
    }
    
    private IEnumerator Aim()
    {
        while (true)
        {
            Stabilize(true, true, true);
            if (RotateToWardsTarget())
            {
                StartCoroutine(Track());
                yield break;
            }
            yield return null;
        }
    }

    private IEnumerator Track()
    {
        _armed = true;
        _movement.z = _forwardForce;
        while (true)
        {
            Stabilize(true, true, false);
            RotateToWardsTarget();
            float angle = Vector3.Angle(transform.forward,_target.transform.position - transform.position);
            if (Vector3.Distance(_targetLastKnownLocation, transform.position) <= _detonateDistance || Mathf.Abs(angle) > 90)
            {
                StartCoroutine(Detonate());
                yield break;
            }
            yield return null;
        }
    }

    private IEnumerator Detonate()
    {
        detonationEffects.ForEach((system => system.Emit(1)));
        Debug.Log("boom");
        Destroy(gameObject, 1f);
        yield break;
    }

    private bool RotateToWardsTarget()
    {
        var currentPosition = _rigidbody.position;
        var currentRotation = _rigidbody.rotation;
        var targetRotation = Quaternion.LookRotation(-(currentPosition - _targetLastKnownLocation).normalized);
        var newRotation = Quaternion.Slerp(currentRotation, targetRotation, _rotateSpeed * Time.fixedDeltaTime);
        _rigidbody.MoveRotation(newRotation);
        return (1 - Mathf.Abs(Quaternion.Dot(currentRotation, targetRotation)) <= _aimPrecision);

    }

    private void Stabilize(bool x, bool y, bool z)
    {
        Vector3 localVelocity = transform.InverseTransformDirection(_rigidbody.velocity);
        _movement.x = x ? -(Mathf.Max(Mathf.Min(localVelocity.x, 1),-1) * _stabilizeForce) : _movement.x;
        _movement.y = y ? -(Mathf.Max(Mathf.Min(localVelocity.y, 1),-1) * _stabilizeForce) : _movement.y;
        _movement.z = z ? -(Mathf.Max(Mathf.Min(localVelocity.z, 1),-1) * _stabilizeForce) : _movement.z;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (_armed)
        {
            StartCoroutine(Detonate());
        }
    }
}
