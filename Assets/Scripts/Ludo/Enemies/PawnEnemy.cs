using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PawnEnemy : Pawn
{
    // INSPECTOR DEFINED VALUES
    public int maxHealth = 1;                           // amount of health enemy starts with
    public float invincibilityTime = .2F;               // time in seconds enemy is invincible
    public float knockbackDistance = .5F;               // knockback distance in Unity units
    public float fixedGravity = 0;                      // the fixed downward to apply to this enemy. AKA "cheap gravity"


    // CORE VARIABLES
    protected int currentHealth;                        // amount of health enemy currently has


    // COMPONENT REFERENCES
    protected Rigidbody2D body;
    protected BoxCollider2D boxCollider2D;
    protected SpriteRenderer sprite;
    protected SpriteAnimator anim;
    protected SpriteSimulator simulator;

    // ENGINEERING VARIABLES
    private int currentState;                           // which user-identifiable state this enemy pawn is in (e.g. about to throw attack vs hopping around)
    private Timer invTimer;
    public bool Invincible => invTimer != null && !invTimer.Done;
    protected Player playerTarget => Player.Instance;
    private Vector2 currentKnockback;                   // direction of the force of an attack that damages this pawn
    protected Vector2 positionWhenHit;                  // position in world space when the enemy was hit
    
    private bool _isGrounded;
    public bool IsGrounded => _isGrounded;

    [HideInInspector]public int facingDirection = 1;
    
    override public void Start()
    {
        anim = GetComponentInChildren<SpriteAnimator>();
        simulator = GetComponentInChildren<SpriteSimulator>();
        body = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        currentHealth = maxHealth;
        currentState = 0; // inits the enemy's state at
        _isGrounded = false;
    }
    public void Update()
    {
        // PRE-UPDATE LOGIC
        sprite.flipX = facingDirection < 0;
        
        _isGrounded = boxCollider2D.IsGrounded(.8F) != null;
        

        if (currentHealth <= 0)
        {
            // NOTE: leave this block and conditional as-is until we're done writing the structure of this class
        }
        else if (Invincible)
        {
            UpdateKnockback(positionWhenHit + currentKnockback.normalized * knockbackDistance);
        }
        else
        {
            switch (currentState)
            {
                case 0:
                    UpdateState0();
                    break;
                case 1:
                    UpdateState1();
                    break;
            }
        }
    }

    void FixedUpdate()
    {
        if (Invincible)
            sprite.color = sprite.color.a == 0 ? Color.white : new Color(0,0,0,0);
        else            
            sprite.color = Color.white;
    }

    public void SetState(int state) => currentState = state;    

    public void TakeDamage(Vector2 force)
    {
        if (Invincible || currentHealth <= 0)  return;          // ignore this call if the enemy is already invincible

        currentKnockback = force;
        facingDirection = (int)Mathf.Sign(-force.x);
        if (_isGrounded)
            currentKnockback = Vector2.right * facingDirection;
        
        positionWhenHit = transform.position;

        currentHealth--;


        if (currentHealth <= 0)
        {
            StartCoroutine(DeathSequence());
            return;
        }

        invTimer = Timer.Set(invincibilityTime, ()=>{
            currentKnockback = Vector2.zero;
        });

        OnHit();
    }


    /*================================*\
    |*--OVERRIDABLE UPDATE FUNCTIONS--*|
    \*================================*/

    protected virtual void OnHit() {}      // called first frame of being hit

    protected virtual void UpdateKnockback(Vector2 moveToPoint)
    {
        // TODO: CODE UNTESTED
        body.MovePosition(positionWhenHit + (moveToPoint - positionWhenHit) * invTimer.Progress);
    }

    protected virtual void UpdateState0() {}

    protected virtual void UpdateState1() {}

    protected virtual IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(.5F);
        gameObject.SetActive(false);
        yield break;
    }

    /*=======================*\
    |*--COLLISION FUNCTIONS--*|
    \*=======================*/

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("PlayerTarget") && !Invincible)
        {
            Debug.Log("Ass");
            Player.Instance.TakeDamage();
        }
    }
}
