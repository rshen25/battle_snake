using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour
{
    private const int columns = 35;             // The amount of columns in the game board
    private const int rows = 35;                // The amount of rows in the game board

    public GameObject[] floorTiles;             // An array to hold the types of floor gameObjects tiles that comprises the floor of the game board
    public GameObject[] wallTiles;              // An array to hold the wall gameObjects tiles comprising the walls of the game board
    public GameObject foodTile;                 // The gameObject that will be used as the food

    private GameObject currentFoodTile;         // The food tile that will be spawned

    // Stores the possible positions on the game board
    private List<Vector3> gridPositions = new List<Vector3>();

    // Initializes the grid position list
    private void InitializeGrid()
    {
        gridPositions.Clear();

        for (int x = 1; x < columns - 1; x++)
        {
            for (int y = 1; y < rows - 1; y++)
            {
                gridPositions.Add(transform.position + new Vector3(x, y, 0f));
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
            }
        }
    }

    // Chooses a random position within the game board
    public Vector3 ChooseRandomPosition()
    {
        int randomIndex = Random.Range(0, gridPositions.Count);
        return gridPositions[randomIndex];
    }

    // Respawns the food when it is eaten
    public void RespawnFood()
    {
        // Get a random position on the board
        Vector3 foodPosition = ChooseRandomPosition();

        // The number of tries before giving up
        int tries = columns * rows;

        // Check if the space is free, loop until a space is found
        while (!CheckIfFreeSpace(new Vector2(foodPosition.x, foodPosition.y)) || tries == 0)
        {
            foodPosition = ChooseRandomPosition();
            tries--;
        }

        // Spawn in the food object
        currentFoodTile = Instantiate(foodTile, foodPosition, Quaternion.identity);
    }

    // Sets up the level
    public void SetupScene()
    {
        InitializeGrid();
        BoardSetup();

        // Place food at the center of the game board
        currentFoodTile = Instantiate(foodTile, transform.position + new Vector3((int)(columns / 2), (int)(rows / 2), 0f), Quaternion.identity);
    }

    // Returns the position of the current food
    public Vector3 GetCurrentFoodPos()
    {
        return currentFoodTile.transform.position;
    }

    // Returns the number of rows of the game board
    public int GetRows()
    {
        return rows;
    }

    // Returns the number of columns of the game board
    public int GetColumns()
    {
        return columns;
    }

    // Resets the board to its initial state
    public void ResetBoard()
    {
        // Reset the food
        Destroy(currentFoodTile);
        currentFoodTile = currentFoodTile = Instantiate(foodTile, transform.position + new Vector3((int)(columns / 2), (int)(rows / 2), 0f), Quaternion.identity);
    }

    // Checks if a space is free to spawn a food tile
    private bool CheckIfFreeSpace(Vector2 position)
    {
        List<Collider2D> colliders = new List<Collider2D>();

        Collider2D results = Physics2D.OverlapCircle(position, 0.45f);

        return results == null;
    }

}
