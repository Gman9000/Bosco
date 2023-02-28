using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassThroughPlatform : MonoBehaviour
{
    static public bool rockwallCondition => PlayerInput.IsPressingDown() && !Player.Instance.IsDownSlash && !Player.IsMovesetLocked(PState.G_Rockclimbing);
    public bool isRockwall = false;
    public float disableColliderTimer;
    [SerializeField] private BoxCollider2D theCollider;
    private bool playerOnPlatform;

    private bool pauseCollisionChanges;

    void Awake()
    {
        if (isRockwall)
        {
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }
    
    void Start()
    {
        pauseCollisionChanges = false;
    }
    
    private void SetPlayerOnPlatform(Collision2D other, bool value)
    {
        Player thePlayer = Player.Instance;

        if(other.gameObject.GetComponent<Player>() != null)
        {
            thePlayer.SetGrounded(value, gameObject.tag);
            playerOnPlatform = value;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if ((isRockwall && !rockwallCondition))
        {
            // ignore collision
        }
        else
            SetPlayerOnPlatform(other, true);
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        SetPlayerOnPlatform(other, false);
    }
    
    void Update()
    {
        if (pauseCollisionChanges)  return;


        if (isRockwall)
        {
            Physics2D.IgnoreCollision(Player.Instance.boxCollider2D, theCollider, true);
            if (rockwallCondition || playerOnPlatform)
            {
                BoxCollider2D otherCollider = Player.Instance.boxCollider2D;
                float myTop = theCollider.Top();
                float playerBottom = otherCollider.Bottom();

                if (myTop <= playerBottom + Game.PIXEL)
                {
                    Physics2D.IgnoreCollision(Player.Instance.boxCollider2D, theCollider, false);     
                    
                }
            }
        }
        
        if (playerOnPlatform && PlayerInput.IsPressingDown() && PlayerInput.HasPressedA())
        {
            Physics2D.IgnoreCollision(Player.Instance.boxCollider2D, theCollider, true);
            StartCoroutine(EnableCollision());
        }
    }

    private IEnumerator EnableCollision()
    {
        pauseCollisionChanges = true;
        yield return new WaitForSeconds(disableColliderTimer);
        if (!isRockwall)
            Physics2D.IgnoreCollision(Player.Instance.boxCollider2D, theCollider, false);
        pauseCollisionChanges = false;
    }
}
