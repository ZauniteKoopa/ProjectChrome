using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyShooterBehav : MonoBehaviour, ITimeEffected{
    //Reference variables
    private Rigidbody rb;
    private Transform enemy;
    private EnemyStatus status;

    //Target Zoning
    private Transform target;
    private const float MAX_DISTANCE = 10f;
    private const float MIN_DISTANCE = 9.5f;

    //Speed variables
    private const float SET_SPEED = 0.115f;
    private float curSpeed;

    //Ranged Attack
    private Stopwatch attackInterval;
    private float curMaxInterval;
    private const float MAX_INTERVAL = 2f;
    public Transform projectile;
    private bool shooting;

    //Recovering
    private bool recovering;

    //Materials
    public Material shootMat;
    public Material defaultMat;

    // Start is called before the first frame update
    void Start(){
        rb = GetComponent<Rigidbody>();
        enemy = GetComponent<Transform>();
        status = GetComponent<EnemyStatus>();
        curSpeed = SET_SPEED;
        curMaxInterval = MAX_INTERVAL;
        attackInterval = gameObject.AddComponent<Stopwatch>();
    }

    // Update is called once per frame
    void FixedUpdate(){
        //Activates enemy once 
        if(target != null) {
            //Projectile Timer
            if (attackInterval.getTime() == 0)  //If attack interval == 0, reset.
                attackInterval.start();

            if (attackInterval.getTime() >= curMaxInterval && !shooting) //If attack interval > set interval, spawn projectile
                StartCoroutine(shootProjectile());

            //Movement
            float distance = SystemCalc.findDistance(target.position, enemy.position);
            Vector3 direction;

            //Decide whether to go towards player or away from player depending on distance
            if (distance > MAX_DISTANCE)
                direction = SystemCalc.findDirection(enemy.position, target.position);
            else if (distance < MIN_DISTANCE)
                direction = SystemCalc.findDirection(target.position, enemy.position);
            else
                direction = Vector3.zero;

            enemy.position += direction * curSpeed;
        }
    }

    private const float ANTICIPATION_DELAY = 0.5f;

    //IEnumerator to execute shooting attack
    private IEnumerator shootProjectile() {
        shooting = true;
        GetComponent<MeshRenderer>().material = shootMat;

        yield return new WaitForSeconds(ANTICIPATION_DELAY / (1 / status.getTimeState()));
        yield return new WaitUntil(status.unpaused);

        GetComponent<MeshRenderer>().material = defaultMat;
        Object.Instantiate(projectile, enemy);
        attackInterval.hardReset();
        shooting = false;
    }

    //Called by the enemy sensor
    public void foundTarget(Transform tgt) {
        target = tgt;
    }

    //Accessor method to enemy target
    public Transform getTarget(){
        return target;
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
        yield return new WaitForSeconds(RECOIL_DURATION);
        rb.velocity = Vector3.zero;
        yield return new WaitForSeconds(STUN_DURATION);
        curSpeed = status.unpaused() ? SET_SPEED * status.getTimeState(): 0 ;
        recovering = false;
    }

    //Recoils back when it gets damaged
    void getDamage(DamagePackage dmgPackage) {
        if (!recovering)
            StartCoroutine(recoilStun());
    }

    //Applies slow
    public void applySlow(float timeFactor) {
        curSpeed *= timeFactor;
        curMaxInterval *= (1 / timeFactor);
        attackInterval.setTime(attackInterval.getTime() * (1 / timeFactor));
    }

    //Applies pause and reverse pause
    public IEnumerator applyPause(float timeDuration){
        //Pausing
        curSpeed = 0;
        attackInterval.stop();

        //Will keep pausing until the end of the duration
        yield return new WaitForSeconds(0.1f);      //To ensure that the checker will happen AFTER status is paused
        yield return new WaitUntil(status.unpaused);

        //Unpausing
        attackInterval.start();
        curSpeed = SET_SPEED * status.getTimeState();
    }
}
