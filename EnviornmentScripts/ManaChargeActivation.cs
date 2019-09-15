using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaChargeActivation : MonoBehaviour
{
    public float manaRecovery;
    public AudioSource manaRestoreSound;

    //When player triggers this, restore health and destroy object
    private void OnTriggerEnter(Collider other){

        if (other.tag == "Player"){
            PlayerStatus player = other.GetComponent<PlayerStatus>();
            player.mana += manaRecovery;

            //Checks if health is over MAX_HEALTH
            if (player.mana > player.getMaxMana())
                player.mana = player.getMaxMana();

            //Destroy object
            StartCoroutine(activated());
        }
    }

    //Algorithm for when its already activated
    private IEnumerator activated(){
        manaRestoreSound.Play();
        GetComponent<MeshRenderer>().enabled = false;
        this.enabled = false;
        yield return new WaitForSeconds(0.25f);
        Object.Destroy(gameObject);
    }
}
