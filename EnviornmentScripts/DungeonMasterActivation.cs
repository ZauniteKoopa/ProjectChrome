using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonMasterActivation : MonoBehaviour {

    public Transform[] lockedDoors; //Public array that's linked to doors of the dungeon
    private bool activated;         //status variables that tells that if dungeon is activated
    public AudioSource lockSound;   //Sound effect to shut doors

    //Checks if player activates the dungeon to lock all doors
    void OnTriggerEnter(Collider other) {
        if(other.tag == "Player" && !activated) {
            //Set activated to true so to not access this again
            activated = true;

            //loop through all lockedDoors, set them to active and link
            lockSound.Play();

            foreach(Transform door in lockedDoors) {
                door.gameObject.SetActive(true);
                door.GetComponent<CentralNode>().addLink(GetComponent<Transform>());
            }
        }
    }

    //If all links to this are broken, destroy this gameObject
    private void linksBroken() {
        //Break links to doors to isolate them
        foreach (Transform door in lockedDoors)
            door.GetComponent<CentralNode>().breakLink(GetComponent<Transform>());

        Object.Destroy(gameObject);
    }
}
