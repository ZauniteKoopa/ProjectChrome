﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryExecution : MonoBehaviour
{
    void OnTriggerEnter(Collider collider) {
        if (collider.tag == "Player")
            SceneManager.LoadScene("VictoryScreen");
    }
}
