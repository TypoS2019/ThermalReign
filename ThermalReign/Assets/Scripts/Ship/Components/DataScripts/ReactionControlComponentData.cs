using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ReactionControlComponentData : ComponentData
{
    public float directionalForceToPowerRatio;
    public float angularForceToPowerRatio;
        
    public float directionalForce;
    public float angularForce;
}
