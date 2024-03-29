﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour {

    public Text gameOverText;
    private PlayerStatus status;
    public GameObject player;

    // Use this for initialization
    void Start () {
        gameOverText.text = "";
        status = player.GetComponent<PlayerStatus>();
    }

    // Update is called once per frame
    void Update() {
        if (status.health <= 0)
            gameOverText.text = "Game Over\nPress the Spacebar to Restart";
    }
}
