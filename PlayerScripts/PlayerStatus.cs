using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStatus : MonoBehaviour, IKillable {

    //Player status variables
    private const float MAX_HEALTH = 5f;
    private const float MAX_MANA = 10f;
    private Stopwatch manaRegeneration;

    //Player stats
    public float health;
    public float mana;
    public float manaRegen; //Time it takes to generate 1 mana point

    //Damage Numbers
    public Dictionary<string, float> damageMap;
    private float clawDamage = 1.0f;

    //Play Movement
    public bool enableMovement;
    public bool enableAttack;
    public bool isFacingRight;
    private const float DEATH_DEPTH = -50f;

    //Attacked Variables
    private Rigidbody playerRB;
    private Stopwatch invincibilityFrame;
    private const float MAX_INVINCIBILITY_FRAME = 1.5f;
    private const int ENEMY_INVINCIBILITY_LAYER = 8;

    //Materials to be used
    public Material defaultMat;
    public Material recoveryMat;

    //Audio
    public AudioSource deathCry;

    //UI Interaction
    public GameObject pauseMenu;

    // Use this for initialization
    void Start () {
        //Movement Status
        enableAttack = true;
        enableMovement = true;
        isFacingRight = true;

        //Attacking variables
        playerRB = GetComponent<Rigidbody>();
        invincibilityFrame = gameObject.AddComponent<Stopwatch>();

        //Player Resources
        health = MAX_HEALTH;
        mana = MAX_MANA;
        manaRegeneration = gameObject.AddComponent<Stopwatch>();

        //Damage Numbers
        damageMap = new Dictionary<string, float>();
        damageMap.Add("Claw", clawDamage);
    }
    
    // Update is called once per frame
    void Update () {
        //Mana Regen Algorithm : Only starts mana if mana < MAX_MANA)
        if (mana < MAX_MANA)
            manaRegeneration.start();

        if(manaRegeneration.getTime() >= manaRegen) {
            if (mana < MAX_MANA)
                mana++;

            manaRegeneration.hardReset();
        }

        //Turns off Invincibility Frames after a certain amount of time
        if (invincibilityFrame.getTime() >= MAX_INVINCIBILITY_FRAME) {
            invincibilityFrame.hardReset();
            gameObject.layer = 0;
            GetComponent<MeshRenderer>().material = defaultMat;
        }

        //Allows pausing with the pause menu
        if (Input.GetKeyDown(KeyCode.Escape)){
            Time.timeScale = 0.0f;
            pauseMenu.SetActive(true);
            pauseMenu.SendMessage("pauseGame");
        }

        //Lose game algorithm
        if (GetComponent<Transform>().position.y <= DEATH_DEPTH && health > 0) {
            health = 0;
            death();
        }

        //Allows resetting when no health
        if(health <= 0 && Input.GetKey("space"))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //Accessor method to max health
    public float getMaxHealth() {
        return MAX_HEALTH;
    }

    //Accessor method to max mana
    public float getMaxMana() {
        return MAX_MANA;
    }

    //Applies damage & knockback to enemy if attacked by player
    //  Pre: dmgPackage must be in dmgPackage format (Maybe turned into a class)
    public void getDamage(DamagePackage dmgPackage){
        if(!invincibilityFrame.isRunning()) {
            //Apply damagePackage to player
            health -= dmgPackage.getDamage();
            if (health < 0)
                health = 0;

            StartCoroutine(applyKnockback(dmgPackage.getKnockback()));
            GetComponent<MeshRenderer>().material = recoveryMat;

            //If health is 0, end game, else do hitstun
            if (health <= 0.0f)
                death();
            else
                StartCoroutine(Hitstun());
        }
    }

    //Private IEnumerator for controlled knockback
    private const float KNOCKBACK_DURATION = 0.15f;

    public IEnumerator applyKnockback(Vector3 knockback) {
        playerRB.velocity = Vector3.zero;
        playerRB.AddForce(knockback);

        yield return new WaitForSeconds(KNOCKBACK_DURATION);

        playerRB.velocity = Vector3.zero;
    }

    private const float HITSTUN_DURATION = 0.75f;

    //Hitstun Function: Forces the player into hitstun when player controls are disabled for a short period of time
    IEnumerator Hitstun() {
        //allows invincibility to enemies
        gameObject.layer = ENEMY_INVINCIBILITY_LAYER;
        invincibilityFrame.start();

        //disables controls for a short period of time
        enableMovement = false;
        enableAttack = false;
        yield return new WaitForSeconds(HITSTUN_DURATION);
        enableMovement = true;
        enableAttack = true;
    }

    //Death function that plays on player death
    private void death() {
        //Disable all abilities
        enableMovement = false;
        enableAttack = false;

        //Make death cry
        deathCry.Play();
        Object.Destroy(GetComponent<Collider>());
    }

    //Method to disable all controls
    public void disableControls() {
        enableAttack = false;
        enableMovement = false;
    }

    //Method to enable all controls
    public void enableControls(){
        enableAttack = true;
        enableMovement = true;
    }
}
