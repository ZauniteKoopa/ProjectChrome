using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    //Loads prototype level (next on the queue behind menu)
    public void playPrototype() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    //Quits game
    public void quitGame() {
        Application.Quit();
    }
}
