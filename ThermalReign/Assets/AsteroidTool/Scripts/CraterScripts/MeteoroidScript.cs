using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteoroidScript : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject, 0.1f);
    }
}
