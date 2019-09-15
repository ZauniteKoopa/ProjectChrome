using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionDamage : MonoBehaviour {

    //Explosion duration
    private Stopwatch explosion;
    private const float EXPLOSION_DURATION = 0.15f;
    private float curDuration;

    //Damage Package
    private const float DAMAGE_VAL = 1.0f;
    private const float KNOCKBACK = 600f;
    private DamagePackage dmgPackage;

	// Use this for initialization
	void Start () {
        dmgPackage = new DamagePackage(DAMAGE_VAL);
        explosion = gameObject.AddComponent<Stopwatch>();
        curDuration = EXPLOSION_DURATION;
	}

    //Check time
    void Update() {
        explosion.start();

        if (explosion.getTime() >= curDuration)
            Object.Destroy(gameObject);
    }

    //Deal damage
    void OnTriggerEnter(Collider other) {
        if(other.tag == "Player") {
            dmgPackage.setCentralizedKnockback(GetComponent<Transform>().position, other.transform.position, KNOCKBACK);
            other.SendMessage("getDamage", dmgPackage);
        }
    }

    //Applies slow
    public void applySlow(float timeFactor) {
        if(timeFactor < 1){
            curDuration /= timeFactor;
            explosion.setTime(explosion.getTime() / timeFactor);
        }else{
            explosion.setTime(explosion.getTime() / timeFactor);
            curDuration /= timeFactor;
        }
    }
}
