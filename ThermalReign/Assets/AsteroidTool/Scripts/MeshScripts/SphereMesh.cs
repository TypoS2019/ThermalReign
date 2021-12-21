using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;


public class Polygon
{
    public List<int> m_Vertices;

    public Polygon(int a, int b, int c)
    {
        m_Vertices = new List<int>() {a, b, c};
    }
}

public class IcoSphereMesh
{
    public List<Polygon> m_Polygons;
    public List<Vector3> m_Vertices;

    public void InitAsIcosohedron()
    {
        
        m_Polygons = new List<Polygon>();
        m_Vertices = new List<Vector3>();

        //math to find icosahedron point.
        float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

        //Adding the 12 vertices needed to create an icosahedron.
        m_Vertices.Add(new Vector3(-1, t, 0).normalized);
        m_Vertices.Add(new Vector3(1, t, 0).normalized);
        m_Vertices.Add(new Vector3(-1, -t, 0).normalized);
        m_Vertices.Add(new Vector3(1, -t, 0).normalized);
        m_Vertices.Add(new Vector3(0, -1, t).normalized);
        m_Vertices.Add(new Vector3(0, 1, t).normalized);
        m_Vertices.Add(new Vector3(0, -1, -t).normalized);
        m_Vertices.Add(new Vector3(0, 1, -t).normalized);
        m_Vertices.Add(new Vector3(t, 0, -1).normalized);
        m_Vertices.Add(new Vector3(t, 0, 1).normalized);
        m_Vertices.Add(new Vector3(-t, 0, -1).normalized);
        m_Vertices.Add(new Vector3(-t, 0, 1).normalized);
        
        
        //Connecting the 12 vertices into 20 sides.
        m_Polygons.Add(new Polygon(0, 11, 5));
        m_Polygons.Add(new Polygon(0, 5, 1));
        m_Polygons.Add(new Polygon(0, 1, 7));
        m_Polygons.Add(new Polygon(0, 7, 10));
        m_Polygons.Add(new Polygon(0, 10, 11));
        m_Polygons.Add(new Polygon(1, 5, 9));
        m_Polygons.Add(new Polygon(5, 11, 4));
        m_Polygons.Add(new Polygon(11, 10, 2));
        m_Polygons.Add(new Polygon(10, 7, 6));
        m_Polygons.Add(new Polygon(7, 1, 8));
        m_Polygons.Add(new Polygon(3, 9, 4));
        m_Polygons.Add(new Polygon(3, 4, 2));
        m_Polygons.Add(new Polygon(3, 2, 6));
        m_Polygons.Add(new Polygon(3, 6, 8));
        m_Polygons.Add(new Polygon(3, 8, 9));
        m_Polygons.Add(new Polygon(4, 9, 5));
        m_Polygons.Add(new Polygon(2, 4, 11));
        m_Polygons.Add(new Polygon(6, 2, 10));
        m_Polygons.Add(new Polygon(8, 6, 7));
        m_Polygons.Add(new Polygon(9, 8, 1));
    }

    //Divides every polygon into 4 smaller polygons.
    public void Subdivide(int recursions)
    {
        var midPointCache = new Dictionary<int, int>();
        for (int i = 0; i < recursions; i++)
        {
            var newPolys = new List<Polygon>();
            foreach (var poly in m_Polygons)
            {
                int a = poly.m_Vertices[0];
                int b = poly.m_Vertices[1];
                int c = poly.m_Vertices[2];
                
                int ab = GetMidPointIndex(midPointCache, a, b);
                int bc = GetMidPointIndex(midPointCache, b, c);
                int ca = GetMidPointIndex(midPointCache, c, a);
      
                newPolys.Add(new Polygon(a, ab, ca));
                newPolys.Add(new Polygon(b, bc, ab));
                newPolys.Add(new Polygon(c, ca, bc));
                newPolys.Add(new Polygon(ab, bc, ca));
            }
            m_Polygons = newPolys;
        }
    }

    public int GetMidPointIndex(Dictionary<int, int> cache, int indexA, int indexB)
    {
        int smallerIndex = Mathf.Min(indexA, indexB);
        int greaterIndex = Mathf.Max(indexA, indexB);
        int key = (smallerIndex << 16) + greaterIndex; 
        int ret;
        if (cache.TryGetValue(key, out ret)) return ret;
        Vector3 p1 = m_Vertices[indexA];
        Vector3 p2 = m_Vertices[indexB];
        Vector3 middle = Vector3.Lerp(p1, p2, 0.5f).normalized;
        ret = m_Vertices.Count;
        m_Vertices.Add(middle);
        cache.Add(key, ret);
        return ret;
    }

    private Vector2[] CreateUV (Vector3[] vertices)
    {
        Vector2[] uv = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++) {
            Vector3 v = vertices[i];
            Vector2 textureCoordinates;
            textureCoordinates.x = Mathf.Atan2(v.x, v.z) / (-2f * Mathf.PI);
            if (textureCoordinates.x < 0f) {
                textureCoordinates.x += 1f;
            }
            textureCoordinates.y = Mathf.Asin(v.y) / Mathf.PI + 0.5f;
            uv[i] = textureCoordinates;
        }

        return uv;
    }
    
    public Mesh GenerateMesh(IndexFormat indexFormat = IndexFormat.UInt16)
    {
        Mesh terrainMesh = new Mesh();
        terrainMesh.indexFormat = indexFormat;
        int vertexCount = m_Polygons.Count * 3;
        int[] indices = new int[vertexCount];
        List<Vector3> verticesList = new List<Vector3>();
        
        for (int i = 0; i < m_Polygons.Count; i++)
        {
            AddVerticesAndIndices(i, 0);
            AddVerticesAndIndices(i, 1);
            AddVerticesAndIndices(i, 2);
        }
        
        terrainMesh.vertices = verticesList.ToArray();
        terrainMesh.normals = verticesList.ToArray();
        terrainMesh.uv = CreateUV(terrainMesh.vertices);
        terrainMesh.SetTriangles(indices, 0);
        terrainMesh.RecalculateTangents();
        return terrainMesh;
        
        void AddVerticesAndIndices( int index, int side)
        {
            Polygon poly = m_Polygons[index];
            int i = verticesList.IndexOf(m_Vertices[poly.m_Vertices[side]]);
            if (i == -1)
            {
                verticesList.Add(m_Vertices[poly.m_Vertices[side]]);
                indices[index * 3 + side] = verticesList.Count-1;
            }
            else
            {
                indices[index * 3 + side] = i;
            }
        }
    }

    
}

