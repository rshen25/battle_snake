using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour
{

    public GameObject gameManager;              // Game Manager prefab to instantiate

    // Start is called before the first frame update
    void Awake()
    {
        if (GameManager.instance == null)
        {
            Instantiate(gameManager);
        }
    }
}
