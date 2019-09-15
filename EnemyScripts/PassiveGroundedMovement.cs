using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveGroundedMovement : MonoBehaviour, ITimeEffected {

    //Movement variables.
    public float SPEED;     //How far enemy will go in each frame
    public float DISTANCE;    //The max distance enemy will go from his original spot
    private float curSpeed;     //Current speed

    //Reference variables on other components
    private CollisionSystem enemyCollision;
    private Transform enemy;
    private float leftBound;
    private float rightBound;

	// Use this for initialization
	void Start () {
        //Set reference variables
        enemyCollision = GetComponent<CollisionSystem>();
        enemy = GetComponent<Transform>();

        //Set bounds
        leftBound = enemy.position.x;
        rightBound = leftBound + DISTANCE;

        curSpeed = SPEED;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if(enemyCollision.grounded) {

            //If player is over left bound, move right
            if (enemy.position.x < leftBound && curSpeed < 0)
                StartCoroutine(reverseDirection());

            //If player is over right bound, move left
            if (enemy.position.x > rightBound && curSpeed > 0)
                StartCoroutine(reverseDirection());

            //Player hit enemy, go the other way and reset
            if (enemyCollision.hitEnemy) {
                StartCoroutine(reverseDirection());
            }

            //Update speed per frame
            enemy.position += Vector3.right * curSpeed;
        }

    }

    //If get damage, reverse direction
    public void getDamage(DamagePackage dmgPackage) {
        //if knockback contradicts with curspeed, reverse direction
        if (dmgPackage.getKnockback().x > 0 && curSpeed < 0 || dmgPackage.getKnockback().x < 0 && curSpeed > 0)
            StartCoroutine(reverseDirection());
    }

    //Apply slow to horizontal movement
    public void applySlow(float timeFactor) {
        curSpeed *= timeFactor;
    }

    //Apply pause to horizontal movement
    public IEnumerator applyPause(float pauseDuration) {
        //Pausing
        int curDir = (curSpeed < 0) ? -1 : 1;
        curSpeed = 0;

        yield return new WaitForSeconds(0.1f);      //To ensure that the checker will happen AFTER status is paused
        yield return new WaitUntil(GetComponent<EnemyStatus>().unpaused);

        //Unpausing
        curSpeed = curDir * SPEED * GetComponent<EnemyStatus>().getTimeState();
    }

    private const float TURN_DELAY = 0.75f;

    //Reverses direction by pausing to turn then go the other way
    public IEnumerator reverseDirection() {
        int curDirection = (curSpeed > 0) ? 1 : -1;
        curSpeed = 0;

        yield return new WaitForSeconds(TURN_DELAY * (1 / GetComponent<EnemyStatus>().getTimeState()));
        yield return new WaitUntil(GetComponent<EnemyStatus>().unpaused);

        curSpeed = -1 * SPEED * curDirection * GetComponent<EnemyStatus>().getTimeState();
    }
}
