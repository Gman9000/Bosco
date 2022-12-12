using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenEntrance : MonoBehaviour
{
    //public GameObject explosionRadius;
    public float waitTime;

    public List<HiddenEntrance> adjacentHiddenEntrances;
    private bool hasExploded;
    public float delayTime;
    private float delayTimeCountdown;
    public bool canBeHitByPlayer;

    private void Start()
    {
        GameObject hiddenChild = new GameObject("Hidden Child");
        hiddenChild.transform.SetParent(transform, false);

        Transform[] childs = transform.GetComponentsInChildren<Transform>();
        foreach (Transform child in childs)
            child.tag = "Hidden";

        //adjacentHiddenEntrances
        for (int i = 0; i < adjacentHiddenEntrances.Count; i++)
        {

            HiddenEntrance temp = adjacentHiddenEntrances[i];
            int randomIndex = Random.Range(i, adjacentHiddenEntrances.Count);
            adjacentHiddenEntrances[i] = adjacentHiddenEntrances[randomIndex];
            adjacentHiddenEntrances[randomIndex] = temp;
        }

        for (int i = 0; i < adjacentHiddenEntrances.Count; i++)
        {
            adjacentHiddenEntrances[i].SetDelay((i + 1) * delayTime);
        }
    }
        public void SetDelay(float delayToSet)
    {
        delayTime = delayToSet;
    }

    
    public void HandleExplosion()
    {
        for (int i = 0; i < adjacentHiddenEntrances.Count; i++)
        {
            adjacentHiddenEntrances[i].HandleExplosion();
        }
        hasExploded = true;
        delayTimeCountdown = delayTime;
        //delayTimeCountdown = delayTime;
    }
    void Update()
    {
        if (hasExploded)
        {
            delayTimeCountdown -= Time.deltaTime;
            if(delayTimeCountdown <= 0 || canBeHitByPlayer)
            {
                StartCoroutine(Explosion());
            }
            
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        

        if (other.CompareTag("PlayerAttack") && canBeHitByPlayer)
        {
            HandleExplosion();
        }


    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("PlayerAttack") && canBeHitByPlayer)
        {
            HandleExplosion();
        }

    }

    private IEnumerator Explosion()
    {

        yield return new WaitForSeconds(waitTime);   //wait the established amount of seconds.

        this.gameObject.SetActive(false);
        Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/FX-explosion")).transform.position = transform.position;
        CameraController.Instance.VertShake(2);

        yield break;
    }
}
