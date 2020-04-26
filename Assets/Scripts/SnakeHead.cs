using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeHead : MovingObject
{
    public GameObject bodyPrefab;
    
    private Stack<GameObject> bodies;

    // Use buffer to constantly update the direction, then on MoveSnake, check the buffer and do the direction checking
    protected int xDirBuffer = 1;
    protected int yDirBuffer = 0;

    protected bool hasEaten = false;
    protected float turnTime = 0.35f;
    protected int stage = 0;
    protected float turnTimeIncrement = 0.75f;

    // Start is called before the first frame update
    protected override void Start()
    {
        //body = bodyPrefab.GetComponent<SnakeBody>();
        bodies = new Stack<GameObject>();
        base.Start();

        InvokeRepeating("MoveSnake", 1f, turnTime);
    }

    // Update is called once per frame
    protected virtual void Update()
    {

    }

    // Moves the snake (head and body included) in the current direction
    protected void MoveSnake()
    {
        // Dont move until the game board is done setting up
        if (GameManager.instance.doingSetup)
        {
            return;
        }

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
                SnakeBody body;
                foreach (GameObject obj in bodies)
                {
                    body = obj.GetComponent<SnakeBody>();
                    body.MoveBody(x, y);
                    x = body.prevX;
                    y = body.prevY;
                }
            }
        }

        // else create new body, link to rest of body, and do not move body
        else
        {
            this.boxCollider.enabled = false;
            AddNewBodyPart(transform.position);
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

            if (GameManager.instance.GetPlayerScore() % 100 == 0)
            {
                IncreaseMovementSpeed();
            }

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
        bodies.Push(body);
    }

    private void IncreaseMovementSpeed()
    {
        if (stage >= 6)
        {
            return;
        }

        stage++;
        turnTime = turnTime * turnTimeIncrement;
        CancelInvoke("MoveSnake");
        InvokeRepeating("MoveSnake", turnTime, turnTime);
    }

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
