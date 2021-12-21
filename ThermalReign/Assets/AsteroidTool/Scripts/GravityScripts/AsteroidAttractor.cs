using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AsteroidAttractor : MonoBehaviour
{
    //semi-realistic gravity strengh (unchangable)
    const float G = 6.674f;

    //script with values for designers to play with
    public GravityScript gravityScript;

    //volume of an object which can only be viewed
    [ReadOnly]
    public float volume;

    //set density
    public float density;

    //current object rigidbody
    public Rigidbody rb;

    //AsteroidAttractor objToAttract: script of obj which has to be attracted to current obj
    //function that attracts the given object to the current object using addforce
    void Attract(AsteroidAttractor objToAttract)
    {
        //get other objects rigidboyd
        Rigidbody rbToAttract = objToAttract.rb;

        //get distance lenght between current object and other object
        Vector3 direction = rb.position - rbToAttract.position;
        float distance = direction.magnitude;

        //save resources if objects are next to each other
        if (distance <= 0.01f)
        {
            return;
        }

        //calculate strenght of pull using G * (mass / disance^2)
        //reversegravityscriptfalloff is the power of how long it takes for the objects to lose most gravitational pull, the higher the number the faster the fall off
        float forceMagnitude = G * (rb.mass * rbToAttract.mass) / Mathf.Pow(distance, gravityScript.reverseGravityStrengthFallOff);

        //get correct direction for pull with calculated force
        Vector3 force = direction.normalized * forceMagnitude;

        //push the object towards current object (pull force)
        rbToAttract.AddForce(force * Time.deltaTime);
    }

        //set base values when script is enabled
        void OnEnable()
    {
        //set current object rigidbody
        rb = gameObject.GetComponent<Rigidbody>();

        //caluclate and set mass of current object
        MassScript.CalculateMass(gameObject, rb, density);
        
        //set gravity to false for script to work
        rb.useGravity = false;
        
        //add current script to array with all AsteroidAttractor scripts
        gravityScript.attractors.Add(this);
    }

    //runs when current script is disabled
    private void OnDisable()
    {
        //remove current script from array with all AsteroidAttractor scripts
        gravityScript.attractors.Remove(this);
    }

    //runs every physics update
    void FixedUpdate()
    {
        //calculate every gravity pull from all other AsteroidAttractor objects towards current object 
        foreach (AsteroidAttractor attractor in gravityScript.attractors)
        {
            //doesn't attract current object to itselfs
            if (attractor != this)
            {
                Attract(attractor);
            }
        }
    }


}