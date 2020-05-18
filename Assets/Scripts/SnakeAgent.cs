using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using MLAgents;

public class SnakeAgent : Agent
{
    public int xDir = -1;
    public int yDir = 0;

    public float moveTime = 0.1f;
    public LayerMask blockingLayer;             // Layer where we check for collisions

    public GameObject bodyPrefab;

    protected bool hasEaten = false;

    protected BoxCollider2D boxCollider;        // The object's collision box
    protected new Rigidbody2D rigidbody;        // The object's rigid body, used for collision

    protected bool isMoving = false;

    private Arena arena;

    private int score = 0;

    private Vector3 foodPos;

    private float inverseMoveTime;

    private List<GameObject> bodies;

    private Vector3 startingPos;

    private readonly float moveSpeed = 0.35f;

    // Actions
    const int k_NoAction = 0;  // do nothing!
    const int k_Left = 1;
    const int k_Right = 2;

    // Initial setup of the snake agent, called when the agent is enabled
    public override void InitializeAgent()
    {
        base.InitializeAgent();

        bodies = new List<GameObject>();

        arena = GetComponentInParent<Arena>();

        startingPos = transform.position + new Vector3(11f, 12f, 0f);   // TODO: remove hard coded numbers

        inverseMoveTime = 1f / moveTime;

        InvokeRepeating("MoveSnake", 1f, moveSpeed);
    }

    public void Start()
    {
        SetFoodPos(arena.GetCurrentFoodPos());

        boxCollider = gameObject.GetComponent<BoxCollider2D>();
        rigidbody = gameObject.GetComponent<Rigidbody2D>();

        AddNewBodyPart(transform.position + new Vector3(1f, 0f, 0f)); // TODO: remove hard coded numbers
        //foodPos = GameManager.instance.GetCurrentFoodPos(); TODO: switch back to game manager
    }

    // Performs actions based on a vector of numbers
    // @param vectorAction - the list of actions for the agent to perform
    public override void AgentAction(float[] vectorAction)
    {
        int action = Mathf.FloorToInt(vectorAction[0]);

        switch(action)
        {
            case k_NoAction:
                break;

            case k_Left:
                MoveLeft();
                break;

            case k_Right:
                MoveRight();
                break;

            default:
                throw new ArgumentException("Invalid action value");
        }
    }

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
        return new float[] { k_NoAction };
    }

    // Collect all non-raycast observations
    public override void CollectObservations()
    { 
        // The distance from the food in the X plane
        AddVectorObs(transform.position.x - foodPos.x);

        // The distance from the food in the Y plane
        AddVectorObs(transform.position.y - foodPos.y);

        // The current score of the AI
        AddVectorObs(score);

        // The direction of the AI
        AddVectorObs(xDir);
        AddVectorObs(yDir);
    }

    // Resets the AI snake to its default starting position and status
    public override void AgentReset()
    {
        foreach (GameObject body in bodies)
        {
            Destroy(body);
        }
        bodies.Clear();

        transform.position = startingPos;

        xDir = -1;
        yDir = 0;

        // GameManager.instance.ResetScores();
        Start();
    }

    // Moves the object towards the direction provided and outputs true/false if successful and a raycast of any collisions
    protected virtual bool Move(int xDir, int yDir, out RaycastHit2D hit)
    {
        Vector2 start = transform.position;
        Vector2 end = start + new Vector2(xDir, yDir);

        this.boxCollider.enabled = false;
        hit = Physics2D.Linecast(start, end, blockingLayer);
        this.boxCollider.enabled = true;

        if (hit.transform == null && !isMoving)
        {
            StartCoroutine(SmoothMovement(end));
            // Add reward for moving closer to food and remove reward for moving away
            float xDisToFood = Math.Abs(foodPos.x - transform.position.x);
            float yDisToFood = Math.Abs(foodPos.y - transform.position.y);
            AddReward(0.01f - (0.001f * (xDisToFood + yDisToFood)));
            return true;
        }

        // TODO: REMOVE
        Debug.Log("Collided into: " + hit.collider.name);

        AddReward(-10f);
        Done();

        return false;
    }

    // Used to move units from one space to the next
    // param: end - specifies the ending position where the unit should move to
    protected IEnumerator SmoothMovement(Vector3 end)
    {
        isMoving = true;
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPosition = Vector3.MoveTowards(rigidbody.position, end, inverseMoveTime * Time.deltaTime);

            rigidbody.MovePosition(end);

            sqrRemainingDistance = (transform.position - end).sqrMagnitude;

            yield return null;
        }

        //The object is no longer moving.
        isMoving = false;
    }

    // Moves the snake (head and body included) in the current direction
    protected void MoveSnake()
    {
        //// Dont move until the game board is done setting up
        //if (GameManager.instance.doingSetup)
        //{
        //    return;
        //}

        RequestDecision();

        Vector2 pos = transform.position;
        // Move Head
        RaycastHit2D hit;
        if (!Move(xDir, yDir, out hit)) return;

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
                //int x = xDir;
                //int y = yDir;
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

                //foreach (GameObject obj in bodies)
                //{
                //    body = obj.GetComponent<SnakeBody>();
                //    body.MoveBody(x, y);
                //    x = body.prevX;
                //    y = body.prevY;
                //}
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

    // Triggers when the snake head collides/overlaps with another object in the level (in this case food)
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        // If collided with food
        if (other.tag == "Food")
        {
            // Eat the food, increase points
            // Update the game manager score
            // GameManager.instance.IncrementAIScore();
            if (arena)
            {
                arena.IncrementAIScore();
                Destroy(other.gameObject);
                // Debug.Log("Destroyed : " + other.tag);
                arena.RespawnFood();
                // Debug.Log("Arena is " + arena.name);
                foodPos = arena.GetCurrentFoodPos();
            }

            hasEaten = true;

            AddReward(1f);

            // Spawn another food
            // GameManager.instance.RespawnFood();

            // foodPos = GameManager.instance.GetCurrentFoodPos();
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

    // Set the snake to move right relative to the direction it is facing
    private void MoveRight()
    {
        if (xDir == 1)
        {
            xDir = 0;
            yDir = -1;
            return;
        }
        if (xDir == -1)
        {
            xDir = 0;
            yDir = 1;
            return;
        }
        if (yDir == 1)
        {
            xDir = 1;
            yDir = 0;
            return;
        }
        if (yDir == -1)
        {
            xDir = -1;
            yDir = 0;
            return;
        }
    }

    // Set the snake to move left relative to the direction it is facing
    private void MoveLeft()
    {
        if (xDir == 1)
        {
            xDir = 0;
            yDir = 1;
            return;
        }
        if (xDir == -1)
        {
            xDir = 0;
            yDir = -1;
            return;
        }
        if (yDir == 1)
        {
            xDir = -1;
            yDir = 0;
            return;
        }
        if (yDir == -1)
        {
            xDir = 1;
            yDir = 0;
            return;
        }
    }
}
