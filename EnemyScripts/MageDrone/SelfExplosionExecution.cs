using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfExplosionExecution : MonoBehaviour {

    //Time setters
    private const float ANTICIPATION_DURATION = 0.95f;
    private const float EXPLOSION_DURATION = 0.2f;
    private Stopwatch explosionTime;

    //Visual and auditory effects
    public Material explodedMat;
    public AudioSource explosionSound;

    //Damage package variables
    private const float DAMAGE = 1.0f;
    private const float KNOCKBACK = 600f;

    //Explosion variables
    public Transform explosion;

    //Reference variables
    private EnemyStatus parentStatus;

	// Use this for initialization
	void Start () {
        explosionTime = gameObject.AddComponent<Stopwatch>();
        parentStatus = GetComponentInParent<EnemyStatus>();
	}
	
	// Update is called once per frame
	void Update () {
        //Starts explosion timer
        if (explosionTime.getTime() == 0f)
            explosionTime.start();

        float curAnticipation = ANTICIPATION_DURATION / parentStatus.getTimeState();

        //If duration times out, trigger explosion
        if (explosionTime.getTime() >= curAnticipation && parentStatus.unpaused()) {
            Instantiate(explosion, GetComponentInParent<Transform>().root);
            Object.Destroy(gameObject);
        }
    }
}