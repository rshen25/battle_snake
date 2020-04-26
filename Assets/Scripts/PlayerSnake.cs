using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSnake : SnakeHead
{

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        AddNewBodyPart(transform.position - new Vector3(1f, 0f, 0f));
    }

    // Update is called once per frame
    protected override void Update()
    {
        int x = (int)Input.GetAxisRaw("Horizontal");
        int y = (int)Input.GetAxisRaw("Vertical");

        if (x != 0 || y != 0)
        {
            xDirBuffer = x;
            yDirBuffer = y;
        }
    }

    // When the player snake collides with another object
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);

        // If collided with another snake part
        if (other.tag == "SnakeBody" || other.tag == "Snake Head" || other.tag == "Wall")
        {
            // Player loses
            CancelInvoke("MoveSnake");

            Debug.Log("Player lost, collided with " + other.tag);

            // Display you lost
            GameManager.instance.ShowLoseMessage();
        }
    }

}
