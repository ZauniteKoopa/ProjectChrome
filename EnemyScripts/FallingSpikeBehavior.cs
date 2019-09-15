using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingSpikeBehavior : MonoBehaviour, ITimeEffected, IKillable {

    //Reference variables
    private Transform spike;
    private Rigidbody rb;
    private Vector3 spawnPoint;

    //Damage Variables
    private float spikeDamage;
    private DamagePackage dmgPackage;

    //Status variables
    private bool slowed;
    private Vector3 timeForce;

    //Natural fall force to make it seem normal after time effects ended
    private const float NATURAL_FALL = -800f;

    // Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
        spike = GetComponent<Transform>();
        spawnPoint = spike.position;

        //Setting default characteristics
        spikeDamage = GetComponent<ProjectileStatus>().damage;
        dmgPackage = new DamagePackage(spikeDamage, Vector3.zero);
        slowed = false;
        rb.useGravity = true;
        gameObject.tag = "EnemyAttack";
	}

    //Runs this every frame
    void FixedUpdate() {
        if (slowed)
            rb.AddForce(timeForce);
    }

    //If collides with something, destroy/respawn. Hit player? Apply damage
    void OnCollisionEnter(Collision collision) {
        Collider collider = collision.collider;

        if(gameObject.tag != "PausedProjectile" || collider.tag == "Platform") {
            //If collider is a player, apply damage
            if (collider.tag == "Player")
                collider.BroadcastMessage("getDamage", dmgPackage);

            //Set default characteristic
            slowed = false;
            rb.useGravity = true;
            gameObject.tag = "EnemyAttack";

            //Respawn
            spike.position = spawnPoint;
        }
    }

    //If hit by player, destroy / respawn
    public void getDamage(DamagePackage dmgPackage) {
        //Set default characteristic
        slowed = false;
        rb.useGravity = true;
        gameObject.tag = "EnemyAttack";

        //Respawn
        spike.position = spawnPoint;
    }


    //If affected by time, create an opposing force to gravity based on time factor
    public void applySlow(float timeFactor) {
        //Checks if enemy is entering time Effects, if not, then it must be exiting time effects
        if (timeFactor < 1) {
            rb.velocity = Vector3.zero;
            slowed = true;
            timeForce = rb.mass * Physics.gravity * (1 - timeFactor) * -1;

        } else {
            rb.useGravity = true;
            slowed = false;
            rb.AddForce(0f, rb.mass * NATURAL_FALL, 0f); //Adds downward force to make it seem like going back to normal gravity
        }
    }

    //Applies pause
    public IEnumerator applyPause(float pauseDuration) {
        //Pausing
        rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;

        yield return new WaitForSeconds(0.1f);      //To ensure that the checker will happen AFTER status is paused
        yield return new WaitUntil(GetComponent<ProjectileStatus>().unpaused);

        //Unpausing
        rb.constraints = ~RigidbodyConstraints.FreezePositionY & ~RigidbodyConstraints.FreezePositionX;

        //Adds downward force to make it seem natural
        rb.AddForce(0f, rb.mass * NATURAL_FALL * GetComponent<ProjectileStatus>().getTimeState(), 0f);
    }
}
