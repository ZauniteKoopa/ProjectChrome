using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowSphereBehavior : MonoBehaviour {
    //Slow variables
    private const float MAX_SLOW_DURATION = 4.0f;
    private const float SLOW_FACTOR = 0.35f;
    private const float SHRINKING_DURATION = 0.25f;
    private const float SHRINKING_FACTOR = 1f;

    //Shockwave variables
    private const int SPHERE_MAX_HEALTH = 3;
    private const float MAX_RECOVERY_TIME = 0.5f;
    private int sphereHealth;
    private Stopwatch recovery;
    private bool vulnerable;

    private const float shockwaveDamage = 2.0f;
    private const float shockwaveKnockback = 150f;
    private bool dying;

    //Reference variables
    //private Transform slowSphere; //For shrinking purposes
    private Stopwatch slowDuration;
    private Transform player;

    //List with objects affected by this sphere
    private HashSet<Collider> effected;

    //Visual / Audio Indicators for player
    public AudioSource staticCrack;
    public AudioSource shockwaveBlast;
    public Material defaultSphere;
    public Material criticalAlter;
    public Material criticalHealth;
    public Material alteredSphere;
    public Material explosion;

	// Use this for initialization
	void Start () {
        //Create stopwatch and set it up
        //slowSphere = GetComponent<Transform>();
        slowDuration = gameObject.AddComponent<Stopwatch>();
        recovery = gameObject.AddComponent<Stopwatch>();
        effected = new HashSet<Collider>();
        sphereHealth = SPHERE_MAX_HEALTH;

        vulnerable = false;
        dying = false;

        //Keep a reference to parent and then detach
        player = transform.parent;
        transform.parent = null;
    }
	
	// Update is called once per frame
	void Update () {
        slowDuration.start();

        //Only starts recovery timer if not vulnerable
        if (!vulnerable)
            recovery.start();

        //When recovery is finished, return to normal material or critical health material
        if(!vulnerable && recovery.getTime() >= MAX_RECOVERY_TIME){
            recovery.hardReset();
            vulnerable = true;
            staticCrack.Stop();
            GetComponent<MeshRenderer>().material = (sphereHealth == 1) ? criticalHealth : defaultSphere;
        }

        //Allows shrinking of sphere at the very end
        //if (slowDuration.getTime() >= MAX_SLOW_DURATION - SHRINKING_DURATION && !dying)
            //slowSphere.localScale -= (Vector3.right + Vector3.up) * SHRINKING_FACTOR;

        //Destroys sphere and reverses all those still effected
        if (slowDuration.getTime() >= MAX_SLOW_DURATION && !dying) {
            //Reverses effects of enemies still in sphere
            foreach(Collider obj in effected)
                if(obj != null && obj.tag != "Player")
                    obj.SendMessage("applySlow", 1 / SLOW_FACTOR);

            player.SendMessage("enableSlow");
            Object.Destroy(gameObject);
        }
	}

    //Entering field, apply time effects and decrement time zone health if needed
    void OnTriggerEnter(Collider other) {

        //If collider is a player or enemy attack, decrement health
        if ((other.tag == "PlayerAttack" || other.tag == "EnemyAttack" || other.tag == "Enemy" || other.tag == "Explosion") && vulnerable && !effected.Contains(other)){
            //Decrement Sphere Health
            sphereHealth--;

            //Check if sphere health is 0
            if (sphereHealth <= 0){
                //Triggers shockwave
                triggerShockwave(other);

                //Destroys object
                Object.Destroy(GetComponent<SphereCollider>());
                StartCoroutine(selfDestruct());
            }else{
                //Plays static crack sound with volumn based on remaining health compared to MAX_HEALTH
                staticCrack.volume = (float)(SPHERE_MAX_HEALTH - sphereHealth) / SPHERE_MAX_HEALTH;
                staticCrack.Play();

                //Change material
                GetComponent<MeshRenderer>().material = (sphereHealth == 1) ? criticalAlter : alteredSphere;

                //Make sphere not vulnerable
                vulnerable = false;
            }
        }

        bool canBeEffected = (other.tag == "Enemy" || other.tag == "Player" || other.tag == "EnemyAttack" || other.tag == "PausedProjectile" || other.tag == "Explosion");

        //If collider is an enemy or player, put them in affected list for shockwave damage
        if (canBeEffected && !effected.Contains(other) && sphereHealth > 0) {
            effected.Add(other);

            //If collider is enemy, broadcast time effect message
            if (other.tag != "Player")
                other.SendMessage("applySlow", SLOW_FACTOR);
        }
    }

    //Exiting field / sphere, reverse time effects
    void OnTriggerExit(Collider other) {
        //If collider is an enemy or player, put them in affected list for shockwave damage
        if (other.tag == "Enemy" || other.tag == "Player" || other.tag == "EnemyAttack") {
            effected.Remove(other);

            //If collider is enemy, broadcast time effect message
            if (other.tag == "Enemy" || other.tag == "EnemyAttack")
                other.SendMessage("applySlow", 1 / SLOW_FACTOR);
        }
    }

    //Triggers shockwave damage to elements in affected
    //  Pre: targetTag must be either "Player" or "Enemy"
    private void triggerShockwave(Collider detonator){
        //Creates new damage package to send 
        DamagePackage dmgPackage = new DamagePackage(shockwaveDamage); 

        //Establishes DSource
        Transform dSource = (detonator.tag == "EnemyAttack" && detonator.GetComponent<ProjectileStatus>().source != null) ? detonator.GetComponent<ProjectileStatus>().source : detonator.transform.root;

        //Sends knockback depending on each suspect and sends damagePackage if suspect is a target
        foreach (Collider suspect in effected)
            if(suspect != null) {               //Checks if object hasn't been deleted
                //Reverses all time effects
                if (suspect.tag == "Enemy" || suspect.tag == "EnemyAttack" || suspect.tag == "PausedProjectile")
                    suspect.SendMessage("applySlow", 1 / SLOW_FACTOR);

                //Applies damage to everyone in the field except the one who detonated it. Applies to players and enemies. Not attacks
                if (suspect.transform != dSource && (suspect.tag == "Player" || suspect.tag == "Enemy")) {
                    dmgPackage.setCentralizedKnockback(detonator.transform.position, suspect.transform.position, shockwaveKnockback);
                    suspect.SendMessage("getDamage", dmgPackage);
                }
            }
    }

    //Co-routine for slow zone self destruction
    //  It will play an audio file and change materials before destroying itself
    IEnumerator selfDestruct() {
        dying = true;
        shockwaveBlast.Play();
        GetComponent<MeshRenderer>().material = explosion;
        Time.timeScale = 0.25f;

        yield return new WaitForSeconds(0.14f);

        shockwaveBlast.Stop();
        Time.timeScale = 1.0f;
        player.SendMessage("enableSlow");
        Object.Destroy(gameObject);
    }
}
