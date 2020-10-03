using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using MLAgents;

public class SnakeAgent : Agent
{
    public Vector2 dir = Vector2.left;          // The direction the snake will be moving

    public bool isMaskSet = true;               // Action mask to make the AI use the discrete actions mask we have defined

    public LayerMask blockingLayer;             // Layer where we check for collisions

    public GameObject bodyPrefab;               // The snake's body gameObject

    protected bool hasEaten = false;            // boolean to check if the snake has currently eaten food

    protected BoxCollider2D boxCollider;        // The object's collision box
    protected new Rigidbody2D rigidbody;        // The object's rigid body, used for collision

    protected bool isMoving = false;            // If the snake is currently moving

    private Arena arena;                        // Field to hold the training arena the snake agent belongs to

    private Vector3 foodPos;                    // The current position of the food tile on the game board

    private List<GameObject> bodies;            // A list to hold the snake's body

    private Vector3 startingPos;                // The original starting position of the snake

    private readonly float moveSpeed = 0.35f;   // The snakes movement speed, it moves at every interval specified here

    private float prevDistance;                 // The distance between the snake and the food tile in its previous movement step, used for AI decision making
    private float rotation;                     // The current rotation of the snake's head, used for AI decision making

    private Vector2 bufferDir = Vector2.left;   // The direction the snake is intended to move to

    // Actions
    // const int k_NoAction = 0;  // do nothing!
    const int k_Left = 0;
    const int k_Right = 1;
    const int k_Up = 2;
    const int k_Down = 3;

    // Initial setup of the snake agent, called when the agent is enabled
    public override void InitializeAgent()
    {
        base.InitializeAgent();

        bodies = new List<GameObject>();

        arena = GetComponentInParent<Arena>();

        startingPos = transform.position + new Vector3(arena.columns - 2, arena.rows - 1, 0f);

        InvokeRepeating("MoveSnake", 1f, moveSpeed);
    }

    // Called before the first frame of the game
    public void Start()
    {
        // Set up the collision box associated with the snake head
        boxCollider = gameObject.GetComponent<BoxCollider2D>();
        rigidbody = gameObject.GetComponent<Rigidbody2D>();
        rigidbody.rotation = 180f;

        // Get the current position of the food tile
        SetFoodPos(arena.GetCurrentFoodPos());

        // Calculate the distance of the food from the snake
        prevDistance = Vector2.Distance(transform.position, foodPos);

        // Add the initial snake body part
        AddNewBodyPart(transform.position + new Vector3(1f, 0f, 0f));
    }

    // Mask the actions of the AI snake to prevent it from hitting the wall when at the edges of the game board
    void SetMask()
    {
        int posX = (int)transform.localPosition.x;
        int posY = (int)transform.localPosition.y;

        int maxPos = (int)arena.columns - 1;

        // If the AI snake is at the left edge of the board
        if (posX == 1)
        {
            SetActionMask(k_Left);
        }

        // If the AI snake is at the right edge of the board
        if (posX == maxPos)
        {
            SetActionMask(k_Right);
        }

        // If the AI snake is at the bottom edge of the board
        if (posY == 1)
        {
            SetActionMask(k_Down);
        }

        // If the AI snake is at the top edge of the board
        if (posY == maxPos)
        {
            SetActionMask(k_Up);
        }
    } 

    // Performs actions based on a vector of numbers
    // @param vectorAction - the list of actions for the agent to perform
    public override void AgentAction(float[] vectorAction)
    {
        int action = Mathf.FloorToInt(vectorAction[0]);

        switch(action)
        {
            case k_Left:
                MoveLeft();
                break;

            case k_Right:
                MoveRight();
                break;

            case k_Up:
                MoveUp();
                break;

            case k_Down:
                MoveDown();
                break;

            default:
                throw new ArgumentException("Invalid action value");
        }
    }

    // AI movement heuristic, allows for manual movement of the AI when training
    public override float[] Heuristic()
    {
        if (Input.GetKey(KeyCode.D))
        {
            return new float[] { k_Right };
        }
        if (Input.GetKey(KeyCode.A))
        {
            return new float[] { k_Left };
        }
        if (Input.GetKey(KeyCode.S))
        {
            return new float[] { k_Down };
        }
        if (Input.GetKey(KeyCode.W))
        {
            return new float[] { k_Up };
        }
        return new float[] { -1f };
    }

