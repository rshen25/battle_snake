using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class SnakeAgent : Agent
{
    public float moveTime;          // The amount of time between movements


    // Actions
    const int k_NoAction = 0;  // do nothing!
    const int k_Up = 1;
    const int k_Down = 2;
    const int k_Left = 3;
    const int k_Right = 4;

    // TODO: Initial setup of the snake agent, called when the agent is enabled
    public override void InitializeAgent()
    {
        base.InitializeAgent();
    }

    // TODO: Performs actions based on a vector of numbers
    // @param vectorAction - the list of actions for the agent to perform
    public override void AgentAction(float[] vectorAction)
    {
        base.AgentAction(vectorAction);

        // Add a reward for staying alive
        AddReward(0.01f);

    }

    // TODO: collect all non-raycast observations
    public override void CollectObservations()
    {
        base.CollectObservations();
    }
}
