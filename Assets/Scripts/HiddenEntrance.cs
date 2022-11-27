using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenEntrance : MonoBehaviour
{
    public GameObject explosionRadius;
    public float waitTime;

    public void HandleReveal()
    {
        gameObject.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("ChainReaction"))
        {
            other.gameObject.transform.parent.gameObject.GetComponent<HiddenEntrance>().HandleReveal();
        }
        if (other.CompareTag("PlayerAttack"))
        {
            explosionRadius.SetActive(true);
            StartCoroutine(Explosion());
        }

    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("ChainReaction"))
        {
            other.gameObject.transform.parent.gameObject.GetComponent<HiddenEntrance>().HandleReveal();
        }
        if (other.CompareTag("PlayerAttack"))
        {
            explosionRadius.SetActive(true);
            StartCoroutine(Explosion());
        }

    }

    private IEnumerator Explosion()
    {
        yield return new WaitForSeconds(waitTime);   //wait the established amount of seconds.

        this.gameObject.SetActive(false);

    }
}
