using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsSleepOnStart : MonoBehaviour {

    Rigidbody rb;

	// Use this for initialization
	void Start () {
		
        rb = GetComponent<Rigidbody>();

        if (rb != null)
            rb.Sleep();
        
	}
	
}
