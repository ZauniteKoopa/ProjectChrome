using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseShockBehavior : MonoBehaviour {

    private static float SHOCK_DURATION = 0.1f;
    private static float PAUSE_DURATION = 4.0f;

	// Use this for initialization
	void Start () {
        //Have the field apear for a small timeframe and then destroy it
        Object.Destroy(gameObject, SHOCK_DURATION);
    }
	
	// If an enemy or enemy attack triggers it and its not already paused, enemy / enemy attack will be paused
    void OnTriggerEnter(Collider other) {
        //Enemies
        if (other.tag == "Enemy" && other.GetComponent<EnemyStatus>().unpaused())
            other.SendMessage("applyPause", PAUSE_DURATION);

        //Enemy attacks
        if(other.tag == "EnemyAttack" && other.GetComponent<ProjectileStatus>().unpaused())
            other.SendMessage("applyPause", PAUSE_DURATION);
    }
}
