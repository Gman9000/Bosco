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
    public Vector2 visionBoxSize = new Vector2(10, 5);  // the bounds that a player must be inside to trigger non-idle state


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
    protected float InvProgress => invTimer.Progress;
    private Rect visionBox;
    private Timer invTimer;
    public bool Invincible => invTimer != null && !invTimer.Done;
    protected Player playerTarget => Player.Instance;
    private Vector2 currentKnockback;                   // direction of the force of an attack that damages this pawn
    protected Vector2 positionWhenHit;                  // position in world space when the enemy was hit
    
    private bool _isGrounded;
    private bool _contactLeft;
    private bool _contactRight;
    private bool _contactUp;
    public bool IsGrounded => _isGrounded;
    
    //private Vector2 velocity;
    private Vector2? moveTo;

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
        _contactUp = false;
        _contactLeft = false;
        _contactRight = false;
        visionBox = new Rect(transform.position.x, transform.position.y, visionBoxSize.x, visionBoxSize.y);
        //velocity = Vector2.zero;
        moveTo = null;
    }
    public void Update()
    {
        // PRE-UPDATE LOGIC
        sprite.flipX = facingDirection < 0;
        visionBox.center = transform.position;     // update the center of the vision box to this pawn

        PhysicsLogic();
        moveTo = null;       

        // UPDATE LOGIC
        if (currentHealth <= 0)
        {
            // NOTE: leave this block and conditional as-is until we're done writing the structure of this class
        }
        else if (Invincible)
        {
            moveTo = UpdateKnockback(positionWhenHit + currentKnockback.normalized * knockbackDistance);
        }
        else if (visionBox.Contains(Player.Position))
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
        else
        {
            SetState(0);
            UpdateStateIdle();
        }
    }

    private void PhysicsLogic()
    {
        Vector2 motion = moveTo == null ? Vector2.zero : (Vector2)(moveTo - transform.position);
        if (!_isGrounded)
            motion.y -= fixedGravity;
        // COLLISION LOGIC        
        HitInfo groundCheck = boxCollider2D.IsGrounded(.8F);
        HitInfo upCheck = boxCollider2D.IsHittingCeiling(.8F);
        HitInfo leftCheck = boxCollider2D.IsHittingLeft(.8F);
        HitInfo rightCheck = boxCollider2D.IsHittingRight(.8F);
        
        _isGrounded = groundCheck;

        

        if (_isGrounded)
        {
            float yDiff = transform.position.y - boxCollider2D.bounds.min.y;
            float contactY = groundCheck.hit.point.y + yDiff;
            body.MovePosition(new Vector2(transform.position.x, contactY));
        }
        else
        {
            if (motion.y < 0)
                body.MovePosition((Vector2)transform.position + motion * Vector2.up);
        }

        if (motion.y > 0)
        {        
            if (upCheck)
            {
                float yDiff = transform.position.y - boxCollider2D.bounds.max.y;
                float contactY = upCheck.hit.point.y + yDiff;
                body.MovePosition(new Vector2(transform.position.x, contactY));
            }
            else
                body.MovePosition((Vector2)transform.position + motion * Vector2.up);
        }


        if (motion.x < 0)
        {
            if (leftCheck)
            {
                float xDiff = transform.position.x - boxCollider2D.bounds.min.x;
                float contactX = groundCheck.hit.point.x + xDiff;
                body.MovePosition(new Vector2(contactX, transform.position.y));
            }
            else
                body.MovePosition((Vector2)transform.position + motion * Vector2.right);
        }
        else if (motion.x > 0)
        {
            if (rightCheck)
            {
                float xDiff = transform.position.x - boxCollider2D.bounds.max.x;
                float contactX = groundCheck.hit.point.x - xDiff;
                body.MovePosition(new Vector2(contactX, transform.position.y));
            }
            else
                body.MovePosition((Vector2)transform.position + motion * Vector2.right);
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
        if (_isGrounded && force.y < 0)
            currentKnockback.y = 0;
        
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

    protected virtual Vector2 UpdateKnockback(Vector2 moveToPoint)
    {
        return positionWhenHit + (moveToPoint - positionWhenHit) * invTimer.Progress;
    }

    protected virtual void UpdateStateIdle() {}

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
            Player.Instance.TakeDamage();
        }
    }
}
