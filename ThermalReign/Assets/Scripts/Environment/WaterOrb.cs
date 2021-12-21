using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterOrb : MonoBehaviour
{
    private List<Rigidbody> _rigidbodies;
    [SerializeField] private float strength;
    [SerializeField] private float raduis;
    
    private void Awake()
    {
        _rigidbodies = new List<Rigidbody>();
    }

    private void FixedUpdate()
    {
        foreach (var rigidbody in _rigidbodies)
        {
            var distance = Vector3.Distance(rigidbody.position, transform.position);
            rigidbody.AddForce((rigidbody.position - transform.position).normalized * strength * Mathf.Max(raduis - distance, 0), ForceMode.Force);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody != null)
        {
            _rigidbodies.Add(other.attachedRigidbody);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody != null)
        {
            _rigidbodies.Remove(other.attachedRigidbody);
        }
    }
}
