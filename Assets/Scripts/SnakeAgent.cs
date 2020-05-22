﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using MLAgents;

public class SnakeAgent : Agent
{
    public Vector2 dir = Vector2.left;

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

        InvokeRepeating("MoveSnake", 1f, moveSpeed);
    }

    public void Start()
    {
        boxCollider = gameObject.GetComponent<BoxCollider2D>();
        rigidbody = gameObject.GetComponent<Rigidbody2D>();

        SetFoodPos(arena.GetCurrentFoodPos());

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
        AddVectorObs(transform.position - foodPos);

        // The distance from the food in the Y plane
        //AddVectorObs(transform.position.y - foodPos.y);

        // The current score of the AI
        //AddVectorObs(score);

        // The direction of the AI
        AddVectorObs(dir);

        AddVectorObs(transform.forward);

        //AddVectorObs(transform.position);

        //AddVectorObs(foodPos);
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

        InvokeRepeating("MoveSnake", 0f, moveSpeed);

        // GameManager.instance.ResetScores();
        Start();
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
            // Add reward for moving closer to food and remove reward for moving away
            float xDisToFood = Math.Abs(foodPos.x - transform.position.x);
            float yDisToFood = Math.Abs(foodPos.y - transform.position.y);
            AddReward(0.01f - (0.001f * (xDisToFood + yDisToFood)));
            isMoving = false;
            return true;
        }

        // TODO: REMOVE
        Debug.Log("Collided into: " + hit.collider.name);

        AddReward(-20f);
        Done();

        return false;
    }

    // Moves the snake (head and body included) in the current direction
    protected void MoveSnake()
    {
        RequestDecision();

        Vector2 pos = transform.position;
        // Move Head
        RaycastHit2D hit;
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

            AddReward(2f);

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

    // Set the snake to move right relative to the direction it is facing
    private void MoveRight()
    {
        if (dir == Vector2.right)
        {
            dir = Vector2.down;
            transform.Rotate(new Vector3(0f, 0f, -90f));
            return;
        }
        if (dir == Vector2.left)
        {
            dir = Vector2.up;
            transform.Rotate(new Vector3(0f, 0f, -90f));
            return;
        }
        if (dir == Vector2.up)
        {
            dir = Vector2.right;
            transform.Rotate(new Vector3(0f, 0f, -90f));
            return;
        }
        if (dir == Vector2.down)
        {
            dir = Vector2.left;
            transform.Rotate(new Vector3(0f, 0f, -90f));
            return;
        }
    }

    // Set the snake to move left relative to the direction it is facing
    private void MoveLeft()
    {
        if (dir == Vector2.right)
        {
            dir = Vector2.up;
            transform.Rotate(new Vector3(0f, 0f, 90f));
            return;
        }
        if (dir == Vector2.left)
        {
            dir = Vector2.down;
            transform.Rotate(new Vector3(0f, 0f, 90f));
            return;
        }
        if (dir == Vector2.up)
        {
            dir = Vector2.left;
            transform.Rotate(new Vector3(0f, 0f, 90f));
            return;
        }
        if (dir == Vector2.down)
        {
            dir = Vector2.right;
            transform.Rotate(new Vector3(0f, 0f, 90f));
            return;
        }
    }
}
