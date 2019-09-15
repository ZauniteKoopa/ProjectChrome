using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBehavior : MonoBehaviour, ITimeEffected {

    //Fire Rate variables
    public float fireRate;
    private Stopwatch loopCycle;
    public float startTime;
    private bool firstProjectile;   //Check if fired first projectile

    //Projectile to be fired
    public Transform projectile;
    public float setSpeed;      //Default speed for projectile

    //Direction variables for projectile
    public bool activatable;
    public bool goesLeft;
    public bool goesRight;
    public bool goesUp;
    public bool goesDown;

	// Use this for initialization
	void Start () {
        if (goesRight && goesLeft || goesUp && goesDown)
            throw new System.ArgumentException("Error: Invalid direction set for cannon");

        if ((!goesRight && !goesLeft && !goesUp && !goesDown) || setSpeed == 0f)
            throw new System.ArgumentException("Error: 0 velocity set for cannon");

        firstProjectile = true;
        loopCycle = gameObject.AddComponent<Stopwatch>();
	}
	
	// Update is called once per frame
	void Update () {
        if(!activatable) {
            //Starts timer
            if (loopCycle.getTime() == 0 || firstProjectile){
                //If first projectile, set timer in advance to start time
                if (firstProjectile){
                    loopCycle.setTime(startTime);
                    firstProjectile = false;
                }

                loopCycle.start();
            }

            //Resets timer and adds projectile
            if (loopCycle.getTime() >= fireRate){
                loopCycle.hardReset();
                Instantiate(projectile, GetComponent<Transform>());
            }
        }
    }

    //Apply time effects by editing the fireRate but maintaining ratio between current / fireRate
    public void applySlow(float timeFactor) {
        timeFactor = 1 / timeFactor;
        fireRate *= timeFactor;
        loopCycle.setTime(loopCycle.getTime() * timeFactor);
    }

    //Pauses Cannon
    public IEnumerator applyPause(float pauseDuration) {
        //Pausing by stopping the stopwatch
        if (loopCycle.getTime() == 0 || loopCycle.getTime() >= fireRate)    //To avoid the cycle from continuing in different circumstances
            loopCycle.setTime(0.001f);
            
        loopCycle.stop();

        yield return new WaitForSeconds(0.1f);      //To ensure that the checker will happen AFTER status is paused
        yield return new WaitUntil(GetComponent<EnemyStatus>().unpaused);

        //Unpausing
        loopCycle.start();
    }

    //If activated by a button, shoot a cannonball
    public void activate() {
        if (activatable)
            Instantiate(projectile, GetComponent<Transform>());
        else
            throw new System.Exception("Error: Should not be activatable. Change settings if supposed to activate");
    }
}
