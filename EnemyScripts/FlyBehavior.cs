using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyBehavior : MonoBehaviour {
    //Reference variables
    private Rigidbody rb;
    private Transform enemy;

    //Target chasing
    private Transform target;
    private Vector3 direction;

    //Speed Variables
    private const float SET_SPEED = 0.12f;
    private float curSpeed;

    //Recovery checker
    private bool recovering;

	// Use this for initialization
	void Start () {
        curSpeed = SET_SPEED;
        rb = GetComponent<Rigidbody>();
        enemy = GetComponent<Transform>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        //Checks if target is found, move player
		if(target != null) {
            direction = SystemCalc.findDirection(enemy.position, target.position);
            enemy.position += direction * curSpeed;
        }
    }

    //Find Target method called by Sensor
    public void foundTarget(Transform tgt) {
        target = tgt;
    }

    //Method that allows recoil when hit collision (since rb is completely fixed)
    void OnCollisionEnter(Collision collision) {
        if (collision.collider.tag == "Player")
            StartCoroutine(recoilStun());
    }

    private const float RECOIL_DURATION = 0.2f;
    private const float STUN_DURATION = 0.15f;

    //Establish recoil stun
    IEnumerator recoilStun() {
        curSpeed = 0;
        recovering = true;
        yield return new WaitForSeconds(RECOIL_DURATION * (1 / GetComponent<EnemyStatus>().getTimeState()));
        rb.velocity = Vector3.zero;
        yield return new WaitForSeconds(STUN_DURATION * (1 / GetComponent<EnemyStatus>().getTimeState()));
        curSpeed = GetComponent<EnemyStatus>().unpaused() ? SET_SPEED * GetComponent<EnemyStatus>().getTimeState() : 0 ;
        recovering = false;
    }

    //Recoils back when it gets damaged
    void getDamage(DamagePackage dmgPackage) {
        if(!recovering)
            StartCoroutine(recoilStun());
    }

    //Applies slow and reverses slow
    public void applySlow(float timeFactor) {
        curSpeed *= timeFactor;
    }

    //Applies pause and reverse pause
    public IEnumerator applyPause(float timeDuration) {
        //Pausing
        curSpeed = 0;

        //Will keep pausing until the end of the duration
        yield return new WaitForSeconds(0.1f);      //To ensure that the checker will happen AFTER status is paused
        yield return new WaitUntil(GetComponent<EnemyStatus>().unpaused);

        //Unpausing
        curSpeed = SET_SPEED * GetComponent<EnemyStatus>().getTimeState();
    }
}
