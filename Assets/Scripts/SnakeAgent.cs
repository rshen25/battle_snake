using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using MLAgents;

public class SnakeAgent : Agent
{
    public Vector2 dir = Vector2.left;

    public bool isMaskSet = true;

    public LayerMask blockingLayer;             // Layer where we check for collisions

    public GameObject bodyPrefab;

    protected bool hasEaten = false;

    protected BoxCollider2D boxCollider;        // The object's collision box
    protected new Rigidbody2D rigidbody;        // The object's rigid body, used for collision

    protected bool isMoving = false;

    private Arena arena;

    private int score = 0;

    private Vector3 foodPos;

    private List<GameObject> bodies;

    private Vector3 startingPos;

    private readonly float moveSpeed = 0.35f;

    private float prevDistance;
    private float rotation;

    private Vector2 bufferDir = Vector2.left;

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

        startingPos = transform.position + new Vector3(arena.columns - 2, arena.rows - 1, 0f);   // TODO: remove hard coded numbers

        InvokeRepeating("MoveSnake", 1f, moveSpeed);
    }

    public void Start()
    {
        boxCollider = gameObject.GetComponent<BoxCollider2D>();
        rigidbody = gameObject.GetComponent<Rigidbody2D>();
        rigidbody.rotation = 180f;

        SetFoodPos(arena.GetCurrentFoodPos());

        prevDistance = Vector2.Distance(transform.position, foodPos);

        AddNewBodyPart(transform.position + new Vector3(1f, 0f, 0f)); // TODO: remove hard coded numbers
        //foodPos = GameManager.instance.GetCurrentFoodPos(); TODO: switch back to game manager
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
            //if (dir == Vector2.left)
            //{
            //    // SetActionMask(0, new int[3] { k_Left, k_Right, k_NoAction });
            //    SetActionMask(0, new int[] { k_Left, k_Right });
            //}
            //if (dir == Vector2.up || dir == Vector2.down)
            //{
            //    SetActionMask(0, new int[1] { k_Left });
            //}
        }

        // If the AI snake is at the right edge of the board
        if (posX == maxPos)
        {
            SetActionMask(k_Right);
            //if (dir == Vector2.right)
            //{
            //    // SetActionMask(0, new int[3] { k_Left, k_Right, k_NoAction });
            //    SetActionMask(0, new int[2] { k_Left, k_Right });
            //}
            //if (dir == Vector2.up || dir == Vector2.down)
            //{
            //    SetActionMask(0, new int[3] { k_Right, k_Down, k_Up });
            //}
        }

        // If the AI snake is at the bottom edge of the board
        if (posY == 0)
        {
            SetActionMask(k_Down);
            //if (dir == Vector2.down)
            //{
            //    // SetActionMask(0, new int[3] { k_Down, k_Up, k_NoAction });
            //    SetActionMask(0, new int[2] { k_Down, k_Up });
            //}
            //if (dir == Vector2.left || dir == Vector2.right)
            //{
            //    SetActionMask(0, new int[3] { k_Right, k_Left, k_Down });
            //}
        }

        // If the AI snake is at the top edge of the board
        if (posY == maxPos)
        {
            SetActionMask(k_Up);
            //if (dir == Vector2.up)
            //{
            //    // SetActionMask(0, new int[3] { k_NoAction, k_Down, k_Up });
            //    SetActionMask(0, new int[2] { k_Down, k_Up });
            //}
            //if (dir == Vector2.left || dir == Vector2.right)
            //{
            //    SetActionMask(0, new int[3] { k_Right, k_Up, k_Left });
            //}
        }
    } 

    // Performs actions based on a vector of numbers
    // @param vectorAction - the list of actions for the agent to perform
    public override void AgentAction(float[] vectorAction)
    {
        int action = Mathf.FloorToInt(vectorAction[0]);

        switch(action)
        {
            //case k_NoAction:
            //    break;

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
        //return new float[] { k_NoAction };
    }

    // Collect all non-raycast observations
    public override void CollectObservations()
    {
        // The direction to the food
        AddVectorObs((transform.position - foodPos).normalized);

        // The direction of the AI
        AddVectorObs(dir);

        AddVectorObs(Vector2.Distance(transform.position, foodPos));

        // The previous distance to the food in the last step
        AddVectorObs(prevDistance);

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

        foreach (GameObject body in bodies)
        {
            Destroy(body);
        }
        bodies.Clear();

        transform.position = startingPos;
        transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));

        dir = Vector2.left;

        arena.ResetScores();

        InvokeRepeating("MoveSnake", 0f, moveSpeed);

        // GameManager.instance.ResetScores();
        Start();
    }


    // Rewards the AI for moving closer to the food and reduces reward for moving away
    private void AddMovementReward()
    {
        
        float currentDistance = Vector3.Distance(foodPos, transform.position);

        if (currentDistance <= prevDistance)
        {
            // AddReward(-0.012f);
            AddReward(0.01f);
        }
        //else
        //{
        //    AddReward(0.01f);
        //}

        prevDistance = currentDistance;
    }

    // Moves the snake (head and body included) in the current direction
    protected void MoveSnake()
    {
        Vector2 pos = transform.position;
        RequestDecision();

        // Move Head
        RaycastHit2D hit;
        if (!Move(dir, out hit)) return;

        // Debug.Log(transform.localPosition.ToString());

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
            // Add reward for moving closer to food and remove reward for moving away

            AddMovementReward();

            isMoving = false;
            return true;
        }

        // TODO: REMOVE
        // Debug.Log("Collided into: " + hit.collider.name);

        AddReward(-1f);

        Done();
        //AgentReset();

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
            // GameManager.instance.IncrementAIScore();
            if (arena)
            {
                arena.IncrementAIScore();
                Destroy(other.gameObject);
                // Debug.Log("CurrentPosition: " + transform.position.ToString());
                arena.RespawnFood();
                foodPos = arena.GetCurrentFoodPos();
            }

            hasEaten = true;

            AddReward(0.2f);

            // Spawn another food
            // GameManager.instance.RespawnFood();

            // foodPos = GameManager.instance.GetCurrentFoodPos();
        }

        // If collided with other snake body

        // If collided with other snake head
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
            dir = Vector2.up;
            //transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            //rigidbody.rotation = 90f;
            rotation = 90f;
        }
    }

    // Set the snake to move down if able
    private void MoveDown()
    {
        if (dir == Vector2.left || dir == Vector2.right)
        {
            dir = Vector2.down;
            //transform.rotation = Quaternion.Euler(0f, 0f, 270f);
            //rigidbody.rotation = 270f;
            rotation = 270f;
        }
    }

    // Set the snake to move right relative to the direction it is facing
    private void MoveRight()
    {
        if (dir == Vector2.up || dir == Vector2.down)
        {
            dir = Vector2.right;
            //transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            //rigidbody.rotation = 0f;
            rotation = 0f;
        }
        //if (dir == Vector2.right)
        //{
        //    dir = Vector2.down;
        //    transform.Rotate(new Vector3(0f, 0f, -90f));
        //    return;
        //}
        //if (dir == Vector2.left)
        //{
        //    dir = Vector2.up;
        //    transform.Rotate(new Vector3(0f, 0f, -90f));
        //    return;
        //}
        //if (dir == Vector2.up)
        //{
        //    dir = Vector2.right;
        //    transform.Rotate(new Vector3(0f, 0f, -90f));
        //    return;
        //}
        //if (dir == Vector2.down)
        //{
        //    dir = Vector2.left;
        //    transform.Rotate(new Vector3(0f, 0f, -90f));
        //    return;
        //}
    }

    // Set the snake to move left relative to the direction it is facing
    private void MoveLeft()
    {
        if (dir == Vector2.up || dir == Vector2.down)
        {
            dir = Vector2.left;
            //transform.rotation = Quaternion.Euler(0f, 0f, 180f);
            //rigidbody.rotation = 180f;
            rotation = 180f;
        }
        //if (dir == Vector2.right)
        //{
        //    dir = Vector2.up;
        //    transform.Rotate(new Vector3(0f, 0f, 90f));
        //    return;
        //}
        //if (dir == Vector2.left)
        //{
        //    dir = Vector2.down;
        //    transform.Rotate(new Vector3(0f, 0f, 90f));
        //    return;
        //}
        //if (dir == Vector2.up)
        //{
        //    dir = Vector2.left;
        //    transform.Rotate(new Vector3(0f, 0f, 90f));
        //    return;
        //}
        //if (dir == Vector2.down)
        //{
        //    dir = Vector2.right;
        //    transform.Rotate(new Vector3(0f, 0f, 90f));
        //    return;
        //}
    }
}
