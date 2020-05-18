using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour
{
    public int xDir = 0;
    public int yDir = 0;

    public float moveTime = 0.1f;
    public LayerMask blockingLayer;             // Layer where we check for collisions

    protected BoxCollider2D boxCollider;        // The object's collision box
    protected new Rigidbody2D rigidbody;        // The object's rigid body, used for collision

    protected bool isMoving = false;

    private float inverseMoveTime;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rigidbody = GetComponent<Rigidbody2D>();

        inverseMoveTime = 1f / moveTime;
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
            transform.position = end;
            return true;
        }

        Debug.Log("Something collided");
        return false;
    }

    // Used to move units from one space to the next
    // param: end - specifies the ending position where the unit should move to
    //protected IEnumerator SmoothMovement (Vector3 end)
    //{
    //    isMoving = true;
    //    float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

    //    while (sqrRemainingDistance > float.Epsilon)
    //    {
    //        Vector3 newPosition = Vector3.MoveTowards(rigidbody.position, end, inverseMoveTime * Time.deltaTime);

    //        rigidbody.MovePosition(end);

    //        sqrRemainingDistance = (transform.position - end).sqrMagnitude;

    //        yield return null;
    //    }

    //    //The object is no longer moving.
    //    isMoving = false;
    //}
}
