using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassScript
{
    //calculate volume based on local scale
    public static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float v321 = p3.x * p2.y * p1.z;
        float v231 = p2.x * p3.y * p1.z;
        float v312 = p3.x * p1.y * p2.z;
        float v132 = p1.x * p3.y * p2.z;
        float v213 = p2.x * p1.y * p3.z;
        float v123 = p1.x * p2.y * p3.z;

        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }

    //calculate volume based on local scale
    public static float VolumeOfMesh(Mesh mesh, GameObject gameObject)
    {
        float localVolume = 0;

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 p1 = vertices[triangles[i + 0]];
            Vector3 p2 = vertices[triangles[i + 1]];
            Vector3 p3 = vertices[triangles[i + 2]];
            localVolume += SignedVolumeOfTriangle(p1, p2, p3);
        }
        localVolume *= gameObject.transform.localScale.x * gameObject.transform.localScale.y * gameObject.transform.localScale.z;
        Debug.Log(Mathf.Abs(localVolume));
        return Mathf.Abs(localVolume);
    }

    //calculates the mass of the current object
    public static void CalculateMass(GameObject gameObject, Rigidbody rb, float density)
    {
        Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;

        //set volume based on localscale
        float volume = VolumeOfMesh(mesh, gameObject);

        //calculate and set mass by: volume * density
        rb.mass = volume * density;
    }
}
