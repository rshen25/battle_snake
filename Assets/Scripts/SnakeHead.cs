using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeHead : MovingObject
{
    public GameObject bodyPrefab;           // The snake's body gameObject

    private List<GameObject> bodies;        // A list to hold the snake's body

    protected Vector2 originalPos;          // The original starting position of the snake

    // Use buffer to constantly update the direction, then on MoveSnake, check the buffer and do the direction checking
    protected int xDirBuffer = 1;
    protected int yDirBuffer = 0;

    protected bool hasEaten = false;        // boolean to check if the snake has currently eaten food

    // Start is called before the first frame updates
    protected override void Start()
    {
        //body = bodyPrefab.GetComponent<SnakeBody>();
        bodies = new List<GameObject>();
        base.Start();
        originalPos = transform.position;
        InvokeRepeating("MoveSnake", 1f, GameManager.instance.turnTime);
    }

    // Called every frame
    protected virtual void Update()
    {}

    // Moves the snake (head and body included) in the current direction
    protected void MoveSnake()
    {
        // Dont move until the game board is done setting up
        if (GameManager.instance.doingSetup)
        {
            return;
        }
        Vector2 pos = transform.position;

        // Update Direction
        UpdateDirection();

        // Move Head
        MoveHead();

        // If no body to instantiate
        if (!hasEaten)
        {
            if (!this.boxCollider.enabled)
            {
                this.boxCollider.enabled = true;
            }
            // Move Body
            if (bodies.Count > 0)
            {
                int x = xDir;
                int y = yDir;

                // Get the tail
                GameObject snakeBodyObj = bodies[bodies.Count - 1];
                SnakeBody body = snakeBodyObj.GetComponent<SnakeBody>();

                // Remove the tail from the list
                bodies.RemoveAt(bodies.Count - 1);

                // Move tail
                snakeBodyObj.transform.position = pos;

                // Insert the tail to the front of the bodies list
                bodies.Insert(0, snakeBodyObj);

            }
        }

        // else create new body, link to rest of body, and do not move body
        else
        {
            this.boxCollider.enabled = false;
            AddNewBodyPart(pos);
            hasEaten = false;
        }
    }

    // Moves the head of the snake
    private void MoveHead()
    {
        RaycastHit2D hit;
        Move(xDir, yDir, out hit);
    }

    // Triggers when the snake head collides/overlaps with another object in the level (in this case food)
    protected virtual void OnTriggerEnter2D (Collider2D other)
    {
        // If collided with food
        if (other.tag == "Food")
        {
            // Eat the food, increase points
            // Update the game manager score
            GameManager.instance.IncrementPlayerScore();

            hasEaten = true;

            Destroy(other.gameObject);

            // Spawn another food
            GameManager.instance.RespawnFood();
        }
    }

    // Adds a new body part to the snake body
    public void AddNewBodyPart(Vector3 position)
    {
        GameObject body = Instantiate(bodyPrefab, position, Quaternion.identity);
        SnakeBody script = body.GetComponent<SnakeBody>();
        script.xDir = this.xDir;
        script.yDir = this.yDir;
        bodies.Insert(0, body);
    }

    // Increases the movement speed of the snake head
    public void IncreaseMovementSpeed()
    {
        CancelInvoke("MoveSnake");
        InvokeRepeating("MoveSnake", GameManager.instance.turnTime, GameManager.instance.turnTime);
    }

    // Resets the position and movement speed of the snake to its initial state
    public void ResetSnake()
    {
        // Stop movement
        CancelInvoke("MoveSnake");

        // Destroy the body parts
        foreach (GameObject body in bodies)
        {
            Destroy(body);
        }
        bodies.Clear();

        // Reset the snake's position and rotation to its original starting position
        transform.position = originalPos;
        transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));

        // Reset the direction
        xDir = 1;
        xDirBuffer = 1;
        yDir = 0;
        yDirBuffer = 0;

        Start();
    }

    // Checks the direction to not allow the snake to move in the opposite direction of its current vector
    // and sets the current direction of the snake
    private void UpdateDirection()
    {
        if (xDirBuffer * -1 == xDir)
        {
            xDirBuffer = xDir;
        }

        if (yDirBuffer * -1 == yDir)
        {
            yDirBuffer = yDir;
        }

        xDir = xDirBuffer;
        yDir = yDirBuffer;

        if (xDir != 0)
        {
            yDir = 0;
        }
    }

}
