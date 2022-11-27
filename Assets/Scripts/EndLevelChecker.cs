using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndLevelChecker : MonoBehaviour
{
    public bool isFinalLevelEnder;
    private bool hasBeenCompleted;
    public GameObject nextLevelConnector;

    // Start is called before the first frame update
    void Awake()
    {
        hasBeenCompleted = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void HandleEndOfLevel()
    {
        hasBeenCompleted = true;
        if (!isFinalLevelEnder)
        {
            Player.Instance.gameObject.transform.position = nextLevelConnector.transform.position;
        }
        else
        {
            //tell the levelManager to win the game
            //LevelManager.Instance.WinGame();
        }
    }

    public bool GetEndLevelCompletionStatus()
    {
        return hasBeenCompleted;
    }
}
