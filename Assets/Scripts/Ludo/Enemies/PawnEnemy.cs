using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EState {None, Idle, Primary, Secondary};

public abstract class PawnEnemy : Pawn
{
    // INSPECTOR DEFINED VALUES
    public int maxHealth = 1;                           // amount of health enemy starts with
    public float invincibilityTime = .3F;               // time in seconds enemy is invincible
    public float knockbackTime = .3F;                   // time in seconds enemy gets knocked back   
    public float defaultStunTime = .1F;
    public float startFriction = .05F;
    public float stopFriction = .05F;
    public float weight = 3;                            // the value that determines how much knockback force moves it
    public float maxFallSpeed = 12;   
    public Vector2 visionBoxSize = new Vector2(10, 5);  // the bounds that a player must be inside to trigger non-idle state
    public bool usesHiddenPlatform = false;
    public int initFacingDirection = 1;

    protected System.Func<bool> knockbackResetCondition;


    // CORE VARIABLES
    protected int currentHealth;                        // amount of health enemy currently has

    // COMPONENT REFERENCES
    protected Rigidbody2D body;
    protected BoxCollider2D boxCollider2D;
    protected SpriteRenderer sprite;
    protected SpriteAnimator anim;
    protected SpriteSimulator simulator;

    // ENGINEERING VARIABLES
    protected EState currentState;
    
    protected Dictionary<EState, System.Func<IEnumerator>> states;
    protected float KnockbackProgress => Mathf.Min(1, knockbackTimer.progress);
    private Rect visionBox;
    private Timer invTimer;
    private Timer knockbackTimer;
    private Timer stunTimer;
    public bool Invincible => (invTimer != null && !invTimer.done);
    protected Player playerTarget => Player.Instance;
    private Vector2 currentKnockback;                   // direction of the force of an attack that damages this pawn
    protected Vector2 positionWhenHit;                  // position in world space when the enemy was hit
    
    
    private float stunDuration;


    private bool _isGrounded;
    private bool _contactLeft;
    private bool _contactRight;
    private bool _contactUp;
    public bool IsGrounded => _isGrounded;

    private bool playerWasInView;

    [HideInInspector]public int facingDirection;

    bool wasKnockingBack;


    void Awake()
    {
        knockbackResetCondition = () => IsGrounded;

        states = new Dictionary<EState, System.Func<IEnumerator>>();

        // These are default values to be overwritten in Start of children        
        DefState(EState.Idle, () => Act_Idle(EState.Idle));
    }
    
    override public void Start()
    {
        anim = GetComponentInChildren<SpriteAnimator>();
        simulator = GetComponentInChildren<SpriteSimulator>();
        body = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        currentHealth = maxHealth;
        _isGrounded = false;
        _contactUp = false;
        _contactLeft = false;
        _contactRight = false;
        visionBox = new Rect(transform.position.x, transform.position.y, visionBoxSize.x, visionBoxSize.y);

        playerWasInView = false;
        wasKnockingBack = false;
        facingDirection = initFacingDirection;
        stunDuration = defaultStunTime;

        knockbackTimer = null;
        invTimer = null;
        stunTimer= null;

        SetState(EState.Idle);
    }

