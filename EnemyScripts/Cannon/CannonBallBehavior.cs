using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBallBehavior : MonoBehaviour, ITimeEffected , IKillable{
    //Max distance it will take before being deleted
    private const float MAX_DISTANCE = 50f;
    private float curDistance;

    //Velocity variables. Distance per frame
    private float setSpeed;
    private float speed;
    private Vector3 direction;

    //Reference Variables
    private Transform ball;

    //DamagePackage Variables
    private const float KNOCKBACK_FACTOR = 200f;
    private DamagePackage dmgPackage;

    // Use this for initialization
	void Start () {
        //Sets the velocity based on direction booleans from source
        CannonBehavior sourceStats = GetComponentInParent<CannonBehavior>();
        direction = Vector3.zero;
        speed = sourceStats.setSpeed;
        setSpeed = speed;

        if (sourceStats.goesLeft)
            direction += Vector3.left;
        else if (sourceStats.goesRight)
            direction += Vector3.right;

        if (sourceStats.goesUp)
            direction += Vector3.up;
        else if (sourceStats.goesDown)
            direction += Vector3.down;

        Vector3 knockback = direction * KNOCKBACK_FACTOR;

        //Sets other variables
        ball = GetComponent<Transform>();
        float damage = GetComponent<ProjectileStatus>().damage;
        dmgPackage = new DamagePackage(damage, knockback);
        curDistance = 0;
        GetComponent<ProjectileStatus>().source = transform.root;

        //Detach from parent
        transform.parent = null;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        //Update velocity / speed
        ball.position += direction * speed;
        curDistance += speed;

        //If curDistance reached max distance, destroy object
        if (curDistance >= MAX_DISTANCE)
            Object.Destroy(gameObject);
	}

    //Checking trigger
    void OnTriggerEnter(Collider other) {
        //Check if cannonball hits player. If so, apply damage
        if((other.tag == "Player" || other.tag == "Platform" || other.tag == "PausedProjectile") && ball.tag == "EnemyAttack"){
            if(other.tag == "Player")
                other.SendMessage("getDamage", dmgPackage);

            Object.Destroy(gameObject);
        }
    }

    //Apply time effects by effecting projectile speed
    public void applySlow(float timeFactor) {
        speed *= timeFactor;
    }

    //Apply damage if hit by player attack
    public void getDamage(DamagePackage dmgPackage) {
        Object.Destroy(gameObject);
    }

    //Applies pause to this cannon ball
    public IEnumerator applyPause(float pauseDuration) {
        //Pausing
        speed = 0;
        GetComponent<Collider>().isTrigger = false;

        //Will keep pausing until the end of the duration
        yield return new WaitForSeconds(0.1f);      //To ensure that the checker will happen AFTER status is paused
        yield return new WaitUntil(GetComponent<ProjectileStatus>().unpaused);

        //Unpausing
        speed = setSpeed * GetComponent<ProjectileStatus>().getTimeState();
        GetComponent<Collider>().isTrigger = true;
    }
}
