using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRespawner : MonoBehaviour
{
    public GameObject objectToSpawn;

    private IEnemy enemyScript;

    void Start()
    {
        objectToSpawn.SetActive(false);
        enemyScript = objectToSpawn.GetComponent<IEnemy>();
    }

    void Update()
    {
        if (!Game.IsPointOnScreen(objectToSpawn.transform.position, 2))
            objectToSpawn.SetActive(false);

        if (!Game.IsPointOnScreen(transform.position, 1) && !objectToSpawn.activeSelf)
        {
            objectToSpawn.SetActive(true);
            enemyScript.Start();
            objectToSpawn.transform.localPosition = Vector3.zero;
        }
    }
}
