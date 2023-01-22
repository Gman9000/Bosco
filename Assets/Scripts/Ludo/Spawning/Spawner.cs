using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpawnCondition {None, Start, Camera}

public class Spawner : MonoBehaviour
{
    public GameObject spawnObject;
    public Rect spawnBounds = new Rect(0, 0, 4, 4);
    public SpawnCondition spawnCondition = SpawnCondition.None;

    private Rect worldBounds;
    private GameObject spawnedEntity = null;
    private Pawn entityPawn = null;

    private bool outOfView = true;

    void OnDrawGizmos()
    {
        if (Application.isEditor)
        {
            worldBounds = spawnBounds;
            worldBounds.x += transform.position.x;
            worldBounds.y += transform.position.y;
        }

        Debug.DrawLine(new Vector3(worldBounds.x, worldBounds.y), new Vector3(worldBounds.x + worldBounds.width, worldBounds.y ),Color.green);
        Debug.DrawLine(new Vector3(worldBounds.x, worldBounds.y), new Vector3(worldBounds.x , worldBounds.y + worldBounds.height), Color.green);
        Debug.DrawLine(new Vector3(worldBounds.x + worldBounds.width, worldBounds.y + worldBounds.height), new Vector3(worldBounds.x + worldBounds.width, worldBounds.y), Color.green);
        Debug.DrawLine(new Vector3(worldBounds.x + worldBounds.width, worldBounds.y + worldBounds.height), new Vector3(worldBounds.x, worldBounds.y + worldBounds.height), Color.green);
    }

    void Start()
    {
        worldBounds = spawnBounds;
        worldBounds.x += transform.position.x;
        worldBounds.y += transform.position.y;

        outOfView = true;
        spawnedEntity = Instantiate(spawnObject);
        spawnedEntity.SetActive(false);
        spawnedEntity.transform.position = transform.position;
        entityPawn = spawnedEntity.GetComponent<Pawn>();
        if (entityPawn != null)
            entityPawn.SetBounds(worldBounds);

        if (spawnCondition == SpawnCondition.Start)
            TrySpawn();
    }

    void Update()
    {
        bool entityVisible = CameraController.viewRect.Contains(spawnedEntity.transform.position);
        bool spawnPointVisible = CameraController.viewRect.Contains(transform.position);
        bool entityInBounds = worldBounds.Contains(spawnedEntity.transform.position);
        bool worldBoundsVisible = worldBounds.Overlaps(CameraController.viewRect);


        if (!spawnPointVisible && !entityVisible)
        {
            Despawn();
        }

        if (spawnPointVisible)
        {
            if (outOfView && spawnCondition == SpawnCondition.Camera)
                TrySpawn();
            outOfView = false;
        }
        else
            outOfView = true;

    }

    public void Despawn()
    {
        spawnedEntity.SetActive(false);
    }

    public void TrySpawn()
    {
        if (!spawnedEntity.activeSelf)  // check dead
        {
            spawnedEntity.transform.position = transform.position;
            entityPawn.Start();
            spawnedEntity.SetActive(true);
        }
    }
}
