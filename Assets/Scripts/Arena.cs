using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class Arena : MonoBehaviour
{
    public int columns = 21;              // The amount of columns in the game board
    public int rows = 21;                 // The amount of rows in the game board

    public GameObject[] floorTiles;             // An array to hold the types of floor gameObjects tiles that comprises the floor of the game board
    public GameObject[] wallTiles;              // An array to hold the wall gameObjects tiles comprising the walls of the game board
    public GameObject foodTile;                 // The gameObject that will be used as the food

    private GameObject currentFoodTile;         // The food tile that will be spawned

    private List<Vector3> gridPositions;        // Stores the possible positions on the game board

    public const float levelStartDelay = 4f;    // The amount of time the game will wait before it will start

    public bool doingSetup = true;              // Boolean to check if the game is setting up the level in order to prevent the user from moving the snake

    public float turnTime = 0.35f;              // The base time to wait between each movement step
    public int stage = 0;                       // The stage at which the player is at, it is dependent on the score, the more food is eaten the higher the stage. Max stage is 6
    public float turnTimeIncrement = 0.75f;     // Multiplied by the stage, and is used to reduce the base turn time to increase the movement speed per stage

    public SnakeAgent snakeAgent;               // The snake agent in the arena

    public Text agentScore;                     // Used for training the AI, shows the training score of the AI

    private int aiScore = 0;                    // The AI's score

    // Start is called before the first frame update
    void Awake()
    {
        InitGame();
    }

    // Sets up the game board
    void InitGame()
    {
        doingSetup = true;

        gridPositions = new List<Vector3>();

        // Set up the game
        SetupScene();

        doingSetup = false;
    }

    // Calls the board script to respawn another food tile onto the board
    public void RespawnFood()
    {
        // Get a random position on the board
        Vector3 foodPosition = ChooseRandomPosition();

        int tries = columns * rows;

        // Check if the space is free, loop until a space is found
        while (!CheckIfFreeSpace(foodPosition) || tries == 0)
        {
            foodPosition = ChooseRandomPosition();
            tries--;
        }

        if (tries <= 0)
        {
            Debug.Log(this.name + " : Ran out of tries.");
        }

        currentFoodTile = Instantiate(foodTile, foodPosition, Quaternion.identity);
        currentFoodTile.transform.SetParent(gameObject.transform);
    }

    // Increases the movespeed of all
    public void IncreaseMovementSpeed()
    {
        if (stage >= 6)
        {
            return;
        }

        stage++;
        turnTime *= turnTimeIncrement;

        snakeAgent.IncreaseMovementSpeed(turnTime);
    }

    // Increments the AI score
    public void IncrementAIScore()
    {
        aiScore += 50;

        if ((aiScore) % 100 == 0)
        {
            IncreaseMovementSpeed();
        }
    }

    // Resets the scores
    public void ResetScores()
    {
        aiScore = 0;
        stage = 0;
        turnTime = 0.35f;
    }

     // Sets up the level
    public void SetupScene()
    {
        InitializeGrid();
        BoardSetup();

        // Place food at the center of the game board
        currentFoodTile = Instantiate(foodTile, transform.position + new Vector3((int)(columns / 2), (int)(rows / 2), 0f), Quaternion.identity);

        currentFoodTile.transform.SetParent(gameObject.transform);
    }

    // Initializes the grid position list
    private void InitializeGrid()
    {
        gridPositions.Clear();

        for (int x = 1; x < columns - 1; x++)
        {
            for (int y = 1; y < rows - 1; y++)
            {
                gridPositions.Add(new Vector3 (transform.position.x + x, transform.position.y + y, 0f));
            }
        }
    }

    // Sets up the floor and wall tiles of the game board
    private void BoardSetup()
    {
        // Create the board
        for (int x = -1; x < columns + 1; x++)
        {
            for (int y = -1; y < rows + 1; y++)
            {
                GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];

                if (x == -1 || y == -1 || x == rows || y == columns)
                {
                    toInstantiate = wallTiles[Random.Range(0, wallTiles.Length)];
                }

                GameObject instance = Instantiate(toInstantiate, transform.position + new Vector3(x, y, 0f), Quaternion.identity) as GameObject;

                instance.transform.SetParent(gameObject.transform);
            }
        }
    }

    // Chooses a random position within the game board
    public Vector3 ChooseRandomPosition()
    {
        int randomIndex = Random.Range(0, gridPositions.Count);
        return gridPositions[randomIndex];
    }

    // Returns the position of the current food
    public Vector3 GetCurrentFoodPos()
    {
        return currentFoodTile.transform.position;
    }

    // Checks if a space is free to spawn a food tile
    private bool CheckIfFreeSpace(Vector3 position)
    {
        List<Collider2D> colliders = new List<Collider2D>();

        Collider2D results = Physics2D.OverlapCircle(position, 0.45f);

        return results == null;
    }

    // Called every frame
    private void Update()
    {
        // Shows the AI's score while training
        agentScore.text = snakeAgent.GetCumulativeReward().ToString("0.000");
    }
}
