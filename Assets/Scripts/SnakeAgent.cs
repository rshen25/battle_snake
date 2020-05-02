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

    private int score = 0;

    private float inverseMoveTime;

    private Stack<GameObject> bodies;

    private Vector3 startingPos;

    private readonly float initialMoveSpeed = 0.35f;

    // Actions
    const int k_NoAction = 0;  // do nothing!
    const int k_Left = 1;
    const int k_Right = 2;
    const int k_Up = 3;
    const int k_Down = 4;

    // Initial setup of the snake agent, called when the agent is enabled
    public override void InitializeAgent()
    {
        base.InitializeAgent();

        bodies = new Stack<GameObject>();

        InvokeRepeating("MoveSnake", 1f, initialMoveSpeed);
    }

    private void Start()
    {
        startingPos = new Vector3(10, 11, 0);   // TODO: remove hard coded numbers

        inverseMoveTime = 1f / moveTime;

        boxCollider = GetComponent<BoxCollider2D>();
        rigidbody = GetComponent<Rigidbody2D>();
        AddNewBodyPart(transform.position + new Vector3(1f, 0f, 0f)); // TODO: remove hard coded numbers
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
                xDir = -1;
                yDir = 0;
                break;

            case k_Right:
                xDir = 1;
                yDir = 0;
                break;

            case k_Up:
                yDir = 1;
                xDir = 0;
                break;

            case k_Down:
                yDir = -1;
                xDir = 0;
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
        if (Input.GetKey(KeyCode.W))
        {
            return new float[] { k_Up };
        }
        if (Input.GetKey(KeyCode.A))
        {
            return new float[] { k_Left };
        }
        if (Input.GetKey(KeyCode.S))
        {
            return new float[] { k_Down };
        }
        return new float[] { k_NoAction };
    }

    // Collect all non-raycast observations
    public override void CollectObservations()
    { 
        Vector3 foodPos = GameManager.instance.GetCurrentFoodPos();

        // The distance from the food in the X plane
        AddVectorObs(transform.position.x - foodPos.x);

        // The distance from the food in the Y plane
        AddVectorObs(transform.position.y - foodPos.y);

        // The current score of the AI
        AddVectorObs(score);
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

        GameManager.instance.ResetScores();

        this.Start();
    }

    private void FixedUpdate()
    {
        if (GetStepCount() % 5 == 0)
        {
            RequestDecision();
        }
        else
        {
            RequestAction();
        }
    }

    // Moves the object towards the direction provided and outputs true/false if successful and a raycast of any collisions
    protected virtual bool Move(int xDir, int yDir, out RaycastHit2D hit)
    {
        Vector2 start = transform.position;
        Vector2 end = start + new Vector2(xDir, yDir);

        boxCollider.enabled = false;
        hit = Physics2D.Linecast(start, end, blockingLayer);
        boxCollider.enabled = true;

        if (hit.transform == null && !isMoving)
        {
            StartCoroutine(SmoothMovement(end));
            return true;
        }

        // TODO: REMOVE
        Debug.Log("Something collided");

        AddReward(-1000f);
        Done();

        return false;
    }

    // Used to move units from one space to the next
    // param: end - specifies the ending position where the unit should move to
    protected IEnumerator SmoothMovement(Vector3 end)
    {
        isMoving = true;
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

        Vector3 foodPos = GameManager.instance.GetCurrentFoodPos();

        // Add reward for moving closer to food and remove reward for moving away
        float xDisToFood = Math.Abs(transform.position.x - foodPos.x);
        float yDisToFood = Math.Abs(transform.position.y - foodPos.y);

        AddReward(0.01f - 0.001f * (xDisToFood + yDisToFood));

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
        // Dont move until the game board is done setting up
        if (GameManager.instance.doingSetup)
        {
            return;
        }

        // Move Head
        RaycastHit2D hit;
        Move(xDir, yDir, out hit);

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

    // Triggers when the snake head collides/overlaps with another object in the level (in this case food)
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        // If collided with food
        if (other.tag == "Food")
        {
            // Eat the food, increase points
            // Update the game manager score
            GameManager.instance.IncrementAIScore();

            hasEaten = true;

            Destroy(other.gameObject);

            AddReward(1f);

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

    public void IncreaseMovementSpeed()
    {
        CancelInvoke("MoveSnake");
        InvokeRepeating("MoveSnake", GameManager.instance.turnTime, GameManager.instance.turnTime);
    }
}
