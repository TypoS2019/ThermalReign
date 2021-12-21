using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CraterCreator
{
    public static Mesh addCraterToMeshOnPosition(Mesh mesh, Vector3 position, Vector3 direction, float craterSize, float craterDepth)
    {
        //Get all the vertices of the Component in an array
        List<Vector3> vertices = mesh.vertices.ToList();

        for (int i = 0; i < vertices.Count; i++)
        {
            //Get the distance of the vertex to the center of the crater.
            float distance = Vector3.Distance(vertices[i], position);
            
            //Check if the distance falls within the range of the crater.   
            if (distance <= craterSize)
            {
                //Calculate the offset the vertex needs to be moved with.
                float temp = Mathf.Pow(craterDepth * ((craterSize - distance) / craterSize), 0.5f);

                //Changes the position of the copied vertices using the direction of the crater, vertex offset and the cratersize.
                vertices[i] = (vertices[i] + direction * temp * craterSize);
            }
        }

        mesh.vertices = vertices.ToArray();
        return mesh;
    }
}