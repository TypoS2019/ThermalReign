using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DetailGenerateStep : GenerateStep
{
    public override GameObject Process(GameObject gameObject)
    {
        AsteroidData data = gameObject.GetComponent<AsteroidData>();

        Mesh mesh = AddCraters(gameObject.GetComponent<MeshFilter>().sharedMesh, data!.CraterAmount, data!.CraterGrouping,
            data!.maxCraterSize, data!.minCraterSize, data!.CraterDepth);
        gameObject.GetComponent<MeshFilter>()!.mesh = mesh;
        gameObject.GetComponent<MeshCollider>()!.sharedMesh = mesh;
        
        return gameObject;
    }

    public override void AddGUI()
    {
        
    }

    private Mesh AddCraters(Mesh mesh, int amount, float grouping, float maxCraterSize, float minCraterSize, float craterDepth)
    {
        
        List<Vector3> craterPoints = new List<Vector3>();
        List<Vector3> craterNormals = new List<Vector3>();
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        
        for (int i = 0; i < amount; i++)
        {
            int r = Random.Range(0, vertices.Length);
            craterPoints.Add(vertices[r]);
            craterNormals.Add(normals[r]);
        }
        for(int i = 0; i< craterNormals.Count; i++)
        {
            CraterCreator.addCraterToMeshOnPosition(mesh, craterPoints[i], -craterNormals[i], Random.Range(minCraterSize, maxCraterSize), craterDepth);
        }
        
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        return mesh;
    }
    
    
    private Mesh AddCraters(Mesh mesh, float multiplier, float grouping, float maxCraterSize, float minCraterSize, float craterDepth)
    {
        List<Vector3> craterPoints = new List<Vector3>();
        List<Vector3> craterNormals = new List<Vector3>();
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        for(int i = 0; i< vertices.Length; i++)
        {
            if (CalculateVertexCrater(vertices[i], normals[i], multiplier, grouping))
            {
                craterPoints.Add(vertices[i]);
                craterNormals.Add(normals[i]);
            }
        }
        for(int i = 0; i< craterNormals.Count; i++)
        {
            CraterCreator.addCraterToMeshOnPosition(mesh, craterPoints[i], -craterNormals[i], Random.Range(minCraterSize, maxCraterSize), craterDepth);
        }
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        return mesh;
    }

    private bool CalculateVertexCrater(Vector3 vertex, Vector3 normal, float multiplier, float grouping)
    {
        
        float noise = Mathf.PerlinNoise(vertex.x, vertex.y) + 
                      Mathf.PerlinNoise(vertex.y, vertex.z) +
                      Mathf.PerlinNoise(vertex.z, vertex.x);
        float r = Random.Range(0.0f, Mathf.Max(1.0f, noise * grouping));
        if (r >= 1 - multiplier) return true;
        return false;
    }
}
