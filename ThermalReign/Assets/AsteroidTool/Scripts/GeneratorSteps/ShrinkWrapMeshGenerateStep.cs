using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using System.Threading;

public class ShrinkWrapMeshGenerateStep : GenerateStep
{
    private string meshName = "Asteroid Mesh";
    private string shrinkObjectName = "ShrinkObject";
    private string layer = "AsteroidToolLayer";
    
    public override GameObject Process(GameObject gameObject)
    {
        //Set mesh to null to prevent it from intervening with shrinking.
        gameObject.GetComponent<MeshFilter>()!.mesh = null;
        gameObject.GetComponent<MeshCollider>()!.sharedMesh = null;
        
        //Create ico-sphere mesh used for shrinking.
        Mesh mesh = GetIcoSphereMesh(gameObject.GetComponent<AsteroidData>()!.subDivideRecursions, gameObject.GetComponent<AsteroidData>()!.indexFormat);
        mesh.name = meshName;
        //Calculate the distance to the furthest collider of the children objects.
        float range = CalculateAsteroidRange(gameObject.transform);
        //Add a game object with the ico-sphere mesh and the range.
        GameObject shrinkObject = AddShrinkGameObject(mesh, gameObject.transform, range*2);
        
        //Add the layer mask to the children objects.
        AddLayerMaskToPrimitiveShapes(gameObject);
        //Shrink the ico-sphere object down to the primitve objects.
        mesh = Shrink(shrinkObject, gameObject);
        //disable the primitive objects and remove the layer mask.
        DisablePrimitiveShapes(gameObject);

        //Reapply the mesh to the asteroid object.
        gameObject.GetComponent<MeshFilter>()!.mesh = mesh;
        gameObject.GetComponent<MeshCollider>()!.sharedMesh = mesh;
        return gameObject;
    }

    public override void AddGUI()
    {
        
    }
    
    private Mesh GetIcoSphereMesh(int subdivideRecursions, IndexFormat indexFormat)
    {
        IcoSphereMesh ico = new IcoSphereMesh();
        ico.InitAsIcosohedron();
        ico.Subdivide(subdivideRecursions);
        return ico.GenerateMesh(indexFormat);
    }

    private GameObject AddShrinkGameObject(Mesh mesh, Transform parent, float diameter)
    {
        GameObject shrinkObject = new GameObject(shrinkObjectName);
        shrinkObject.transform.parent = parent;
        shrinkObject.transform.position = parent.position;
        shrinkObject.transform.localScale = new Vector3(diameter, diameter, diameter);
        shrinkObject.AddComponent<MeshFilter>()!.mesh = mesh;
        return shrinkObject;
    }

    private Mesh Shrink(GameObject shrinkObject, GameObject parent)
    {
        MeshFilter meshFilter = shrinkObject.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.sharedMesh;
        Transform transform = shrinkObject.transform;
        Vector3[] vertices = new Vector3[mesh.vertices.Length];
        
        System.Array.Copy(mesh.vertices, vertices, vertices.Length);

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = ShrinkVertex(vertices[i], mesh.normals[i], transform, parent.transform);
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        
        Object.DestroyImmediate(shrinkObject);
        
        return mesh;
    }

    private Vector3 ShrinkVertex(Vector3 vertex, Vector3 normal, Transform transform, Transform parent)
    {
        Vector3 rayDirection = -normal; RaycastHit hit;
        if (Physics.Raycast( transform.TransformPoint(vertex), rayDirection, out hit, Vector3.Distance(transform.TransformPoint(vertex), transform.position), layerMask: LayerMask.GetMask(layer) ) ) {
            return parent.InverseTransformPoint(hit.point);
        }
        return Vector3.zero;
    }

    private void AddLayerMaskToPrimitiveShapes(GameObject gameObject)
    {
        foreach (Transform child in gameObject.transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer(layer);
        }
    }
    
    private void DisablePrimitiveShapes(GameObject gameObject)
    {
        foreach (Transform child in gameObject.transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Default");
            child.gameObject.SetActive(false);
        }
    }

    private float CalculateAsteroidRange(Transform transform)
    {
        float maxDistance = 0;
        foreach (Transform child in transform)
        {
            Bounds bounds = child.GetComponent<Collider>().bounds;
            if (bounds != null)
            {
                Vector3 max = bounds.max;
                Vector3 min = bounds.min;
                List<Vector3> corners = new List<Vector3>();
                corners.Add(max); 
                corners.Add(min);
                corners.Add(new Vector3(min.x, min.y, max.z));
                corners.Add(new Vector3(min.x, max.y, min.z));
                corners.Add(new Vector3(max.x, min.y, min.z));
                corners.Add(new Vector3(min.x, max.y, max.z));
                corners.Add(new Vector3(max.x, min.y, max.z));
                corners.Add(new Vector3(max.x, max.y, min.z));
                foreach (var corner in corners)
                {
                    float distance = Vector3.Distance(corner, transform.position);
                    maxDistance = distance > maxDistance ? distance : maxDistance;
                }
            }
        }
        return maxDistance;
    }
}
