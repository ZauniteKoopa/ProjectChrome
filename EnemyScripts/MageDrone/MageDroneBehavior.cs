using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageDroneBehavior : MonoBehaviour, ITimeEffected {

    //Reference variables
    private Transform target;
    private EnemyStatus status;
    private Rigidbody rb;

    //Aggression stopwatch
    private Stopwatch aggression;
    private float curInterval;
    private const float AGGRESSION_INTERVAL = 1.5f;

    //Teleportation Helpers
    public Transform colliderCollector;
    public Transform anticipationShadow;
    private const float MIN_RADIUS = 9.75f;
    private const float MAX_RADIUS = 10.75f;

    //Attack Transforms / Materials
    public Material attackMat;
    public Material defaultMat;
    public Transform bombMissile;
    public Transform selfDestruct;

    // Use this for initialization
	void Start () {
        //Establish reference variables
        status = GetComponent<EnemyStatus>();
        rb = GetComponent<Rigidbody>();

        //Establish stopwatch
        aggression = gameObject.AddComponent<Stopwatch>();
        aggression.setTime(AGGRESSION_INTERVAL);
        curInterval = AGGRESSION_INTERVAL;
	}
	
	// Update is called once per frame
	void Update () {
        //Initiates aggression
        if (aggression.getTime() == 0) 
            aggression.start();

        //Allows aggression after interval passed
        if (target != null && aggression.getTime() >= curInterval)
            StartCoroutine(teleport());

    }

    //Alerted when enemy is in sensor
    void foundTarget(Transform tgt) {
        target = tgt;
    }

    //Accessor Method for target
    public Transform getTarget() {
        return target;
    }

    private const float ANTICIPATION_DELAY = 0.5f;

    //IEnumerator that allows for enemy attacks and teleportation
    IEnumerator teleport() {
        //Stops stopwatch
        aggression.stop();
        aggression.setTime(0.001f);

        //Sets it up
        Transform collector = Instantiate(colliderCollector, target);
        yield return new WaitForSeconds(0.05f);                         //Delay allows collector to process potential collisions
        Vector3 destPoint = findTelePoint(collector.GetComponent<ColliderCollecting>().getPotentialCollisions());
        UnityEngine.Object.Destroy(collector.gameObject);

        //Anticipation
        Transform telegraphedTele = Instantiate(anticipationShadow, destPoint, Quaternion.identity);
        yield return new WaitForSeconds(ANTICIPATION_DELAY / status.getTimeState());
        yield return new WaitUntil(GetComponent<EnemyStatus>().unpaused);

        //Teleports
        UnityEngine.Object.Destroy(telegraphedTele.gameObject);
        GetComponent<Transform>().position = destPoint;

        //Attacks and resets timer
        yield return StartCoroutine(attack());
        aggression.reset();
    }

    //Probability chance by RNG out of 10. If not a sticky missile, self destruct
    private const int STICKY_MISSILE_CHANCE = 7;
    private const int MAX_CHANCE = 10;

    //Allows attacks - 2 different attacks chosen by RNG
    IEnumerator attack() {
        GetComponent<MeshRenderer>().material = attackMat;
        yield return new WaitForSeconds(ANTICIPATION_DELAY / status.getTimeState());
        yield return new WaitUntil(GetComponent<EnemyStatus>().unpaused);

        //Choose attack by RNG and then instantiate object
        int chosenAttack = UnityEngine.Random.Range(0, MAX_CHANCE);

        if (chosenAttack < STICKY_MISSILE_CHANCE)
            Instantiate(bombMissile, gameObject.transform);
        else
            Instantiate(selfDestruct, gameObject.transform);

        GetComponent<MeshRenderer>().material = defaultMat;
    }

    //Finds point to teleport to and checks if it hits any collisions. If so, pick another point and reset
    private Vector3 findTelePoint(List<Collider> potentialCollisions) {
        Bounds testTeleport = pickPoint();

        //Checks for any potential collisions
        for(int i = 0; i < potentialCollisions.Count; i++)
            if (testTeleport.Intersects(potentialCollisions[i].bounds)){
                //If potential collision found, pick another point
                testTeleport = pickPoint();
                i = -1;     //To account for i++
            }

        return testTeleport.center;
    }

    //Picks a center point to create bounding box in for checking potential collisions
    private Bounds pickPoint() {
        //Randomly picks radius and x value
        float radius = UnityEngine.Random.Range(MIN_RADIUS, MAX_RADIUS);
        float x = UnityEngine.Random.Range(-radius, radius);

        //Calculates y value from 2 randomly generated values using pythag and randomly chooses 1 of 2 points
        float y = (float)Math.Pow((double)radius, 2) - (float)Math.Pow((double)x, 2);
        y = (float)Math.Sqrt((double)y);

        y = (UnityEngine.Random.Range(0, 2) == 1) ? y : -1 * y;

        return new Bounds(new Vector3(x + target.position.x, y + target.position.y, GetComponent<Transform>().position.z), GetComponent<Collider>().bounds.size);
    }

    //Recoil constant when getting damaged
    private const float RECOIL_DURATION = 0.1f;

    //When damaged, move according to knockback
    private IEnumerator getDamage(DamagePackage dmgPackage) {
        rb.constraints = ~RigidbodyConstraints.FreezePositionY & ~RigidbodyConstraints.FreezePositionX;
        yield return new WaitForSeconds(RECOIL_DURATION);
        rb.velocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
    }

    //Applies slow to enemy
    public void applySlow(float timeFactor) {
        if(timeFactor < 1) {
            curInterval /= timeFactor;
            aggression.setTime(aggression.getTime() / timeFactor);
        }else{
            aggression.setTime(aggression.getTime() / timeFactor);
            curInterval /= timeFactor;
        }
    }

    //Applies pause to enemy
    public IEnumerator applyPause(float pauseDuration) {
        //Pausing
        if (aggression.getTime() == 0 || aggression.getTime() >= curInterval)
            aggression.setTime(0.01f);

        float percentPassed = aggression.getTime() / curInterval;
        aggression.stop();

        //Will keep pausing until the end of the duration
        yield return new WaitForSeconds(0.1f);      //To ensure that the checker will happen AFTER status is paused
        yield return new WaitUntil(GetComponent<EnemyStatus>().unpaused);

        //Unpausing
        aggression.setTime(curInterval * percentPassed);
        aggression.start();
    }
}
