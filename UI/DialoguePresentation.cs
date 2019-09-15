using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialoguePresentation : MonoBehaviour
{
    public string dialogue;             //Text to show in dialoguebox
    public GameObject dialogueBox;      //Reference to text box  
    private bool playerAccess;          //Bool to keep track of whether player is standing in front of sign

    //Checks each frame whether player wants to read or not
    void Update() {
        if (Input.GetKeyDown("w") && playerAccess)
            StartCoroutine(readSign());
    }

    //Checks if player is within trigger range
    void OnTriggerEnter(Collider other){
        if (other.tag == "Player")
            playerAccess = true;
    }

    //Checks if player left trigger range
    void OnTriggerExit(Collider other) {
        if (other.tag == "Player")
            playerAccess = false;
    }


    //Presents the writing on the sign until player presses a key
    private IEnumerator readSign() {

        Text setText = dialogueBox.GetComponentInChildren<Text>();
        setText.text = dialogue;
        dialogueBox.SetActive(true);
        yield return null;
        Time.timeScale = 0.0f;

        while (!Input.anyKeyDown)
            yield return null;
            
        dialogueBox.SetActive(false);
        Time.timeScale = 1.0f;

    }
}
