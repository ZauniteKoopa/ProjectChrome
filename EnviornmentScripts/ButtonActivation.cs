using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonActivation : MonoBehaviour {

    //Keeps track of enemies spawned
    public Transform[] enemySpawns;
    private LinkedList<Transform> activeEnemies;

    private bool activated;                         //Boolean checking variable for button activation
    private const float BUTTON_DURATION = 8.0f;     //Interval it takes to press the button again
    public Transform output;                        //Output object to be activated

    public AudioSource activateAudio;               //Sound played when activated

    //Intializes all data structures
    void Start() {
        activeEnemies = new LinkedList<Transform>();
    }

    //Activates button spawning when player or player attack touches button
    void OnTriggerEnter(Collider other) {
        //Checks if a player presses button to activate button
        if((other.tag == "Player" || other.tag == "PlayerAttack") && !activated) {
            activated = true;

            //Destroys all enemies still active
            foreach(Transform enemy in activeEnemies)
                if (enemy != null)
                    Object.Destroy(enemy.gameObject);

            activeEnemies.Clear();

            //Creates clone of enemy with the spawn point and puts them in activeEnemies
            foreach(Transform spawn in enemySpawns) {
                Transform enemy = Instantiate(spawn);
                activeEnemies.AddLast(enemy);
                enemy.gameObject.SetActive(true);
            }

            //Activates linked output
            if(output != null)
                output.SendMessage("activate");

            //Does coroutine for button interval
            StartCoroutine(buttonInterval());
        }
    }

    //Allows button interval duration to play out 
    IEnumerator buttonInterval() {
        GetComponent<MeshRenderer>().enabled = false;
        activateAudio.Play();
        yield return new WaitForSeconds(BUTTON_DURATION);
        GetComponent<MeshRenderer>().enabled = true;
        activated = false;
    }

}
