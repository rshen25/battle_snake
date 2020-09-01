using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour
{
    // The direction of the moving object
    public int xDir = 0;
    public int yDir = 0;

    public LayerMask blockingLayer;             // Layer where we check for collisions

    protected BoxCollider2D boxCollider;        // The object's collision box
    protected new Rigidbody2D rigidbody;        // The object's rigid body, used for collision

    protected bool isMoving = false;            // If the object is moving

    // Start is called before the first frame update
    protected virtual void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rigidbody = GetComponent<Rigidbody2D>();

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
            //StartCoroutine(SmoothMovement(end));
            rigidbody.position = end;
            // transform.position = end;
            return true;
        }

        return false;
    }

}
