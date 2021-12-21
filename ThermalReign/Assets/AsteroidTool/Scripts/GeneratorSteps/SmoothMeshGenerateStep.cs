using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothMeshGenerateStep : GenerateStep
{

	
    public override GameObject Process(GameObject gameObject)
    {
	    Mesh mesh = MeshSmoothing.LaplacianFilter(gameObject.GetComponent<MeshFilter>()!.sharedMesh, gameObject.GetComponent<AsteroidData>()!.smoothRecursions);
		
	    mesh.RecalculateBounds();
	    mesh.RecalculateNormals();
	    mesh.RecalculateTangents();
	    
	    gameObject.GetComponent<MeshFilter>().mesh = mesh;
	    gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
	    
        return gameObject;
    }

    public override void AddGUI()
    {
	    
    }
}

public class MeshSmoothing {
	
		public static Mesh LaplacianFilter (Mesh mesh, int times = 1) {
			mesh.vertices = LaplacianFilter(mesh.vertices, mesh.triangles, times);
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			return mesh;
		}

		public static Vector3[] LaplacianFilter(Vector3[] vertices, int[] triangles, int times) {
			var network = VertexConnection.BuildNetwork(triangles);
			for(int i = 0; i < times; i++) {
				vertices = LaplacianFilter(network, vertices, triangles);
			}
			return vertices;
		}

		static Vector3[] LaplacianFilter(Dictionary<int, VertexConnection> network, Vector3[] origin, int[] triangles) {
			Vector3[] vertices = new Vector3[origin.Length];
			for(int i = 0, n = origin.Length; i < n; i++) {
				var connection = network[i].Connection;
				var v = Vector3.zero;
				foreach(int adj in connection) {
					v += origin[adj];
				}
				vertices[i] = v / connection.Count;
			}
			return vertices;
		}

		/*
		 * HC (Humphrey’s Classes) Smooth Algorithm - Reduces Shrinkage of Laplacian Smoother
		 * alpha 0.0 ~ 1.0
		 * beta  0.0 ~ 1.0
		*/
		public static Mesh HCFilter (Mesh mesh, int times = 5, float alpha = 0.5f, float beta = 0.75f) {
			mesh.vertices = HCFilter(mesh.vertices, mesh.triangles, times, alpha, beta);
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			return mesh;
		}

		static Vector3[] HCFilter(Vector3[] vertices, int[] triangles, int times, float alpha, float beta) {
			alpha = Mathf.Clamp01(alpha);
			beta = Mathf.Clamp01(beta);

			var network = VertexConnection.BuildNetwork(triangles);

			Vector3[] origin = new Vector3[vertices.Length];
			Array.Copy(vertices, origin, vertices.Length);
			for(int i = 0; i < times; i++) {
				vertices = HCFilter(network, origin, vertices, triangles, alpha, beta);
			}
			return vertices;
		}
			
		public static Vector3[] HCFilter(Dictionary<int, VertexConnection> network, Vector3[] o, Vector3[] q, int[] triangles, float alpha, float beta) {
			Vector3[] p = LaplacianFilter(network, q, triangles);
			Vector3[] b = new Vector3[o.Length];

			for(int i = 0; i < p.Length; i++) {
				b[i] = p[i] - (alpha * o[i] + (1f - alpha) * q[i]);
			}

			for(int i = 0; i < p.Length; i++) {
				var adjacents = network[i].Connection;
				var bs = Vector3.zero;
				foreach(int adj in adjacents) {
					bs += b[adj];
				}
				p[i] = p[i] - (beta * b[i] + (1 - beta) / adjacents.Count * bs);
			}

			return p;
		}

	}

	
public class VertexConnection {

	public HashSet<int> Connection { get { return connection; } }

	HashSet<int> connection;

	public VertexConnection() {
		this.connection = new HashSet<int>();
	}

	public void Connect (int to) {
		connection.Add(to);
	}

	public static Dictionary<int, VertexConnection> BuildNetwork (int[] triangles) {
		var table = new Dictionary<int, VertexConnection>();

		for(int i = 0, n = triangles.Length; i < n; i += 3) {
			int a = triangles[i], b = triangles[i + 1], c = triangles[i + 2];
			if(!table.ContainsKey(a)) {
				table.Add(a, new VertexConnection());
			}
			if(!table.ContainsKey(b)) {
				table.Add(b, new VertexConnection());
			}
			if(!table.ContainsKey(c)) {
				table.Add(c, new VertexConnection());
			}
			table[a].Connect(b); table[a].Connect(c);
			table[b].Connect(a); table[b].Connect(c);
			table[c].Connect(a); table[c].Connect(b);
		}

		return table;
	}

}
