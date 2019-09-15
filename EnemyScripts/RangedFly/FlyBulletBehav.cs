using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyBulletBehav : MonoBehaviour {

    //Damage and velocity
    private DamagePackage dmgPackage;
    private Vector3 direction;
    private const float KNOCKBACK_VAL = 500f;
    private const float SET_SPEED = 0.25f;
    private float curSpeed;

    //Time out variables
    private float curDistance;
    private const float MAX_DISTANCE = 60f;

    // Use this for initialization
    void Start() {
        if (GetComponentInParent<FlyShooterBehav>().getTarget() == null)
            throw new System.Exception("Error: Nothing to aim");

        //Create direction
        Vector3 startingPos = GetComponentInParent<Transform>().position;
        Vector3 terminalPos = GetComponentInParent<FlyShooterBehav>().getTarget().position;
        direction = SystemCalc.findDirection(startingPos, terminalPos);
        curSpeed = SET_SPEED;

        //Create Damage Package
        float damage = GetComponent<ProjectileStatus>().damage;
        dmgPackage = new DamagePackage(damage, direction * KNOCKBACK_VAL);

        //Time out variables
        curDistance = 0;

        //detach from parent while maintaining reference to source
        GetComponent<ProjectileStatus>().source = transform.root;
        transform.parent = null;
    }

    // Update is called once per frame
    void FixedUpdate() {
        GetComponent<Transform>().position += direction * curSpeed;
        curDistance += curSpeed;

        //Time out when reached a certain distance
        if (curDistance >= MAX_DISTANCE)
            UnityEngine.Object.Destroy(gameObject);

    }

    //Checks collisions with projectile
    void OnTriggerEnter(Collider other) {

        if (other.tag == "Player" || other.tag == "Platform" || other.tag == "PausedProjectile")
        {
            if (other.tag == "Player")
                other.SendMessage("getDamage", dmgPackage);

            UnityEngine.Object.Destroy(gameObject);
        }
    }

    //Applies slow & reverses slow
    public void applySlow(float timeFactor) {
        curSpeed *= timeFactor;
    }

    //When hit by attack
    void getDamage(DamagePackage dmgPackage) {
        UnityEngine.Object.Destroy(gameObject);
    }

    //Applies pause
    public IEnumerator applyPause(float pauseDuration) {
        //Pausing
        curSpeed = 0;
        GetComponent<Collider>().isTrigger = false;

        //Will keep pausing until the end of the duration
        yield return new WaitForSeconds(0.1f);      //To ensure that the checker will happen AFTER status is paused
        yield return new WaitUntil(GetComponent<ProjectileStatus>().unpaused);

        //Unpause
        curSpeed = SET_SPEED * GetComponent<ProjectileStatus>().getTimeState();
        GetComponent<Collider>().isTrigger = true;
    }
}

