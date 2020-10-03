# Battle Snake
A simple <a href="https://en.wikipedia.org/wiki/Snake_(video_game_genre)">Snake</a> game played against an AI to see which one gets the higher score, developed using the Unity Engine and written in C#.

# Download/Run game
You may download the game <a href=""> here </a>. 
To run, execute the Snake Unity application found in the download.

# About
<p>
A basic game of Snake where the player must get the highest score but with a twist. The player must compete against an AI.
So the player will need to avoid colliding with itself, the walls, and the other AI snake while attempting to eat as many food pellets as possible.
The AI was trained using reinforcement learning through the <a href="https://github.com/Unity-Technologies/ml-agents/tree/0.14.0">Unity Machine Learning Agents Toolkit (ML-Agents Toolkit)</a>.
ML-Agents is a Unity library that allows for training agents or NPCS through reinforcement learning, neuroevolution, or through other machine learning methods. ML-Agents provides a
Python API and its machine learning algorithms are based on <a href="https://www.tensorflow.org/">TensorFlow</a>, an open source platform for machine learning.
</p>

![SnakeDemo GIF](https://github.com/rshen25/battle_snake/blob/master/res/snake_demo.gif)

# Training Process
<p>
To train the AI, we must define a learning policy, which consists of three main parameters: <strong>Observations, Actions, and Rewards.</strong> 
Observations are what the agent perceives about its environment. In this case, I have defined the following to be relevant for the agent:
</p>
<ul>
<li>Direction of the food pellet in relation to the agent</li>
<li>Distance between the agent and the food pellet</li>
<li>Current direction the agent is moving towards</li>
<li>Current rotation of the agent's head</li>
<li>Direction of the agent was moving in the previous step</li>
</ul>

```
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
```

<p>
The agent also has three raycasts coming out of its head to sense any obstacles to its immediate left, right, and forward vectors. This set of information seemed to provide the most success
in training and yielding desirable results. Previous iterations that did not seem as successful did not include relative direction to the goal, only the current position of the goal and the
current position of the snake. Also, some of the data was not normalized between a value of [-1, 1], which also yielded varied results. So, with this information the agent uses it to make a 
decision on which action to take that will maximize their rewards, either continue moving <em>forward, left, or right</em>. 
</p>

<p>
There were various iterations of the reward system, some earlier iterations involved giving a small reward every time the agent moved in a positive direction towards the goal. Another was to
punish the agent when it moved away or when it collided. However, what proved to be the most successful was to give a small reward when the agent moved closer to the goal than its previous 
movement step, and to not punish it when it moved away or collided with anything. And of course, a much larger reward was given when the agent achieved its goal - eating a food pellet. 
Negative rewards did not prove very effective in training the AI to achieve its goal. 
</p>
Below you can see the training process in action, the first gif is of the training at its initial stages.

![SnakeTrainingEarly GIF](https://github.com/rshen25/battle_snake/blob/master/res/snake_early_training_demo.gif)

Here is the training process 25 minutes in.

![SnakeTrainingLate GIF](https://raw.githubusercontent.com/rshen25/battle_snake/master/res/snake_20m_training_demo.gif)

