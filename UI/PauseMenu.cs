using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour{

    //Called when resume is called
    public void resumeGame() {
        Time.timeScale = 1.0f;
    }

    //Called when exiting game to go back to Main Menu
    public void quitGame() {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(0);
    }

    //Called to allow leaving menu through button shortcut
    private IEnumerator pauseGame() {
        yield return null;

        while (!Input.GetKeyDown(KeyCode.Escape))
            yield return null;

        Time.timeScale = 1.0f;
        gameObject.SetActive(false);
    }

}
