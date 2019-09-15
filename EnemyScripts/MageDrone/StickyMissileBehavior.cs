using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyMissileBehavior : MonoBehaviour, ITimeEffected {

    //Missile Speed
    private DamagePackage dmgPackage;
    private Vector3 direction;
    private const float KNOCKBACK_VAL = 500f;
    private const float SET_SPEED = 0.25f;
    private float curSpeed;

    //Time out for space efficiency
    private const float MAX_DISTANCE = 60f;
    private float curDistance;

    //Bomb set up
    private bool activated;
    private bool triggered;
    private const float BOMB_ANTICIPATION = 4.25f;
    public Transform explosion;

    //Materials
    public Material activeMat;
    public Material criticalMat;

    // Use this for initialization
    void Start() {
        if (GetComponentInParent<MageDroneBehavior>().getTarget() == null)
            throw new System.Exception("Error: Nothing to aim");

        curDistance = 0;

        //Create direction
        Vector3 startingPos = GetComponentInParent<Transform>().position;
        Vector3 terminalPos = GetComponentInParent<MageDroneBehavior>().getTarget().position;
        direction = SystemCalc.findDirection(startingPos, terminalPos);
        curSpeed = SET_SPEED;

        //Create Damage Package
        float damage = GetComponent<ProjectileStatus>().damage;
        dmgPackage = new DamagePackage(damage, direction * KNOCKBACK_VAL);

        //Detach from parent while maintaining reference to source
        GetComponent<ProjectileStatus>().source = transform.root;
        transform.parent = null;
    }

    // Update is called once per frame
    void Update (){

        //Times out after going a certain distance
        if (curDistance >= MAX_DISTANCE)
            UnityEngine.Object.Destroy(gameObject);
    }

    //Fixed update for movement
    void FixedUpdate(){
        //Move projectile if bomb not activated
        if (!activated){
            GetComponent<Transform>().position += direction * curSpeed;
            curDistance += curSpeed;
        }
    }

    //When collision, activate bomb if not activated
    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Player" || other.tag == "Platform" || other.tag == "PausedProjectile") {
            if (other.tag == "Player") {
                other.SendMessage("getDamage", dmgPackage);

                //If player steps on air / land mine, trigger explosion
                if (activated)
                    triggered = true;
            }

            if (!activated) {
                activated = true;
                GetComponent<MeshRenderer>().material = activeMat;
                StartCoroutine(explode());
            }
        }
    }

    private const int NUM_FRAMES = 30;

    //IEnumerator that allows fair explosion
    IEnumerator explode() {
        float frameDuration = BOMB_ANTICIPATION / NUM_FRAMES;
        yield return new WaitForSeconds(0.5f);
        UnityEngine.Object.Destroy(GetComponent<Rigidbody>());

        //Frame checks
        for(int curFrame = 0; curFrame < NUM_FRAMES; curFrame++) {

            yield return new WaitForSeconds(frameDuration / GetComponent<ProjectileStatus>().getTimeState());
            yield return new WaitUntil(GetComponent<ProjectileStatus>().unpaused);

            //Check if triggered to break through loop
            if (triggered)
                curFrame = NUM_FRAMES;

            //If on last third, change materials
            if (curFrame >= NUM_FRAMES * 2.0f / 3.0f && GetComponent<ProjectileStatus>().unpaused())
                GetComponent<MeshRenderer>().material = (curFrame % 2 == 1) ? activeMat : criticalMat;
        }

        //Explodes
        Instantiate(explosion, GetComponent<Transform>().position, Quaternion.identity);
        UnityEngine.Object.Destroy(gameObject);
    }

    //If hit by player, activate the bomb
    public void getDamage(DamagePackage dmgPackage) {
        if (!activated){
            activated = true;
            GetComponent<MeshRenderer>().material = activeMat;
            StartCoroutine(explode());
        }
    }

    //Apply slow
    public void applySlow(float timeFactor) {
        curSpeed *= timeFactor;
    }

    //Apply pause
    public IEnumerator applyPause(float pauseDuration){
        //Pausing
        curSpeed = 0;
        GetComponent<Collider>().isTrigger = false;

        //Will keep pausing until the end of the duration
        yield return new WaitForSeconds(0.1f);      //To ensure that the checker will happen AFTER status is paused
        yield return new WaitUntil(GetComponent<ProjectileStatus>().unpaused);

        //Unpausing
        curSpeed = SET_SPEED * GetComponent<ProjectileStatus>().getTimeState();
        GetComponent<Collider>().isTrigger = true;

    }
}
