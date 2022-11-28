using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRespawner : MonoBehaviour
{
    public GameObject objectToSpawn;
    private bool isReadyToRespawnEnemy;
    // Start is called before the first frame update
    void Awake()
    {

    }
    public void IsReadyToSpawn(bool trigger)
    {
        isReadyToRespawnEnemy = trigger;
    }

    /*void OnBecameVisible()
    {
        if (isReadyToRespawnEnemy)
        {
            Debug.Log("we are ready to spawn");
        }
    }*/

    /*void OnBecameInvisible()
    {
        if (isReadyToRespawnEnemy)
        {
            Debug.Log("we are ready to spawn");
            objectToSpawn.transform.position = transform.position;
            objectToSpawn.SetActive(true);
            objectToSpawn.SendMessage("Start", SendMessageOptions.DontRequireReceiver);
        }
    }*/

    void Update()
    {
        if (isReadyToRespawnEnemy && (Mathf.Abs(Camera.main.transform.position.x - transform.position.x) > 7F
        || Mathf.Abs(Camera.main.transform.position.y - transform.position.y) > 6F))
        {
            Debug.Log("we are ready to spawn");
            objectToSpawn.transform.position = transform.position;
            objectToSpawn.SetActive(true);
            objectToSpawn.SendMessage("Start", SendMessageOptions.DontRequireReceiver);
        }
    }
}
