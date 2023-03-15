using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{

	public float angle = 30;
	
	private Transform transform;

    // Start is called before the first frame update
    void Start()
    {
        transform = gameObject.transform;
    }

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(Vector3.zero, Vector3.up, angle);
    }
}
