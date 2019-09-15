using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatus : MonoBehaviour, ITimeEffected, IKillable {

    //Health
    public float MAX_HEALTH;
    public float health;
    private const float DEATH_DEPTH = -50f;

    //Reference variables
    private Rigidbody thisRB;
    private Transform thisBody;

    //Damage Map
    public Dictionary<string, float> damageMap;
    private float bodyDamage = 1.0f;

    //Boolean to check time conditions
    private float curTimeState;
    private bool paused;
    private bool hitEnemy; //Boolean value to check when to disrupt pause

    //Materials
    public Material pauseMat;
    public Material defaultMat;

    //Graph Creation
    public Transform[] linkedUnits;

    // Use this for initialization
    void Start () {
        health = MAX_HEALTH;
        thisRB = GetComponent<Rigidbody>();
        thisBody = GetComponent<Transform>();

        //Time conditions
        curTimeState = 1.0f;

        //Damage Map
        damageMap = new Dictionary<string, float>();
        damageMap.Add("Body", bodyDamage);

        //Establish any links if needed
        foreach (Transform links in linkedUnits)
            links.GetComponent<CentralNode>().addLink(thisBody);
    }

    //Called every frame
    void Update() {
        //Checks if reaches death depth. If so, destroy object
        if (thisBody.position.y < DEATH_DEPTH)
            death();
    }

    //Applies damage & knockback to enemy if attacked by player
    //  Pre: dmgPackage must be in dmgPackage format (Maybe turned into a class)
    public void getDamage(DamagePackage dmgPackage) {
        hitEnemy = true;
        float dmgTaken = dmgPackage.getDamage();
        Vector3 knockback = dmgPackage.getKnockback() * getTimeState();

        health -= dmgTaken;

        //Sets velocity to 0 if knockback.y > 0
        if(knockback.y > 0)
            thisRB.velocity = Vector3.zero;

        thisRB.AddForce(knockback);

        if (health <= 0.0f)
            death();
    }

    //Method that destroys object upon death. If linked, will alert linked unit
    private void death() {
        //Breaks any links associated with any centralnodes
        foreach (Transform link in linkedUnits)
            if (link != null)
                link.GetComponent<CentralNode>().breakLink(thisBody);

        Object.Destroy(gameObject);
    }

    //Constants in term of knockback
    private const float KNOCKBACK = 750f;
    private const float SELF_KNOCKBACK = 250f;

    //Collision Enter checker: checks if collided with player
    void OnCollisionEnter(Collision collision) {
        Collider collider = collision.collider;

        //Checks if hit a player or another enemy
        if(collider.tag == "Player") {
            hitEnemy = true;

            //Set variables up to calculate knockback direction
            Vector3 victimPos = collider.bounds.center;
            Vector3 thisPos = thisBody.position;

            //Create self-recoil package to self
            DamagePackage dmgPackage = new DamagePackage(0f);
            dmgPackage.setCentralizedKnockback(victimPos, thisPos, SELF_KNOCKBACK);
            getDamage(dmgPackage);      //Gives recoil

            //Create damage package to send to player
            dmgPackage.setCentralizedKnockback(thisPos, victimPos, KNOCKBACK);
            dmgPackage.setDamage(bodyDamage);

            //Send damagePackage
            collider.gameObject.SendMessage("getDamage", dmgPackage);
        }
    }

    //Applies slow to status
    public void applySlow(float timeFactor) {
        curTimeState *= timeFactor;
    }

    private const int PAUSE_FRAMES = 20;

    //Applies pause to status
    public IEnumerator applyPause(float pauseDuration) {
        //Set up
        Material prevMat = GetComponent<MeshRenderer>().material;
        hitEnemy = false;
        paused = true;
        float frameDuration = pauseDuration / PAUSE_FRAMES;
        int curFrame = 0;

        //Pausing
        GetComponent<MeshRenderer>().material = pauseMat;

        //Will keep pausing until the end of the duration or if health decrements
        while (curFrame < PAUSE_FRAMES && !hitEnemy) {
            yield return new WaitForSeconds(frameDuration);

            //When reaching the last third of the frames, begin switching between materials
            if (curFrame >= (PAUSE_FRAMES * 2) / 3)
                GetComponent<MeshRenderer>().material = (curFrame % 2 == 0) ? prevMat : pauseMat;
            curFrame++;
        }

        //Unpausing
        paused = false;
        hitEnemy = false;
        GetComponent<MeshRenderer>().material = prevMat;
    }

    //Accessor method for curTimeState
    public float getTimeState() {
        return curTimeState;
    }

    //Accessor Method for paused
    public bool unpaused() {
        return !paused;
    }
}
