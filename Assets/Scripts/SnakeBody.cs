using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SnakeBody : MovingObject
{
    public int prevX;
    public int prevY;

    protected override bool Move(int xDir, int yDir, out RaycastHit2D hit)
    {
        Vector2 start = transform.position;
        Vector2 end = start + new Vector2(xDir, yDir);

        boxCollider.enabled = false;
        hit = Physics2D.Linecast(start, end, blockingLayer);
        boxCollider.enabled = true;

        //StartCoroutine(SmoothMovement(end));
        return true;
    }

    // Moves the body
    public void MoveBody(int xDir, int yDir)
    {
        prevX = this.xDir;
        prevY = this.yDir;

        RaycastHit2D hit;
        Move(this.xDir, this.yDir, out hit);

        this.xDir = xDir;
        this.yDir = yDir;
    }
}
