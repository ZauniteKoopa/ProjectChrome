using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationarySpikeBehavior : MonoBehaviour {

    //Reference variables
    private EnemyStatus status;
    public Material critHealthMat;

	// Use this for initialization
	void Start () {
        status = GetComponent<EnemyStatus>();
	}
	
	// Update is called once per frame
	void Update () {
        //If at critical health, change material
        if (status.health == 1)
            GetComponent<MeshRenderer>().material = critHealthMat;
	}
}