    public void Update()
    {
        // PRE-UPDATE LOGIC
        sprite.flipX = facingDirection < 0;
        visionBox.center = transform.position;     // update the center of the vision box to this pawn

        // UPDATE LOGIC
        if (currentHealth <= 0)
        {
            // NOTE: leave this block and conditional as-is until we're done writing the structure of this class
        }
        else if (knockbackTimer != null && knockbackTimer.active)
        {
            SetState(EState.None);
            body.velocity = currentKnockback;            
            wasKnockingBack = true;
        }
        else
        {
            if (visionBox.Contains(Player.Position) && states.ContainsKey(EState.Primary))   // normal behavior loop
            {
                if (currentState != EState.Primary)   // this could be a problem if switching from a state not covered by this condition check
                    SetState(EState.Primary);
                playerWasInView = true;
            }
            else
            {
                if (currentState != EState.Idle)    // this could be a problem if switching from a state not covered by this condition check
                    SetState(EState.Idle);
                playerWasInView = false;
            }

            wasKnockingBack = false;
        }

        Debug.DrawLine(new Vector3(visionBox.x, visionBox.y), new Vector3(visionBox.x + visionBox.width, visionBox.y ),Color.blue);
        Debug.DrawLine(new Vector3(visionBox.x, visionBox.y), new Vector3(visionBox.x , visionBox.y + visionBox.height), Color.blue);
        Debug.DrawLine(new Vector3(visionBox.x + visionBox.width, visionBox.y + visionBox.height), new Vector3(visionBox.x + visionBox.width, visionBox.y), Color.blue);
        Debug.DrawLine(new Vector3(visionBox.x + visionBox.width, visionBox.y + visionBox.height), new Vector3(visionBox.x, visionBox.y + visionBox.height), Color.blue);


        PhysicsPass();
    }

    private void PhysicsPass()
    {
        HitInfo groundCheck = boxCollider2D.IsGrounded(.99F);
        HitInfo upCheck = boxCollider2D.IsHittingCeiling(.8F);
        HitInfo leftCheck = boxCollider2D.IsHittingLeft(.8F);
        HitInfo rightCheck = boxCollider2D.IsHittingRight(.8F);

        _contactLeft = leftCheck;
        _contactRight = rightCheck;
        _contactUp = upCheck;
        _isGrounded = groundCheck;

        if (leftCheck)
        {
            if (body.velocity.x < 0)
                body.velocity = new Vector2(0, body.velocity.y);

            if (currentKnockback.x < 0)
                currentKnockback.x /= -2F;
        }

        if (rightCheck)
        {
            if (body.velocity.x > 0)
                body.velocity = new Vector2(0, body.velocity.y);
            if (currentKnockback.x > 0)
                currentKnockback.x /= -2F;
        }

        if (upCheck)
        {
            if (body.velocity.y > 0)
                body.velocity = new Vector2(body.velocity.x, 0);
            if (currentKnockback.y > 0)
                currentKnockback.y /= -2F;
        }

        if (groundCheck.layerName == "Ground")
        {
            if (body.velocity.y < 0)
                body.velocity = new Vector2(body.velocity.x, 0);

            if (currentKnockback.y < 0)
                currentKnockback.y /= 2F;
        }

        if (groundCheck.layerName == "TwoWayPlatform")
        {
            if (body.velocity.y < 0 && usesHiddenPlatform && groundCheck.hit.collider.CompareTag("Rockwall"))
                body.velocity = new Vector2(body.velocity.x, 0);

            if (currentKnockback.y < 0)
                currentKnockback.y /= 2F;
        }

        // friction
        float newVelX = body.velocity.x;
        newVelX -= Mathf.Sign(newVelX) * stopFriction * Game.relativeTime;
        if (Mathf.Sign(newVelX) == Mathf.Sign(body.velocity.x)) // checking the sign so the friction doesn't reverse movement direction
            body.velocity = new Vector2(newVelX, body.velocity.y);
        else
            body.velocity = new Vector2(0, body.velocity.y);

        // movement restrictions
        if (body.velocity.y < - maxFallSpeed)
            body.velocity = new Vector2(body.velocity.x, -maxFallSpeed);
    }

    void FixedUpdate()
    {
        if (Invincible && currentHealth > 0)
            sprite.color = sprite.color.a == 0 ? Color.white : new Color(0,0,0,0);
        else            
            sprite.color = Color.white;
    }

    public void DefState(EState stateID, System.Func<IEnumerator> loop)
    {
        if (states.ContainsKey(stateID))
            states.Remove(stateID);

        states.Add(stateID, loop);
    }

    public void SetState(EState stateID)
    {
        if (!gameObject.activeSelf)  return;

        if (stateID != EState.None && currentHealth > 0)
        {
            StartCoroutine(states[stateID]());
        }

        currentState = stateID;
    }

