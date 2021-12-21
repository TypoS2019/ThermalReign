using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GravityScriptableObject", menuName = "ScriptableObjects/GravityScriptableObject", order = 1)]
public class GravityScript : ScriptableObject
{
    //reversegravityscriptfalloff is the power of how long it takes for the objects to lose most gravitational pull, the higher the number the faster the fall off
    public float reverseGravityStrengthFallOff;

    //list of all AsteroidAttractor scripts
    public List<AsteroidAttractor> attractors;
}
