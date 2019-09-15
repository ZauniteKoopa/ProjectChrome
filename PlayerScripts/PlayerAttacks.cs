using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttacks : MonoBehaviour {

    //Attack Management Variables
    public Transform player;
    private Stopwatch attackStopwatch;
    public bool canAttack;

    //Claw Properties
    public Transform clawHitbox;
    private Vector3[,] CLAW_DIMENSIONS;

    //Claw Constants
    private const float ATTACK_DELAY = 0.5f;
    private const float CLAW_WIDTH = 2.2f;
    private float clawLength = 4.25f;

    //Slow Sphere Properties
    public Transform slowSphere;
    public Transform slowInit;
    public Material groundedSlowSlam;
    private bool canSlow;
    private const float SLOW_INIT_JUMP = 650f;
    private const float SLOW_MANA_COST = 5;
    private const float SLOW_COOLDOWN = 0.5f;

    //Pause roar manager
    public Transform pauseShock;
    private Stopwatch pauseStopwatch;
    private bool canPause;
    private const float PAUSE_COOLDOWN = 0.5f;
    private const float PAUSE_MANA_COST = 3;

    //References to other components
    private PlayerStatus status;
    private CollisionSystem cs;

    // Use this for initialization
    void Start () {
        canAttack = true;
        canSlow = true;
        canPause = true;

        //Stopwatches
        attackStopwatch = gameObject.AddComponent<Stopwatch>();
        pauseStopwatch = gameObject.AddComponent<Stopwatch>();

        //Obtain important reference variables
        player = GetComponent<Transform>();
        status = GetComponent<PlayerStatus>();
        cs = GetComponent<CollisionSystem>();

        //Claw Dimensions Array to be used when changing claw dimensions
        CLAW_DIMENSIONS = new Vector3[4, 3] { {new Vector3(0,1), new Vector3(0, 0, 90), new Vector3(CLAW_WIDTH / player.localScale.x, clawLength / player.localScale.y, 1)},
                                              {new Vector3(0, -1), new Vector3(0, 0, -90), new Vector3(CLAW_WIDTH / player.localScale.x, clawLength / player.localScale.y, 1)},
                                              {new Vector3(1.5f, 0), new Vector3(0, 0, 0), new Vector3(clawLength / player.localScale.x, CLAW_WIDTH / player.localScale.y, 1)},
                                              {new Vector3(-1.5f, 0), new Vector3(0, 0, 0), new Vector3(clawLength / player.localScale.x, CLAW_WIDTH / player.localScale.y, 1)}};
    }

    // Update is called once per frame
    void Update() {
        if(status.enableAttack) {
            clawAttackManager();
            slowSphereManager();
            pauseShockManager();
        }
    }

    //Row ID for claw manager
    private const int UP_SLASH = 0;
    private const int DOWN_SLASH = 1;
    private const int RIGHT_SLASH = 2;
    private const int LEFT_SLASH = 3;

    //Manages claw attacks: allows attack animation / frame and starts timer for attack delay
    private void clawAttackManager()
    {
        //Allows Delay
        if (attackStopwatch.getTime() >= ATTACK_DELAY && !Input.GetKey("p")) {
            canAttack = true;
            attackStopwatch.hardReset();
        }

        //Attacking
        if (Input.GetKey("i") && canAttack) {
            //Changes positioning of slash depending on conditions
            if (Input.GetKey("w")){
                changeClawDimensions(UP_SLASH);

            }else if (Input.GetKey("s") && !cs.grounded){
                changeClawDimensions(DOWN_SLASH);

            }else if (status.isFacingRight){
                changeClawDimensions(RIGHT_SLASH);

            }else{
                changeClawDimensions(LEFT_SLASH);

            }

            Instantiate(clawHitbox, player);
            canAttack = false;
            attackStopwatch.start();
        }
    }

    //Uses the Claw Dimension Array to change the position and direction of hitbox
    //  Pre: 0 <= clawRowID < 4
    private void changeClawDimensions(int clawRowID) {
        clawHitbox.position = CLAW_DIMENSIONS[clawRowID, 0];
        clawHitbox.eulerAngles = CLAW_DIMENSIONS[clawRowID, 1];
        clawHitbox.localScale = CLAW_DIMENSIONS[clawRowID, 2];
    }

    //Managers slow sphere attacks
    private void slowSphereManager() {
        //Creates Slow Sphere
        if (Input.GetKey("o") && canSlow && status.mana >= SLOW_MANA_COST) {
            status.mana -= SLOW_MANA_COST;

            //If in the air, do small jump and tail flip. Else, put slow zone on ground
            if (GetComponent<CollisionSystem>().grounded){
                StartCoroutine(slowAnim());
            }else{
                GetComponent<Rigidbody>().velocity = Vector3.zero;
                GetComponent<Rigidbody>().AddForce(Vector3.up * SLOW_INIT_JUMP);
                status.enableAttack = false;
                Instantiate(slowInit, player);
            }

            canSlow = false;
        }
    }

    //Method to disable controls temporarily for slow zone animation
    private const float SLOW_SLAM_DURATION = 0.45f;

    private IEnumerator slowAnim() {
        status.disableControls();
        GetComponent<MeshRenderer>().material = groundedSlowSlam;
        Instantiate(slowSphere, player);

        yield return new WaitForSeconds(SLOW_SLAM_DURATION);

        GetComponent<MeshRenderer>().material = status.defaultMat;
        status.enableControls();
    }

    //Enables slow after initial slow zone is destroyed after a short delay
    private IEnumerator enableSlow() {
        yield return new WaitForSeconds(SLOW_COOLDOWN);
        canSlow = true;
    }

    //Manage pause shock attacks
    private void pauseShockManager() {
        //Allows Cooldown
        if (pauseStopwatch.getTime() >= PAUSE_COOLDOWN && !Input.GetKey("p")) {
            canPause = true;
            pauseStopwatch.hardReset();
        }

        //Creates pause shock
        if (Input.GetKey("p") && canPause && status.mana >= PAUSE_MANA_COST) {
            status.mana -= PAUSE_MANA_COST;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            Instantiate(pauseShock, player);
            canPause = false;
            pauseStopwatch.start();
        }
    }
}


