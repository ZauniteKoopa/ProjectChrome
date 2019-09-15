using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeOut : MonoBehaviour {

    private const float MAX_TIMEOUT = 5f;

	// Use this for initialization
	void Start () {
        Object.Destroy(gameObject, MAX_TIMEOUT);
	}

}
