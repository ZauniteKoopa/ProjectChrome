using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour {

    private PlayerStatus status;
    public GameObject player;
    public Text healthText;

    // Use this for initialization
    void Start () {
        status = player.GetComponent<PlayerStatus>();
        healthText.text = "Health: " + status.health;
    }
    
    // Update is called once per frame
    void Update () {
        healthText.text = "Health: " + status.health;
    }
}
