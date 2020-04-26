using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float levelStartDelay = 4f;
    public static GameManager instance = null;

    private int playerScore = 0;                // The player score

    public bool doingSetup = true;              // Boolean to check if the game is setting up the level in order to prevent the user from moving the snake


    private Text playerScoreTxt;                // The player scores text
    private Text loseTxt;                       // The text to show the lose message at the center of the screen
    private Text winTxt;                        // The text to show the win message 
    private BoardManager boardScript;

    // Start is called before the first frame update
    void Awake()
    {
        // Check if there are multiple instances of the game manager
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
        boardScript = GetComponent<BoardManager>();

        playerScoreTxt = GameObject.Find("PlayerScore").GetComponent<Text>();
        loseTxt = GameObject.Find("LoseText").GetComponent<Text>();
        winTxt = GameObject.Find("WinText").GetComponent<Text>();

        loseTxt.gameObject.SetActive(false);
        winTxt.gameObject.SetActive(false);

        InitGame();
    }

    void InitGame()
    {
        doingSetup = true;

        // Set up the game
        boardScript.SetupScene();

        doingSetup = false;
    }

    // Called when the player wins/loses
    public void GameOver()
    {

    }

    // Increments the player score and updates the UI
    public void IncrementPlayerScore()
    {
        playerScore += 50;

        playerScoreTxt.text = "Score: " + playerScore;

    }

    // Calls the board script to respawn another food tile onto the board
    public void RespawnFood()
    {
        boardScript.RespawnFood();
    }

    // Displays the lose message
    public void ShowLoseMessage()
    {
        loseTxt.gameObject.SetActive(true);
    }

    // Displays the win message
    public void ShowWinMessage()
    {
        winTxt.gameObject.SetActive(true);
    }

    // Gets the player score
    public int GetPlayerScore()
    {
        return playerScore;
    }
}
