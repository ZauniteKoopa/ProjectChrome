using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderCollecting : MonoBehaviour {

    private List<Collider> potentialCollisions;

	// Use this for initialization
	void Start () {
        potentialCollisions = new List<Collider>();
	}
	
	// Collects colliders that trigger collision
	void OnTriggerEnter(Collider other) {
        if(other.tag == "Platform" || other.tag == "PausedProjectile")
            potentialCollisions.Add(other);
    }

    //Accessor Method for potentialCollisions
    public List<Collider> getPotentialCollisions() {
        return potentialCollisions;
    }

}
