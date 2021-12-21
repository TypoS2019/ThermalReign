using UnityEngine;
using UnityEngine.Rendering;

public class AsteroidData : MonoBehaviour
{
    [Range(1, 6)]
    public int subDivideRecursions = 5;    

    [Range(1, 200)]
    public int smoothRecursions = 100;
    public IndexFormat indexFormat = IndexFormat.UInt32;
    public float ShrinkDiameter;
    
    [Range(1, 100)]
    public float asteroidDensity = 1.3f;
    public bool addGravity = true;
    
    //Crater fields
    public float CraterGrouping = 0.6f;
    public float CraterMultiplier;
    [Range(1, 20)]
    public float maxCraterSize = 10;
    [Range(0.1f, 5)]
    public float minCraterSize = 1;
    public int CraterAmount = 100;
    [Range(0.1f, 10)]
    public float CraterDepth = 0.25f;
    [Range(0.1f, 10)]
    public float minForceRequired = 10;
    public bool addColisions = true;
    [Range(0.1f, 10)]
    public float impactForceMultiplier = 1f;
}
