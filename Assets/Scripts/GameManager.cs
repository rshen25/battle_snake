using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public const float levelStartDelay = 4f;    // The time to wait before the game starts
    public static GameManager instance = null;  // The game manager instance

    public bool doingSetup = true;              // Boolean to check if the game is setting up the level in order to prevent the user from moving the snake

    public float turnTime = 0.35f;                  // The time it takes before the next movement step is taken
    public int stage = 0;                           // The stage determines the speed at which the snakes move, it is multiplied by the turnTimeIncrement
    public const float turnTimeIncrement = 0.75f;   // How much the speed is incremented when the next stage is reached

    public GameObject playerSnake;              // The Player snake head
    public GameObject enemySnake;               // The Enemy snake head

    private PlayerSnake playerSnakeScript;      // The script for the player snake
    private EnemySnake enemySnakeScript;        // The script that controls the AI snake

    private int playerScore = 0;                // The player score
    private int aiScore = 0;                    // The AI's score

    private Text playerScoreTxt;                // The player scores text
    private Text aiScoreTxt;                    // The AI scores text
    private Text gameOverTxt;                   // The text to show the lose message at the center of the screen
    private Text endScoreTxt;                   // The text to show the scores of the player and AI at the end of the game

    private Button btnPlayAgain;                // Reference to the play again button shown at end of game
    private Button btnExit;                     // Reference to the exit game button

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

        InitGame();

        // Get the Player and AI snake
        playerSnakeScript = playerSnake.GetComponent<PlayerSnake>();
        enemySnakeScript = enemySnake.GetComponent<EnemySnake>();

        // Get the UI elements
        playerScoreTxt = GameObject.Find("PlayerScore").GetComponent<Text>();
        aiScoreTxt = GameObject.Find("AIScore").GetComponent<Text>();
        gameOverTxt = GameObject.Find("GameOverText").GetComponent<Text>();
        endScoreTxt = GameObject.Find("EndScoreText").GetComponent<Text>();

        btnPlayAgain = GameObject.Find("Button_PlayAgain").GetComponent<Button>();
        btnExit = GameObject.Find("Button_Exit").GetComponent<Button>();

        // Hides the end game screen while the game is in progress
        instance.HideEndGameScreen();
    }

    // Initializes the game
    void InitGame()
    {
        doingSetup = true;

        // Set up the game board
        boardScript.SetupScene();

        doingSetup = false;
    }

    // Called when the player wins/loses
    public void GameOver()
    {
        // Show the scores and game over message
        ShowGameOverMessage();

        // Stops the player and enemy snake from moving
        playerSnakeScript.CancelInvoke("MoveSnake");
        enemySnakeScript.CancelInvoke("MoveSnake");

    }

    // Increments the player score and updates the UI
    public void IncrementPlayerScore()
    {
        playerScore += 50;
        playerScoreTxt.text = "Your Score: " + playerScore.ToString();

        if ((playerScore + aiScore) % 100 == 0)
        {
            IncreaseMovementSpeed();
        }
    }

    // Increments the AI score
    public void IncrementAIScore()
    {
        aiScore += 50;
        aiScoreTxt.text = "AI Score: " + aiScore.ToString();

        if ((playerScore + aiScore) % 100 == 0)
        {
            IncreaseMovementSpeed();
        }
    }

    // Calls the board script to respawn another food tile onto the board
    public void RespawnFood()
    {
        boardScript.RespawnFood();
        enemySnakeScript.SetFoodPos(GetCurrentFoodPos());
    }

    // Displays the lose message
    public void ShowGameOverMessage()
    {
        gameOverTxt.gameObject.SetActive(true);
        btnExit.gameObject.SetActive(true);
        btnPlayAgain.gameObject.SetActive(true);
        endScoreTxt.text = $"Your Score:     {playerScore}\nAI Score:         {aiScore}";
        endScoreTxt.gameObject.SetActive(true);
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
        // If the stage is 6 or greater, then we have reached the max speed and do not increase
        if (stage >= 6)
        {
            return;
        }

        // Increment the stage 
        stage++;
        turnTime *= turnTimeIncrement;

        playerSnakeScript.IncreaseMovementSpeed();
        enemySnakeScript.IncreaseMovementSpeed(turnTime);
    }

    // Resets the scores
    public void ResetScores()
    {
        playerScore = 0;
        aiScore = 0;
        stage = 0;
        turnTime = 0.35f;

        playerScoreTxt.text = "Your Score: " + playerScore.ToString();
        aiScoreTxt.text = "AI Score: " + aiScore.ToString();
    }

    // Resets the game board, UI and player positions to the initial start phase
    public void Reset()
    {
        instance.ResetScores();
        Debug.Log("button pressed");

        // Hide end game screen
        instance.HideEndGameScreen();

        // Reset Game
        boardScript.ResetBoard();
        playerSnakeScript.ResetSnake();
        enemySnakeScript.ResetSnake();
    }

    // Returns the amount of rows in the board
    public int GetRows()
    {
        return boardScript.GetRows();
    }

    // Returns the amount of columns in the game board
    public int GetColumns()
    {
        return boardScript.GetColumns();
    }

    // Hides the end game UI elements
    private void HideEndGameScreen()
    {
        gameOverTxt.gameObject.SetActive(false);
        endScoreTxt.gameObject.SetActive(false);
        btnPlayAgain.gameObject.SetActive(false);
        btnExit.gameObject.SetActive(false);
    }
}
