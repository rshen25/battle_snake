using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float levelStartDelay = 4f;
    public static GameManager instance = null;

    public bool doingSetup = true;              // Boolean to check if the game is setting up the level in order to prevent the user from moving the snake

    public float turnTime = 0.35f;
    public int stage = 0;
    public float turnTimeIncrement = 0.75f;

    // private PlayerSnake playerSnake;
    private SnakeAgent aiSnake;

    private int playerScore = 0;                // The player score
    private int aiScore = 0;                    // The AI's score

    private Text playerScoreTxt;                // The player scores text
    private Text aiScoreTxt;                    // The AI scores text
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

        // Get the Player and AI snake
        // playerSnake = GameObject.Find("SnakeHead").GetComponent<PlayerSnake>();
        aiSnake = GameObject.Find("SnakeHead2").GetComponent<SnakeAgent>();

        // Get the UI elements
        playerScoreTxt = GameObject.Find("PlayerScore").GetComponent<Text>();
        aiScoreTxt = GameObject.Find("AIScore").GetComponent<Text>();
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
        playerScoreTxt.text = playerScore.ToString();

        if ((playerScore + aiScore) % 100 == 0)
        {
            IncreaseMovementSpeed();
        }
    }

    // Increments the AI score
    public void IncrementAIScore()
    {
        aiScore += 50;
        aiScoreTxt.text = aiScore.ToString();

        if ((playerScore + aiScore) % 100 == 0)
        {
            IncreaseMovementSpeed();
        }
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

    // Gets and returns the position of the current food tile on the game board
    public Vector3 GetCurrentFoodPos()
    {
        return boardScript.GetCurrentFoodPos();
    }

    // Increases the movespeed of all
    public void IncreaseMovementSpeed()
    {
        if (stage >= 6)
        {
            return;
        }
        
        stage++;
        turnTime = turnTime * turnTimeIncrement;

        // playerSnake.IncreaseMovementSpeed();
        aiSnake.IncreaseMovementSpeed(turnTime);
    }

    // Resets the scores
    public void ResetScores()
    {
        playerScore = 0;
        aiScore = 0;
        stage = 0;
        turnTime = 0.35f;
    }
}
