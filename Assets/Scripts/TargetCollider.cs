using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCollider : MonoBehaviour
{
    public Collider catheter;

    private bool hit = false;

    public bool getHit() { return hit; }

    void OnTriggerEnter(Collider collision)
    {
        hit = (collision == catheter) ? true : hit;
    }

    void OnTriggerExit(Collider collision)
    {
        hit = (collision == catheter) ? false : hit;
    }
}
