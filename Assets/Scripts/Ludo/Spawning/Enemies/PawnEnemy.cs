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
    public Vector2 visionBoxOffset = new Vector2(0, 0);
    public bool usesRockwalls = false;
    public int initFacingDirection = 1;

    private Vector2 separatorForce;

    protected System.Func<bool> knockbackResetCondition;
    
    public BoxCollider2D ledgeDetector;                 // the trigger collider that is used to detect ledges and turn around


    // CORE VARIABLES
    protected int currentHealth;                        // amount of health enemy currently has

    // COMPONENT REFERENCES
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
    private Vector2 currentKnockback;                   // direction of the force of an attack that damages this pawn
    protected Vector2 positionWhenHit;                  // position in world space when the enemy was hit
    
    
    private float stunDuration;


    private bool _isGrounded;
    private bool _contactLeft;
    private bool _contactRight;
    private bool _contactUp;
    private bool _contactSlope;
    public bool IsGrounded => _isGrounded;

    private bool playerWasInView;

    protected bool isStunned;

    protected bool frictionActive;

    bool wasKnockingBack;

    protected Coroutine currentSequence;

    private Timer playerAwarenessTimer;
    public float playerAwarenessDuration = 5;       // amount of time before the enemy forgets they saw the player

    private bool atLedge;


    override public void Awake()
    {
        base.Awake();
        collidableTags.Add("Ground");
        collidableTags.Add("Hidden");
        collidableTags.Add("TwoWayPlatform");
        
        knockbackResetCondition = () => IsGrounded;

        states = new Dictionary<EState, System.Func<IEnumerator>>();

        // These are default values to be overwritten in Start of children        
        DefState(EState.Idle, () => Act_Idle(EState.Idle));
    }
    
    override public void Start()
    {
        anim = GetComponentInChildren<SpriteAnimator>();
        simulator = GetComponentInChildren<SpriteSimulator>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        currentHealth = maxHealth;
        _isGrounded = false;
        _contactUp = false;
        _contactLeft = false;
        _contactRight = false;
        _contactSlope = false;

        playerWasInView = false;
        wasKnockingBack = false;
        facingDirection = initFacingDirection;
        stunDuration = defaultStunTime;
        separatorForce = Vector2.zero;

        knockbackTimer = null;
        playerAwarenessTimer = null;
        invTimer = null;
        stunTimer = null;
        isStunned = false;

        frictionActive = true;
        atLedge = false;

        gameObject.layer = LayerMask.NameToLayer("Enemy");

        StopAllCoroutines();

        currentSequence = null;
        visionBox = new Rect(transform.position.x, transform.position.y, visionBoxSize.x, visionBoxSize.y);

        if (states.ContainsKey(EState.Idle))
            SetState(EState.Idle);
        base.Start();
    }

    override protected void ActionUpdate()
    {
        // PRE-UPDATE LOGIC
        visionBox.center = (Vector2)transform.position + Vector2.right * visionBoxOffset.x * facingDirection + Vector2.up * visionBoxOffset.y;     // update the center of the vision box to this pawn

        // UPDATE LOGIC
        if (Pawn.skitMode)
        {
            SetState(EState.None);
            if (playerAwarenessTimer)
                playerAwarenessTimer.Cancel();
            playerWasInView = true;
            body.velocity = Vector2.zero;
            // tofo: handle skits
        }
        else if (currentHealth <= 0)
        {
            // NOTE: leave this block and conditional as-is until we're done writing the structure of this class
        }
        else if (knockbackTimer)
        {
            SetState(EState.None);
            body.velocity = currentKnockback;            
            wasKnockingBack = true;
            isStunned = true;
        }
        else if (wasKnockingBack)
        {
            if (knockbackResetCondition())
            {
                ApplyFriction(2);   // extra friction
                if (!stunTimer)
                {
                    currentKnockback = Vector2.zero;
                    stunTimer = Timer.Set(stunDuration, () => {
                        if (PrimaryStateCondition())   // normal behavior loop
                        {   // this could be a problem if switching from a state not covered by this condition check
                            if (currentState != EState.Primary)
                            {
                                SetState(EState.Primary);
                                SetAware();
                            }
                            playerWasInView = true;
                        }
                        else if (states.ContainsKey(EState.Idle))
                        {
                            if (currentState != EState.Idle)    // this could be a problem if switching from a state not covered by this condition check
                            {
                                SetState(EState.Idle);
                            }
                            playerWasInView = false;
                        }
                        isStunned = false;
                        wasKnockingBack = false;
                    });
                }
            }       
        }
        else if (!isStunned)
        {            
            if (PrimaryStateCondition())
            {
                if (currentState != EState.Primary)
                {
                    SetState(EState.Primary);
                    SetAware();
                }
                playerWasInView = true;
            }
            else if (states.ContainsKey(EState.Idle))
            {
                if (currentState != EState.Idle)
                {
                    SetState(EState.Idle);
                }
                playerWasInView = false;
            }
        }

        Debug.DrawLine(new Vector3(visionBox.x, visionBox.y), new Vector3(visionBox.x + visionBox.width, visionBox.y ),Color.blue);
        Debug.DrawLine(new Vector3(visionBox.x, visionBox.y), new Vector3(visionBox.x , visionBox.y + visionBox.height), Color.blue);
        Debug.DrawLine(new Vector3(visionBox.x + visionBox.width, visionBox.y + visionBox.height), new Vector3(visionBox.x + visionBox.width, visionBox.y), Color.blue);
        Debug.DrawLine(new Vector3(visionBox.x + visionBox.width, visionBox.y + visionBox.height), new Vector3(visionBox.x, visionBox.y + visionBox.height), Color.blue);

        base.ActionUpdate();
    }

    protected bool PrimaryStateCondition() => states.ContainsKey(EState.Primary) && (visionBox.Contains(Player.Position) || playerAwarenessTimer);

    protected void SetAware()
    {
        if (playerAwarenessTimer)
            playerAwarenessTimer.Cancel();
        playerAwarenessTimer = Timer.Set(playerAwarenessDuration, () => {
            if (currentState != EState.Idle && states.ContainsKey(EState.Idle))
                SetState(EState.Idle);
        });
    }

    override protected void PhysicsPass()
    {
        base.PhysicsPass();

        HitInfo groundCheck = boxCollider.IsGrounded(1, collidableTags.ToArray());
        HitInfo ceilingCheck = CeilingCheck(collidableTags);
        HitInfo leftCheck = LeftCheck(collidableTags);
        HitInfo rightCheck = RightCheck(collidableTags);
        HitInfo slopeCheck = SlopeCheck(collidableTags);

        if (ledgeDetector)
            atLedge = IsGrounded && !ledgeDetector.IsGrounded(1, new[]{"Ground", "TwoWayPlatform", "Hidden"});

        _contactLeft = leftCheck;
        _contactRight = rightCheck;
        _contactUp = ceilingCheck;
        _contactSlope = slopeCheck;
        _isGrounded = groundCheck || slopeCheck;

        float vx = body.velocity.x;
        float vy = body.velocity.y;

        if (Mathf.Abs(vx) < Mathf.Abs(separatorForce.x))
            vx = separatorForce.x;
        if (Mathf.Abs(vy) < Mathf.Abs(separatorForce.y))
            vy = separatorForce.y;

        body.velocity = new Vector2(vx, vy);

        if (leftCheck)
        {
            if (body.velocity.x < 0)
                body.velocity = new Vector2(0, body.velocity.y);

            if (currentKnockback.x < 0)
            {
                currentKnockback.x *= -.5F;
            }
        }

        if (rightCheck)
        {
            if (body.velocity.x > 0)
                body.velocity = new Vector2(0, body.velocity.y);
            if (currentKnockback.x > 0)
            {
                currentKnockback.x *= -.5F;
            }
        }

        if (ceilingCheck)
        {
            if (body.velocity.y > 0)
                body.velocity = new Vector2(body.velocity.x, 0);
            if (currentKnockback.y > 0)
            {
                currentKnockback.y *= -.5F;
            }
        }

        if (groundCheck.layerName == "Ground")
        {
            if (body.velocity.y < 0)
                body.velocity = new Vector2(body.velocity.x, 0);

            if (currentKnockback.y < 0)
                currentKnockback.y *= -.5F;
        }

        if (groundCheck.layerName == "TwoWayPlatform")
        {
            if (usesRockwalls || !groundCheck.collider.CompareTag("Rockwall"))
            {
                if (body.velocity.y < 0)
                    body.velocity = new Vector2(body.velocity.x, 0);

                if (currentKnockback.y < 0)
                    currentKnockback.y *= -.5F;
            }
            else if (gameObject.layer == LayerMask.NameToLayer("Enemy"))    // if this object isn't supposed to colleide with rockwalls, temporarily change layer to ignore collision with it
            {
                gameObject.layer = LayerMask.NameToLayer("EnemyBypass");
                _isGrounded = false;
                Timer.Set(.1F, () => {
                    gameObject.layer = LayerMask.NameToLayer("Enemy");
                });
            }
        }

        // friction
        ApplyFriction(1);

        // movement restrictions
        if (body.velocity.y < - maxFallSpeed)
            body.velocity = new Vector2(body.velocity.x, -maxFallSpeed);
    }

    void FixedUpdate()
    {
        if (Invincible && currentHealth > 0)
            anim.ToggleVisible();
        else            
            anim.SetVisible(true);
    }

    public void DefState(EState stateID, System.Func<IEnumerator> loop)
    {
        if (states.ContainsKey(stateID))
            states.Remove(stateID);

        states.Add(stateID, loop);
    }

    public void ApplyFriction(float frictionFactor)
    {
        if (!frictionActive)    return;

        float newVelX = body.velocity.x;
        newVelX -= Mathf.Sign(newVelX) * stopFriction * frictionFactor * Game.relativeTime;
        if (Mathf.Sign(newVelX) == Mathf.Sign(body.velocity.x)) // checking the sign so the friction doesn't reverse movement direction
            body.velocity = new Vector2(newVelX, body.velocity.y);
        else
            body.velocity = new Vector2(0, body.velocity.y);
    }

    public void SetState(EState stateID)
    {
        if (!gameObject.activeSelf)  return;

        if (currentSequence != null)
        {
            StopCoroutine(currentSequence);
            currentSequence = null;
        }

        
        if (stateID != EState.None && currentHealth > 0)
        {
            if (states.ContainsKey(stateID))
                currentSequence = StartCoroutine(states[stateID]());
            else
                Debug.LogError("Cannot set Enemy State to " + stateID + ", as it has not been defined for this subclass");
        }


        currentState = stateID;
    }

    public bool InflictDamage(Vector2 force, int damage, float stunFactor)
    {        
        if (Invincible)  return false;          // ignore this call if the enemy is already invincible

        currentKnockback = GetKnockback(force * Mathf.Max(0, 10 - weight));
        facingDirection = (int)Mathf.Sign(-force.x);
        if (_isGrounded && force.y < 0)
            currentKnockback.y = 0;
        
        positionWhenHit = transform.position;
        stunDuration = defaultStunTime * stunFactor;
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            StopAllCoroutines();
            StartCoroutine(DeathSequence());
            return true;
        }

        SetState(EState.None);      
        invTimer = Timer.Set(invincibilityTime);
        SetAware();
        

        if (knockbackTimer != null && knockbackTimer.active)
        {
            knockbackTimer.Cancel();
        }

        knockbackTimer = Timer.Set(knockbackTime, () => {
            currentKnockback = Vector2.zero;
        });


        OnHurt();        
        return true;
    }

    public void RedirectKnockback(Vector2 direction)
    {
        if (!Player.redirectAttacks)    return;

        if (direction.magnitude > 0)
        {
            if (!_isGrounded || direction.y >= 0)
                currentKnockback = direction.normalized * currentKnockback.magnitude;
        }
    }

    /*================================*\
    |*  OVERRIDABLE UPDATE FUNCTIONS  *|
    \*================================*/

    protected virtual void OnHurt() {}      // called first frame of being hit
       
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
        if (other.CompareTag("PlayerTarget") && !Invincible && currentHealth > 0)
        {
            Player.Instance.InflictDamage(1);
        }
    }

    override protected void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Vector2 diff = transform.position - other.transform.position;
            separatorForce.x = diff.x * 2.0F;
            if (Mathf.Abs(separatorForce.x) < 4)
                separatorForce.x = Mathf.Sign(separatorForce.x) * 4;
            separatorForce.y = diff.y * 6.0F;
        }

        base.OnCollisionStay2D(other);
    }

    override protected void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            separatorForce = Vector2.zero;
        }

        base.OnCollisionExit2D(other);
    }

    /*========================*\
    |*  ENEMY ACTION TOOLSET  *|
    \*========================*/

    protected IEnumerator Act_Walk(EState stateID, float xSpeed)
    {
        frictionActive = false;

        while (currentState == EState.Idle)
        {
            body.velocity = new Vector2(xSpeed * facingDirection, body.velocity.y);
            yield return new WaitUntil(() => (_contactLeft && facingDirection < 0 ||
                _contactRight && facingDirection > 0) ||
                atLedge);
            facingDirection = -facingDirection;
        }

        yield return null;
    }

    protected IEnumerator Act_WalkTowardPlayer(EState stateID, float xSpeed, float updateDelay)
    {
        frictionActive = false;
        while (currentState == EState.Primary)
        {
            facingDirection = (int)Mathf.Sign(Player.Position.x - transform.position.x);
            body.velocity = new Vector2(xSpeed * facingDirection, body.velocity.y);
            yield return new WaitForSeconds(updateDelay);   
            yield return null;
        }       

        yield return null;
    }
    
    protected IEnumerator Act_Inching(EState stateID, float xSpeed, float moveDuration, float pauseDuration, System.Action onMove = null, System.Action onStill = null)
    {
        float timeStamp = Time.time;
        bool isMoving = true;
        float duration = moveDuration;
        bool wasGrounded = IsGrounded;
        frictionActive = true;

        while (currentState == stateID)
        {
            if (IsGrounded)
            {
                if (_contactLeft)
                    facingDirection = 1;
                else if (_contactRight)
                    facingDirection = -1;

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

            wasGrounded = IsGrounded;
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
