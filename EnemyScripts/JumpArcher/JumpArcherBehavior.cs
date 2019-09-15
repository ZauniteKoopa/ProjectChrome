using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpArcherBehavior : MonoBehaviour, ITimeEffected {

    //Reference variables
    private CollisionSystem enemyCollision;
    private Rigidbody rb;
    private Transform enemy;
    public Transform projectile;

    //Constant movement variables
    private const float JUMP_FORCE = 1500f;
    private const float DAMPEN_FORCE = 60f;
    private const float DAMPEN_LIMIT = 15f;
    private const float JUMP_X_MOVE = 0.1f;
    private const float RUSH_MOVE = 0.35f;
    private float curJumpXMove;
    private float curRushMove;

    //Time variables
    private const float NATURAL_FALL = -800f;
    private const float DECAY_RATE = 3f;
    private const float DECAY_START_FACTOR = 90f;
    private float decayUpForce;
    private float slowFactor;
    private bool slowed;

    //Target finder and aggression
    private Transform target;
    private float curXTarget;
    private Stopwatch aggression;
    private bool rushing;
    private float aggressionInterval;
    public Material defaultMat;
    public Material rushingMat;         //Attack material
    private bool firedProjectile;

    //Probability variables - Chance out of 100
    private const int STATIC_JUMP_CHANCE = 35;
    private const int JUMP_FORWARD_CHANCE = 35;
    private const int RUSH_CHANCE = 30;
    private const int MAX_CHANCE = 100;
    private int divider1;
    private int divider2;

	// Use this for initialization
	void Start () {
        //Reference variables
        enemyCollision = GetComponent<CollisionSystem>();
        rb = GetComponent<Rigidbody>();
        enemy = GetComponent<Transform>();

        //Movement and aggression variables
        curJumpXMove = 0f;
        curRushMove = 0f;
        aggression = gameObject.AddComponent<Stopwatch>();
        aggressionInterval = 0.6f;
        firedProjectile = true; //Allows not firing a projectile at the start

        //Set dividers for probability
        divider1 = STATIC_JUMP_CHANCE;
        divider2 = STATIC_JUMP_CHANCE + JUMP_FORWARD_CHANCE;
	}
	
	// Update is called once per frame
	void Update () {
        //starts timer when target found
        if (aggression.getTime() == 0f && target != null)
            aggression.start();

        //If found target and agression interval passed, set up aggression
        if (aggression.getTime() >= aggressionInterval && enemyCollision.grounded)
            aggressiveMove();
    }
    
    //Fixed update on movement
    void FixedUpdate() {

        aggressiveExecution();  //executes aggression

        //Dampen jump to lower floatiness
        if (!enemyCollision.grounded && Mathf.Abs(rb.velocity.y) < DAMPEN_LIMIT)
            rb.AddForce(Vector3.down * DAMPEN_FORCE);

        //Movement when slowed
        if (slowed){
            //Reduces velocity by slowFactor
            rb.velocity *= slowFactor * 1.6f;

            //Adds a decaying force to make up for reduced velocity in physics system if object going up
            if (rb.velocity.y > 0 && decayUpForce > 0){
                rb.AddForce(decayUpForce * slowFactor * Vector3.up);
                decayUpForce -= DECAY_RATE;
            }
        }
    }

    //Method to be called when sensor found target
    void foundTarget(Transform tgt) {
        target = tgt;
    }

    //Accessor Method for target
    public Transform getTarget(){
        return target;
    }

    //Set up Movements made if target is found
    void aggressiveMove(){
        //Resets timer and sets it to a place so it won't start
        aggression.setTime(0.001f);
        aggression.stop();

        //If player if right of transform, go right, else, go left
        curXTarget = target.position.x;
        int direction = (curXTarget > enemy.position.x) ? 1 : -1;

        //If slowed, try to jump out
        if (slowed) {
            curJumpXMove = -1 * direction * GetComponent<EnemyStatus>().getTimeState() * JUMP_X_MOVE;
            rb.AddForce(Vector3.up * JUMP_FORCE);
            firedProjectile = false;
        }else{
            //Randomly choose a move
            int chosenMove = Random.Range(0, MAX_CHANCE);

            if(chosenMove >= 0 && chosenMove < divider2) {  //Jump
                if (chosenMove >= 0 && chosenMove < divider1)
                    curJumpXMove = 0;                           //Static jump
                else
                    curJumpXMove = JUMP_X_MOVE * direction;     //Jump towards target

                firedProjectile = false;
                rb.AddForce(Vector3.up * JUMP_FORCE);       //Actual jump
            }else 
                StartCoroutine(rushAnticipation(direction));         //Rush
        }
    }

    //Executes aggressive movements that have been set up
    void aggressiveExecution() {
        //Establishing walled variable
        bool walled = (curJumpXMove < 0 && GetComponent<CollisionSystem>().leftWalled) || (curJumpXMove > 0 && GetComponent<CollisionSystem>().rightWalled);
        float curMove = (rushing) ? curRushMove : curJumpXMove;

        if (curMove < 0)
            walled = GetComponent<CollisionSystem>().leftWalled;
        else
            walled = GetComponent<CollisionSystem>().rightWalled;

        //Constantly move by curJumpXMovement when jumping
        if (!enemyCollision.grounded && !walled) {
            enemy.position += Vector3.right * curJumpXMove;

            //Allows arrow to be fired when at peak of jump if not paused
            if (Mathf.Abs(rb.velocity.y) < 1.0f && GetComponent<EnemyStatus>().unpaused() && !firedProjectile)
                StartCoroutine(shootAnticipation());
        }

        //If doing a rush attack, rush
        if (rushing && GetComponent<EnemyStatus>().unpaused()) {
            enemy.position += Vector3.right * curRushMove;

            //If went too far off, stop rushing
            if (curRushMove < 0 && enemy.position.x < curXTarget || curRushMove > 0 && enemy.position.x > curXTarget || walled) {
                rushing = false;
                curRushMove = 0;
                aggression.hardReset();
                GetComponent<MeshRenderer>().material = defaultMat;
            }
        }
    }

    //Const for Anticipation for attacks
    private const float ANTICIPATION_DURATION = 0.5f;
    private const float AERIAL_ANTICIPATION = 0.25f;

    //Sets anticipation for rush for the player
    IEnumerator rushAnticipation(int direction) {
        GetComponent<MeshRenderer>().material = rushingMat;
        rushing = true;

        yield return new WaitForSeconds(ANTICIPATION_DURATION / GetComponent<EnemyStatus>().getTimeState());

        curRushMove = RUSH_MOVE * direction * GetComponent<EnemyStatus>().getTimeState();
    }

    //Sets up anticipation for aerial shooting attack
    IEnumerator shootAnticipation() {
        GetComponent<MeshRenderer>().material = rushingMat;
        firedProjectile = true;

        yield return new WaitForSeconds(AERIAL_ANTICIPATION / GetComponent<EnemyStatus>().getTimeState());

        Instantiate(projectile, GetComponent<Transform>());
        GetComponent<MeshRenderer>().material = defaultMat;
    }

    //Checks collision
    void OnCollisionEnter(Collision collision) {
        StartCoroutine(checkGrounded());

        //If enemy hits player, recoil and hard reset
        if (collision.collider.tag == "Player") {
            if (rushing) {
                rushing = false;
                curRushMove = 0;
                aggression.hardReset();
                GetComponent<MeshRenderer>().material = defaultMat;
            }
        }
    }

    //Checks grounded and ensures it happens after collision system processing
    // ONLY USED WITH ON COLLISION ENTER and for the interval
    private IEnumerator checkGrounded() {
        yield return new WaitForSeconds(0.01f);
        if (enemyCollision.grounded && GetComponent<EnemyStatus>().unpaused())
            aggression.hardReset();
    }

    //Applies slow or reverses slow
    public void applySlow(float timeFactor) {
        if(timeFactor < 1) {
            slowed = true;
            rb.velocity *= timeFactor;
            slowFactor = timeFactor;
            if(enemyCollision.grounded)
                decayUpForce = 700f;
            else
                decayUpForce = rb.velocity.y * DECAY_START_FACTOR;

            //Change timer while avoiding overlap
            aggressionInterval /= timeFactor;
            aggression.setTime(aggression.getTime() / timeFactor);

        }else{
            slowed = false;

            //Adds a force to make it seem normal depending on which direction its going
            if (rb.velocity.y > 0)
                rb.AddForce(0, NATURAL_FALL * -1, 0);
            else
                rb.AddForce(0, NATURAL_FALL, 0);

            //Change timer while avoiding overlap
            aggression.setTime(aggression.getTime() / timeFactor);
            aggressionInterval /= timeFactor;
        }

        //Change fixed movements
        curJumpXMove *= timeFactor;
        curRushMove *= timeFactor;
    }

    //Applies pause to this object by fixing the rigidbody and unpausing by adding an endforce after
    public IEnumerator applyPause(float pauseDuration) {
        //Stored directions
        int prevJumpDir = (curJumpXMove < 0) ? -1 : 1;
        int prevRushDir = (curXTarget < enemy.position.x) ? -1 : 1;
        bool prevRushing = rushing;

        //Pausing
        if (aggression.getTime() == 0 || aggression.getTime() >= aggressionInterval)
            aggression.setTime(0.001f);
        aggression.stop();
        firedProjectile = true;

        rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
        rushing = false;
        curJumpXMove = 0;

        yield return new WaitForSeconds(0.1f);      //To ensure that the checker will happen AFTER status is paused
        yield return new WaitUntil(GetComponent<EnemyStatus>().unpaused);

        //Unpausing
        aggression.start();
        rb.constraints = ~RigidbodyConstraints.FreezePositionY & ~RigidbodyConstraints.FreezePositionX;
        curJumpXMove = JUMP_X_MOVE * prevJumpDir * GetComponent<EnemyStatus>().getTimeState();
        if(prevRushing) {
            curRushMove = RUSH_MOVE * prevRushDir * GetComponent<EnemyStatus>().getTimeState();
            rushing = true;
        }

        //Adds downward force to make it seem natural
        rb.AddForce(0f, rb.mass * NATURAL_FALL * GetComponent<EnemyStatus>().getTimeState(), 0f);
    }

}
