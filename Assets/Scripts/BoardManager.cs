using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour
{

    // The size of the level
    public int columns = 13;
    public int rows = 13;

    public GameObject[] floorTiles;
    public GameObject[] wallTiles;
    public GameObject foodTile;

    private GameObject currentFoodTile;
    private Transform boardHolder;

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

    public void RespawnFood()
    {
        // Get a random position on the board
        Vector3 foodPosition = ChooseRandomPosition();

        int tries = columns * rows;

        // Check if the space is free, loop until a space is found
        while (!CheckIfFreeSpace(new Vector2(foodPosition.x, foodPosition.y)) || tries == 0)
        {
            foodPosition = ChooseRandomPosition();
            tries--;
        }

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

    // Checks if a space is free to spawn a food tile
    private bool CheckIfFreeSpace(Vector2 position)
    {

        List<Collider2D> colliders = new List<Collider2D>();

        Collider2D results = Physics2D.OverlapCircle(position, 0.45f);

        // if (results) Debug.Log(results.tag);

        return results == null;
    }

}
