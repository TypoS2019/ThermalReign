using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using Random = UnityEngine.Random;

[RequireComponent(typeof(LocalVolumetricFog))]
public class Texture3dChanger : MonoBehaviour
{
    private LocalVolumetricFog _fog;
    [SerializeField] private float maxDistance;
    [SerializeField] private List<Vector3> points;
    [SerializeField] private Cell cellDirection;
    private void Awake()
    {
        _fog = GetComponent<LocalVolumetricFog>();
    }

    void Start()
    {
        Texture3D texture3D = (Texture3D) _fog.parameters.volumeMask;
        Debug.Log(texture3D.depth);
        Debug.Log(texture3D.height);
        Debug.Log(texture3D.width);
        Debug.Log(texture3D.mipmapCount);
    }

    void Update()
    {
        Texture3D texture3D = (Texture3D) _fog.parameters.volumeMask;

        // Color[] colors = texture3D.GetPixels();
        // Color[] colorsA = new Color[texture3D.width * texture3D.depth * texture3D.height];
        
        for (int x = 0; x < texture3D.width; x++)
        {
            for (int y = 0; y < texture3D.height; y++)
            {
                for (int z = 0; z < texture3D.depth; z++)
                {
                    float shortestDistance = float.MaxValue;
                    foreach (var point in points)
                    {
                        float distance = Vector3.Distance(new Vector3(x, y, z), point);
                        shortestDistance = distance < shortestDistance ? distance : shortestDistance;
                    }

                    if (shortestDistance < maxDistance)
                    {
                        texture3D.SetPixel(x,y,z, new Color(1,1,1, Mathf.Min(Mathf.Abs(((int) cellDirection)-(shortestDistance/maxDistance)), 1)));
                    }
                    else
                    {
                        texture3D.SetPixel(x,y,z, new Color(1,1,1,0));
                    }
                    
                    // if (distance < maxDistance)
                    // {
                    //     Debug.Log(texture3D.GetPixel(x,y,z));
                    //     texture3D.SetPixel(x,y,z, new Color(1,1,1,0));
                    //     Debug.Log("set: " + x + " " + y + " " + z);
                    // }
                }
            }
        }
        texture3D.Apply();
        // for (int i = 0; i < points.Count; i++)
        // {
        //     var point = points[i];
        //     point.Set(point.x + Random.Range(-1, 1) * Time.deltaTime, point.y + Random.Range(-1, 1) * Time.deltaTime, point.z + Random.Range(-1, 1) * Time.deltaTime);
        //     points[i] = point;
        // }
    }

    public enum Cell
    {
        Inward = 0,
        Outward = 1
    }
}