    public bool TakeDamage(Vector2 force, float stunFactor = 1)
    {
        if (Invincible)  return false;          // ignore this call if the enemy is already invincible

        currentKnockback = GetKnockback(force * Mathf.Max(0, 10 - weight));
        facingDirection = (int)Mathf.Sign(-force.x);
        if (_isGrounded && force.y < 0)
            currentKnockback.y = 0;
        
        positionWhenHit = transform.position;
        stunDuration = defaultStunTime * stunFactor;
        currentHealth--;

        if (currentHealth <= 0)
        {
            StopAllCoroutines();
            StartCoroutine(DeathSequence());
            return true;
        }

        invTimer = Timer.Set(invincibilityTime);

        if (knockbackTimer != null && knockbackTimer.active)
            knockbackTimer.Cancel();

        SetState(EState.None);

        knockbackTimer = Timer.Set(knockbackTime, () => {
            currentKnockback = Vector2.zero;
        });

        OnHit();
        return true;
    }

    /*================================*\
    |*  OVERRIDABLE UPDATE FUNCTIONS  *|
    \*================================*/

    protected virtual void OnHit() {}      // called first frame of being hit

       
    protected virtual Vector2 GetKnockback(Vector2 velocity)
    {
        Vector2 upness = Vector2.up * Mathf.Max(0, 10 - weight);

        if (body.gravityScale == 0)
            upness = Vector2.zero;

        return velocity + upness;
    }

    protected virtual IEnumerator DeathSequence()
    {
        float timeStamp = Time.time;
        int shakeTimes = 3;

        for (int i = shakeTimes; i > 0; i--)
        {
            yield return new WaitForEndOfFrame();
            Game.VertShake(2);
            Game.FreezeFrame(Game.FRAME_TIME);
            yield return null;
        }
        yield return new WaitUntil(() => Time.time - timeStamp > .5F);

                
        gameObject.SetActive(false);
        yield break;
    }

    /*=======================*\
    |*  COLLISION FUNCTIONS  *|
    \*=======================*/

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("PlayerTarget") && !Invincible)
        {
            Player.Instance.TakeDamage();
        }
    }

    /*========================*\
    |*  ENEMY ACTION TOOLSET  *|
    \*========================*/

    protected IEnumerator Act_Inching(EState stateID, float xSpeed, float moveDuration, float pauseDuration, System.Action onMove = null, System.Action onStill = null)
    {
        float timeStamp = Time.time;
        bool isMoving = true;
        float duration = moveDuration;

        while (currentState == stateID)
        {
            if (IsGrounded)
            {
                bool lastIsMoving = isMoving;


                int lastFacingDirection = facingDirection;

                if (_contactLeft)
                    facingDirection = 1;
                else if (_contactRight)
                    facingDirection = -1;    

                if (facingDirection != lastFacingDirection)
                {
                    isMoving = true;
                    timeStamp = Time.time;
                    duration = moveDuration / 2;
                }
                
                if (isMoving)
                {
                    if (Time.time - timeStamp >= duration)
                    {
                        isMoving = false;
                        timeStamp = Time.time;
                        duration = pauseDuration;
                    }                        
                    
                    body.velocity = new Vector2(xSpeed * facingDirection, body.velocity.y);                    
                }
                else
                {
                    if (Time.time - timeStamp >= duration)
                    {
                        isMoving = true;
                        timeStamp = Time.time;
                        duration = moveDuration;
                    }
                }



                if (Mathf.Abs(body.velocity.x) > 1 && onMove != null)
                    onMove();
                else if (onStill != null)
                    onStill();
            }
            yield return new WaitForFixedUpdate();
            yield return null;

            if (currentState != stateID)
                yield break;
        }

        yield break;
    }

    protected IEnumerator Act_Idle(EState stateID)
    {
        anim.PlayDefault();
        yield return new WaitUntil(() => currentState != stateID);
        yield break;
    }
}
