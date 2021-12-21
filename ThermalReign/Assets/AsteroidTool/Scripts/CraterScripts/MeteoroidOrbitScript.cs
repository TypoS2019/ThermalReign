using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteoroidOrbitScript : MonoBehaviour
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        rb.AddForce(Vector3.left * 400);
    }

    void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject, 0.1f);
    }
}
