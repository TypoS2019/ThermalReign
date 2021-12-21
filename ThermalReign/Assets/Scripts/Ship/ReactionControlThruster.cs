using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ReactionControlThruster : MonoBehaviour
{
    [SerializeField] private Active onYaw = Active.None;
    [SerializeField] private Active onRoll = Active.None;
    [SerializeField] private Active onPitch = Active.None;
    [SerializeField] private Active onStrafeX = Active.None;
    [SerializeField] private Active onStrafeY = Active.None;
    [SerializeField] private Active onStrafeZ = Active.None;
    [SerializeField] private int particleCount;
    [SerializeField] private float deadZone = 0.1f;
    private ParticleSystem _particleSystem;
    private bool _active = false;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    public void ReactToThrustVectors(Vector3 force, Vector3 torque)
    {
        _active = false;
        _active = (torque.x > deadZone && onPitch == Active.Positive) || _active;
        _active = (torque.x < -deadZone && onPitch == Active.Negative) || _active;
        
        _active = (torque.y > deadZone && onYaw == Active.Positive) || _active;
        _active = (torque.y < -deadZone && onYaw == Active.Negative) || _active;
        
        _active = (torque.z > deadZone && onRoll == Active.Positive) || _active;
        _active = (torque.z < -deadZone && onRoll == Active.Negative) || _active;
        
        _active = (force.x > deadZone && onStrafeX == Active.Positive) || _active;
        _active = (force.x < -deadZone && onStrafeX == Active.Negative) || _active;
        
        _active = (force.y > deadZone && onStrafeY == Active.Positive) || _active;
        _active = (force.y < -deadZone && onStrafeY == Active.Negative) || _active;
        
        _active = (force.z > deadZone && onStrafeZ == Active.Positive) || _active;
        _active = (force.z < -deadZone && onStrafeZ == Active.Negative) || _active;
    }

    private void Update()
    {
        if (_active)
        {
            _particleSystem.Emit(Mathf.CeilToInt(particleCount * Time.deltaTime));
        }
    }

    public enum Active
    {
        Positive,
        Negative,
        None
    }
}