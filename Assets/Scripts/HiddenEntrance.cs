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

    //randomization of list
    /*public void HandleReveal()
    {
        StartCoroutine(Explosion());
        //gameObject.SetActive(false);
    }*/

    private void Start()
    {
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
        /*if (other.CompareTag("ChainReaction"))
        {
            //Debug.Log("we in");
            other.gameObject.transform.parent.gameObject.GetComponent<HiddenEntrance>().HandleReveal();
        }*/

        if (other.CompareTag("PlayerAttack") && canBeHitByPlayer)
        {
            HandleExplosion();
        }


    }
    private void OnTriggerStay2D(Collider2D other)
    {
        /*if (other.CompareTag("ChainReaction"))
        {
            //Debug.Log("we in");
            other.gameObject.transform.parent.gameObject.GetComponent<HiddenEntrance>().HandleReveal();
        }*/
        if (other.CompareTag("PlayerAttack") && canBeHitByPlayer)
        {
            HandleExplosion();
        }

    }

    private IEnumerator Explosion()
    {
        //explosionRadius.SetActive(true);
        /*RaycastHit hit;
        
        Vector3 p1 = transform.position;
        

        // Cast a sphere wrapping character controller 10 meters forward
        // to see if it is about to hit anything.
        if (Physics.SphereCast(p1, 1, transform.forward, out hit, 8))
        {
            Debug.Log("WE in here" + hit.collider.tag);
            if (hit.collider.CompareTag("ChainReaction"))
            {
                hit.collider.gameObject.GetComponent<HiddenEntrance>().HandleReveal();
            }
        }*/

        yield return new WaitForSeconds(waitTime);   //wait the established amount of seconds.

        this.gameObject.SetActive(false);

        yield break;
    }
}
