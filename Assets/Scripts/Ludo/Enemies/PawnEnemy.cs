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
    private Coroutine currentState;

    protected System.Func<IEnumerator> stateIdle;
    protected System.Func<IEnumerator> statePrimary;
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

    private bool playerWasInView;
    
    protected float? moveToX;
    protected float? moveToY;

    [HideInInspector]public int facingDirection = 1;


    void Awake()
    {
        // These are default values to be overwritten in Start of children
        stateIdle = new System.Func<IEnumerator>(() => Act_Idle());
        statePrimary = new System.Func<IEnumerator>(() => Act_Idle());
    }
    
    override public void Start()
    {
        anim = GetComponentInChildren<SpriteAnimator>();
        simulator = GetComponentInChildren<SpriteSimulator>();
        body = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        currentHealth = maxHealth;
        currentState = null; // inits the enemy's state at
        _isGrounded = false;
        _contactUp = false;
        _contactLeft = false;
        _contactRight = false;
        visionBox = new Rect(transform.position.x, transform.position.y, visionBoxSize.x, visionBoxSize.y);
        moveToX = null;
        moveToY = null;

        playerWasInView = false;
        //SetState(statePrimary);
    }

    public void Update()
    {
        // PRE-UPDATE LOGIC
        sprite.flipX = facingDirection < 0;
        visionBox.center = transform.position;     // update the center of the vision box to this pawn

        PhysicsLogic(moveToX, moveToY);
        moveToX = null;
        moveToY = null;

        // UPDATE LOGIC
        if (currentHealth <= 0)
        {
            // NOTE: leave this block and conditional as-is until we're done writing the structure of this class
        }
        else if (Invincible)
        {
            SetState(null);
            Vector2 newPos = UpdateKnockback(positionWhenHit + currentKnockback.normalized * knockbackDistance);
            moveToX = newPos.x;
            moveToY = newPos.y;
        }
        else if (visionBox.Contains(Player.Position))
        {
            if (!playerWasInView)
                SetState(statePrimary);
            playerWasInView = true;
        }
        else
        {
            if (playerWasInView)
                SetState(stateIdle);
            playerWasInView = false;
        }

        Debug.DrawLine(new Vector3(visionBox.x, visionBox.y), new Vector3(visionBox.x + visionBox.width, visionBox.y ),Color.blue);
        Debug.DrawLine(new Vector3(visionBox.x, visionBox.y), new Vector3(visionBox.x , visionBox.y + visionBox.height), Color.blue);
        Debug.DrawLine(new Vector3(visionBox.x + visionBox.width, visionBox.y + visionBox.height), new Vector3(visionBox.x + visionBox.width, visionBox.y), Color.blue);
        Debug.DrawLine(new Vector3(visionBox.x + visionBox.width, visionBox.y + visionBox.height), new Vector3(visionBox.x, visionBox.y + visionBox.height), Color.blue);
    }

    private void PhysicsLogic(float? x, float? y)
    {
        Vector3 newPos = new Vector2(x == null ? transform.position.x : (float)x, y == null ? transform.position.y : (float)y);
        Vector2 motion = newPos - transform.position;

        // COLLISION LOGIC        
        HitInfo groundCheck = boxCollider2D.IsGrounded(.8F);
        HitInfo upCheck = boxCollider2D.IsHittingCeiling(.8F);
        HitInfo leftCheck = boxCollider2D.IsHittingLeft(.8F);
        HitInfo rightCheck = boxCollider2D.IsHittingRight(.8F);
        
        _isGrounded = groundCheck;
        if (!_isGrounded)
            motion += Vector2.down * fixedGravity;

        Vector2 pos = transform.position;       

        if (motion.y < 0)
        {
            if (_isGrounded)
            {
                float yDiff = transform.position.y - boxCollider2D.bounds.min.y;
                float contactY = groundCheck.hit.point.y + yDiff;
                pos.y = contactY;
            }
            else
            {
                pos.y += motion.y;
            }
        }
        else if (motion.y > 0)
        {        
            if (upCheck)
            {
                float yDiff = transform.position.y - boxCollider2D.bounds.max.y;
                float contactY = upCheck.hit.point.y + yDiff;
                pos.y = contactY;
            }
            else
                pos.y += motion.y;
        }


        if (motion.x < 0)
        {
            if (leftCheck)
            {
                float xDiff = transform.position.x - boxCollider2D.bounds.min.x;
                float contactX = groundCheck.hit.point.x + xDiff;
                pos.x = contactX;
            }
            else
                pos.x += motion.x;
        }
        else if (motion.x > 0)
        {
            if (rightCheck)
            {
                float xDiff = transform.position.x - boxCollider2D.bounds.max.x;
                float contactX = groundCheck.hit.point.x - xDiff;
                pos.x  = contactX;
            }
            else
                pos.x += motion.x;
        }

        body.MovePosition(pos);
    }

    void FixedUpdate()
    {
        if (Invincible)
            sprite.color = sprite.color.a == 0 ? Color.white : new Color(0,0,0,0);
        else            
            sprite.color = Color.white;
    }

    public void SetState(System.Func<IEnumerator> state)
    {
        if (currentState != null)
            StopCoroutine(currentState);
        currentState = null;
        if (state != null)
            currentState = StartCoroutine(state());
    }

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
        Vector2 upness = Vector2.up * .25F;
        if (moveToPoint.y < positionWhenHit.y)
            upness = Vector2.zero;
        return positionWhenHit + (moveToPoint + upness - positionWhenHit) * InvProgress;
    }

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

    /*========================*\
    |*--ENEMY ACTION TOOLSET--*|
    \*========================*/

    protected IEnumerator MoveTowardPositionX(float x, float duration)
    {
        Timer moveTimer = Timer.Set(duration);
        float startX = transform.position.x;

        while (!moveTimer.Done)
        {
            moveToX = startX + (x - startX) * moveTimer.Progress;
            yield return new WaitForEndOfFrame();
        }
        
        yield break;
    }

    protected IEnumerator MoveTowardPositionY(float y, float duration)
    {
        Timer moveTimer = Timer.Set(duration);
        float startY = transform.position.y;

        while (!moveTimer.Done)
        {
            moveToX = startY + (y - startY) * moveTimer.Progress;
            yield return new WaitForEndOfFrame();
        }
        
        yield break;
    }

    protected IEnumerator Act_Inching()
    {
        while (currentHealth > 0)
        {
            yield return MoveTowardPositionX(transform.position.x - 1, .5F);
            yield return new WaitForSeconds(.2F);
        }

        currentState = null;
        yield break;
    }

    protected IEnumerator Act_Idle()
    {
        anim.PlayDefault();
        yield return new WaitUntil(() => currentState == null);
        currentState = null;
        yield break;
    }
}
