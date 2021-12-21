using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsGenerateStep : GenerateStep
{
    public override GameObject Process(GameObject gameObject)
    {
        AsteroidData data = gameObject.GetComponent<AsteroidData>();
        if (data!.addColisions && gameObject.GetComponent<CollisionCraters>() == null)
        {
            gameObject.AddComponent<CollisionCraters>();
        }

        if (data!.addGravity)
        {
            var asteroidAttractor = gameObject.GetComponent<AsteroidAttractor>() == null ? gameObject.AddComponent<AsteroidAttractor>() : gameObject.GetComponent<AsteroidAttractor>();
            asteroidAttractor.density = data!.asteroidDensity;
        }

        Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
        }
        return gameObject;
    }

    public override void AddGUI()
    {
        
    }
}
