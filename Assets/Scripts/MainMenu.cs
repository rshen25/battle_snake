using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Loads the game when the play button is pressed
    public void StartSnake()
    {
        SceneManager.LoadScene("Main");
    }

    // Quits the game
    public void ExitGame()
    {
        Debug.Log("Quitting.");
        Application.Quit();
    }
}
