using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassThroughPlatform : MonoBehaviour
{
    public float disableColliderTimer;
    [SerializeField] private Collider2D theCollider;
    private bool playerOnPlatform;
    // Start is called before the first frame update
    void Start()
    {
        //theCollider = GetComponent<Collider2D>();
    }

    private void SetPlayerOnPlatform(Collision2D other, bool value)
    {
        Player thePlayer = other.gameObject.GetComponent<Player>();
        if(thePlayer != null)
        {
            playerOnPlatform = value;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        SetPlayerOnPlatform(other, true);
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        SetPlayerOnPlatform(other, false);
    }
    // Update is called once per frame
    void Update()
    {
        if (playerOnPlatform && PlayerInput.IsPressingDown() && PlayerInput.HasPressedA())
        {
            Physics2D.IgnoreCollision(Player.Instance.gameObject.GetComponent<Collider2D>(), theCollider, true);
            //theCollider.enabled = false;
            StartCoroutine(EnableCollision());
        }
    }

    private IEnumerator EnableCollision()
    {
        yield return new WaitForSeconds(disableColliderTimer);
        Physics2D.IgnoreCollision(Player.Instance.gameObject.GetComponent<Collider2D>(), theCollider, false);
        //theCollider.enabled = true;
    }
}
