using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoWayPlatform : MonoBehaviour
{
    private const float FALLTHROUGH_TIME = .15F;

    static public bool rockwallCondition => PlayerInput.Held(Button.Down) && !Player.Instance.IsDownSlash && !Player.IsMovesetLocked(PState.G_Rockclimb);
    public bool isRockwall = false;

    private BoxCollider2D boxCollider;
    private bool playerOnPlatform;

    private bool pauseCollisionChanges;

    void Awake()
    {
        if (isRockwall)
        {
            GetComponent<SpriteRenderer>().enabled = false;
        }

        boxCollider = GetComponent<BoxCollider2D>();

        if (isRockwall)
            gameObject.tag = "Rockwall";
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
            Physics2D.IgnoreCollision(Player.Instance.boxCollider2D, boxCollider, true);
            if (rockwallCondition || playerOnPlatform)
            {
                BoxCollider2D otherCollider = Player.Instance.boxCollider2D;
                float myTop = boxCollider.Top();
                float playerBottom = otherCollider.Bottom();

                if (myTop <= playerBottom + Game.PIXEL)
                {
                    Physics2D.IgnoreCollision(Player.Instance.boxCollider2D, boxCollider, false);     
                    
                }
            }
        }
        
        if (playerOnPlatform && PlayerInput.Held(Button.Down) && PlayerInput.Pressed(Button.B))
        {
            Physics2D.IgnoreCollision(Player.Instance.boxCollider2D, boxCollider, true);
            StartCoroutine(EnableCollision());
        }
    }

    private IEnumerator EnableCollision()
    {
        pauseCollisionChanges = true;
        yield return new WaitForSeconds(FALLTHROUGH_TIME);
        if (!isRockwall)
            Physics2D.IgnoreCollision(Player.Instance.boxCollider2D, boxCollider, false);
        pauseCollisionChanges = false;
    }
}
