using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySensing : MonoBehaviour {

    //Checks if player triggers sensing. If he/she is, alert parent that target found
    void OnTriggerEnter(Collider other) {
        if (other.tag == "Player") {
            SendMessageUpwards("foundTarget", other.transform);
            Object.Destroy(gameObject);
        }
    }

}
