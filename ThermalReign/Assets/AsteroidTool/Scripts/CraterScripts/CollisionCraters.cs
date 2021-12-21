using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[RequireComponent(typeof(AsteroidData), typeof(Rigidbody))]
public class CollisionCraters : MonoBehaviour
{
    private Mesh mesh;
    private AsteroidData asteroidData;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        // Get the mesh of the Component
        mesh = GetComponent<MeshFilter>().mesh;
        asteroidData = GetComponent<AsteroidData>();
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 impactVector = collision.relativeVelocity * collision.rigidbody.mass;


        //Check if the relativeVelocity * Mass produces enough force to crater the asteroid
        if (impactVector.magnitude >= asteroidData.minForceRequired)
        {
            var craterSize = Mathf.Max(Mathf.Min(asteroidData.maxCraterSize, impactVector.magnitude), asteroidData.minCraterSize) * asteroidData.impactForceMultiplier;


            CraterCreator.addCraterToMeshOnPosition(mesh, transform.InverseTransformPoint(collision.rigidbody.position), impactVector.normalized, craterSize, asteroidData.CraterDepth);

            //Recalculate the position of the changed vertices
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            GetComponent<MeshCollider>().sharedMesh = mesh;

            MassScript.CalculateMass(gameObject, rb, asteroidData.asteroidDensity);
        }
    }
}
