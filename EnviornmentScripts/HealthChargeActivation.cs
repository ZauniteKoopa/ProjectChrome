using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthChargeActivation : MonoBehaviour
{
    public float healthRecovery;
    public AudioSource healSound;

    //When player triggers this, restore health and destroy object
    private void OnTriggerEnter(Collider other) {

        if(other.tag == "Player") {
            PlayerStatus player = other.GetComponent<PlayerStatus>();
            player.health += healthRecovery;

            //Checks if health is over MAX_HEALTH
            if (player.health > player.getMaxHealth())
                player.health = player.getMaxHealth();

            //Destroy object
            StartCoroutine(activated());
        }
    }

    //Algorithm for when its already activated
    private IEnumerator activated() {
        healSound.Play();
        GetComponent<MeshRenderer>().enabled = false;
        this.enabled = false;
        yield return new WaitForSeconds(0.25f);
        Object.Destroy(gameObject);
    }
}
