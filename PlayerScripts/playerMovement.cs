using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour {
    //Player & Weapons components
    private Transform player;
    private Rigidbody rb;
    private CollisionSystem playerCollision;
    private Stopwatch jumpStopwatch;
    private Stopwatch dashStopwatch;

    //Script Variables and Constants
    private const float WALKING_SPEED = 0.3f;
    private const float JUMP_X_SPEED = 0.15f;
    private const float DASH_DELAY = 0.8f;

    //Player Behavior Variables
    private float speed = JUMP_X_SPEED;
    private bool canDash;
    private PlayerStatus status;

    //Audio Sources
    public AudioSource dashSound;

    // Use this for initialization
    void Start () {
        //Return Variables obtained by the parent component
        rb = GetComponent<Rigidbody>();
        player = GetComponent<Transform>();
        playerCollision = GetComponent<CollisionSystem>();

        status = GetComponent<PlayerStatus>();
        canDash = true;

        dashStopwatch = gameObject.AddComponent<Stopwatch>();
        jumpStopwatch = GetComponent<Stopwatch>();
    }
	
	// Update is called once per frame
	void Update () {
        //Jump
        if(status.enableMovement)
            jump();

        //Dashing
        if (status.enableAttack)
            dashManager();

        //Set variable is player is grounded
        speed = (playerCollision.grounded) ? WALKING_SPEED : JUMP_X_SPEED;
	}

    //Fixed update for movement
    void FixedUpdate() {
        //Actual controls 
        if (status.enableMovement){
            //Moving right
            if (Input.GetKey("d") && !playerCollision.rightWalled){
                player.Translate(Vector3.right * speed);
                status.isFacingRight = true;
            }

            //Moving left
            if (Input.GetKey("a") && !playerCollision.leftWalled){
                player.Translate(Vector3.left * speed);
                status.isFacingRight = false;
            }
        }

        //Allows less floaty jumping by slaming player object when player slows at a certain point
        if (Mathf.Abs(rb.velocity.y) < 15 && !playerCollision.grounded)
            rb.AddForce(0, -70, 0);
    }

    //Constant for jumping
    private const float ADD_JUMP = 250f;
    private const float BASE_JUMP = 1250f;
    private const float MAX_JUMP_DELAY = 0.12f;

    //Method that allows the player to either tapJump or fullJump (looked at another time)
    public void jump() {

        //Starts jump stopwatch and adds initial base jump
        if (Input.GetKeyDown("space") && playerCollision.grounded) {
            rb.velocity = Vector3.zero;
            rb.AddForce(Vector3.up * BASE_JUMP);
            jumpStopwatch.start();
        }

        //When hiting an enemy or timer times out, reset stopwatch
        if(jumpStopwatch.getTime() > MAX_JUMP_DELAY) {
            //If still holding spacebar after all this time, add force for full jump
            if (Input.GetKey("space") && rb.velocity.y > 0)
                rb.AddForce(Vector3.up * ADD_JUMP);
                
            jumpStopwatch.hardReset();
        }
    }

    //Method that manages the canDash variable and allows dashing
    public void dashManager() {
        //Allows delay
        if (!canDash && dashStopwatch.getTime() >= DASH_DELAY) {
            canDash = true;
            dashStopwatch.hardReset();
        }

        //Dashing
        if (Input.GetKey(KeyCode.LeftShift) && canDash) {
            if (status.isFacingRight && canDash)
                StartCoroutine(Dash(Vector3.right));
            else
                StartCoroutine(Dash(Vector3.left));

            canDash = false;
            dashStopwatch.start();
        }
    }

    //Constants for dashing
    private const float DASH_SPEED = 0.55f;
    private const int MAX_FRAME = 10;
    private const float END_FORCE = 100f;
    private const float RECALIBRATING_FORCE = 300f;

    //Dash Function: Allows Player to Dash
    //Pre: Direction either Vector3.left or Vector3.right
    IEnumerator Dash(Vector3 direction) {
        //Set up the variables for frame checking and enable dashing
        int curFrame = 0;
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        bool hitWall = (playerCollision.leftWalled && !status.isFacingRight) || (playerCollision.rightWalled && status.isFacingRight);
        status.disableControls();
        dashSound.Play();

        while (curFrame < MAX_FRAME && !hitWall && !playerCollision.hitEnemy) {    //Looping through each frame
            player.Translate(direction * DASH_SPEED);
            hitWall = (playerCollision.leftWalled && !status.isFacingRight) || (playerCollision.rightWalled && status.isFacingRight);
            yield return new WaitForFixedUpdate();
            curFrame++;
        }

        rb.constraints = ~RigidbodyConstraints.FreezePositionY & ~RigidbodyConstraints.FreezePositionX;

        //If not in hitstun, immediately enable controls
        if(!playerCollision.hitEnemy)
            status.enableControls();

        player.position = new Vector3(player.position.x, player.position.y, -9.75f);
    }


    //Sets variables if player is in the air
    public void inTheAir() {
        playerCollision.grounded = false;
        speed = JUMP_X_SPEED;
    }
}