    // Collect all non-raycast observations
    public override void CollectObservations()
    {
        // The direction to the food
        AddVectorObs((transform.position - foodPos).normalized);

        // The direction of the AI
        AddVectorObs(dir);

        // The current distance between the snake head and the food tile 
        AddVectorObs(Vector2.Distance(transform.position, foodPos));

        // The previous distance to the food in the last step
        AddVectorObs(prevDistance);

        // The current rotation of the agent's head
        Quaternion rotation = transform.rotation;
        Vector2 normalized = (rotation.eulerAngles / 180.0f) - Vector3.one;
        AddVectorObs(normalized);

        if (isMaskSet)
        {
            SetMask();
        }
    }

    // Resets the AI snake to its default starting position and status
    public override void AgentReset()
    {
        CancelInvoke("MoveSnake");

        // Destroy every body part
        foreach (GameObject body in bodies)
        {
            Destroy(body);
        }
        bodies.Clear();

        // Reset the agent's position and rotation
        transform.position = startingPos;
        transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));

        // Reset the direction
        dir = Vector2.left;
        bufferDir = Vector2.left;

        // Reset the score
        arena.ResetScores();

        InvokeRepeating("MoveSnake", 0f, moveSpeed);

        Start();
    }


    // Rewards the AI for moving closer to the food
    private void AddMovementReward()
    {
        
        float currentDistance = Vector3.Distance(foodPos, transform.position);

        if (currentDistance <= prevDistance)
        {
            AddReward(0.01f);
        }

        prevDistance = currentDistance;
    }

    // Moves the snake (head and body included) in the current direction
    protected void MoveSnake()
    {
        Vector2 pos = transform.position;
        RequestDecision();

        // Move Head
        RaycastHit2D hit;
        dir = bufferDir;
        if (!Move(dir, out hit)) return;

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
                SnakeBody body;

                // Get the tail
                GameObject snakeBodyObj = bodies[bodies.Count - 1];
                body = snakeBodyObj.GetComponent<SnakeBody>();

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
            AddNewBodyPart(transform.position);
            hasEaten = false;
            this.boxCollider.enabled = true;
        }
    }

    // Moves the object towards the direction provided and outputs true/false if successful and a raycast of any collisions
    protected virtual bool Move(Vector2 dir, out RaycastHit2D hit)
    {
        Vector2 start = transform.position;
        Vector2 end = start + dir;

        this.boxCollider.enabled = false;
        hit = Physics2D.Linecast(start, end, blockingLayer);
        this.boxCollider.enabled = true;

        if (!hit && !isMoving)
        {
            rigidbody.position = end;
            rigidbody.rotation = rotation;

            // Add reward for moving closer to food
            AddMovementReward();

            isMoving = false;
            return true;
        }

        AddReward(-1f);

        Done();

        return false;
    }

    // Triggers when the snake head collides/overlaps with another object in the level (in this case food)
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        // If collided with food
        if (other.tag == "Food")
        {
            // Eat the food, increase points
            // Update the game manager score
            if (arena)
            {
                arena.IncrementAIScore();
                Destroy(other.gameObject);
                arena.RespawnFood();
                foodPos = arena.GetCurrentFoodPos();
            }

            hasEaten = true;

            AddReward(0.2f);
        }
    }

    // Adds a new body part to the snake body
    public void AddNewBodyPart(Vector3 position)
    {
        GameObject body = Instantiate(bodyPrefab, position, Quaternion.identity);
        SnakeBody script = body.GetComponent<SnakeBody>();
        bodies.Insert(0, body);
    }

    // Increase the move speed for the snake
    public void IncreaseMovementSpeed(float turnTime)
    {
        CancelInvoke("MoveSnake");
        InvokeRepeating("MoveSnake", turnTime, turnTime);
    }

    // Sets the current position of the food
    public void SetFoodPos(Vector3 pos)
    {
        foodPos = pos;
    }

    // Sets the arena the snake agent belongs to
    public void SetArena(Arena arena)
    {
        this.arena = arena;
    }

    // Set the snake to move up if able
    private void MoveUp()
    {
        if (dir == Vector2.left || dir == Vector2.right)
        {
            bufferDir = Vector2.up;
            rotation = 90f;
        }
    }

    // Set the snake to move down if able
    private void MoveDown()
    {
        if (dir == Vector2.left || dir == Vector2.right)
        {
            bufferDir = Vector2.down;
            rotation = 270f;
        }
    }

    // Set the snake to move right relative to the direction it is facing
    private void MoveRight()
    {
        if (dir == Vector2.up || dir == Vector2.down)
        {
            bufferDir = Vector2.right;
            rotation = 0f;
        }
    }

    // Set the snake to move left relative to the direction it is facing
    private void MoveLeft()
    {
        if (dir == Vector2.up || dir == Vector2.down)
        {
            bufferDir = Vector2.left;
            rotation = 180f;
        }
    }
}
