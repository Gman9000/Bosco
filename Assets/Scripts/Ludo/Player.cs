using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PState {Unassigned, 
    Idle, Walk, Run, Duck,
Jump, Fall, Sliding,
G_AtkSide1, G_AtkSide2, G_AtkTwist, G_AtkUp, G_AtkLow,
A_AtkUp, A_AtkSide, A_AtkDown, A_AtkNeutral,
G_Rockclimb, G_RockclimbShimmy,
LadderIdle, LadderMove,
FenceIdle, FenceMove,
}

public class Player : Pawn
{
    public static bool IsHurting => Instance.isHurting;
    public static bool IsInvincible => Instance.invTimer;
    public static int Hp => Instance.currentHealth;
    public static int FacingDirection => (int)Instance.facingDirection;
    public static Vector3 Position => Instance.transform.position;

    public static readonly bool redirectAttacks = false;        // a variable that toggles the mechanic idea of trajectory redirection

    public readonly List<PState> AttackStates = new List<PState>(){
        PState.G_AtkSide1,
        PState.G_AtkSide2,
        PState.G_AtkTwist,
        PState.G_AtkUp,
        PState.G_AtkLow,
        PState.A_AtkDown,
        PState.A_AtkUp,
        PState.A_AtkNeutral
    };

    public AudioClip[] sfx_swordSounds;
    public AudioClip sfx_jump;
    public AudioClip sfx_hurt;
    public static Player Instance { get; protected set;}
    public float moveSpeed;
    public float climbSpeed;
    public float runSpeed;
    public float startFriction = 1;
    public float stopFriction = 1;
    public float startFrictionAir = .4F;
    public float stopFrictionAir = .4F;
    public float maxFallSpeed = .1F;
    public float hurtDuration = 1.2F;

    private PState _state;
    private PState lastState;
    private string animationOverride;

    public PState state {
        get => _state;
        private set
        {
            _state = value;
        }
    }

    private PState lastAtk;
    
    //for jumping
    public float jumpSpeed;
    public float jumpTime;

    public float earlyJumpWindow = Game.FRAME_TIME * 2;
    private float jumpTimeCountdown;
    private bool isJumping;
    public float originalGravityScale;
    public float shimmySpeed = 2;
    public float rockJumpSpeed = 5;
    public float rayCastMagnitude;

    private bool inputMovePaused = false;
    private bool inputAttackPaused = false;

    private HitInfo activeFloorHit;

    public static bool IsMovesetLocked(PState move) => _movesetLocks.Contains(move);
    private static List<PState> _movesetLocks = new List<PState>() {
        PState.G_AtkSide2,
        PState.A_AtkUp,
        PState.A_AtkDown,
        PState.G_AtkTwist,
        PState.G_Rockclimb
    };

    public AtkboxDictionary attackBoxes;

    private bool isHitting = false;
    public float hitTime;
    public float hitCooldown;               // should be less than hit time
    public float hitComboWindow;            // should be less than hit time

    public float doubleTapWindow = .1F;     // the time window in seconds for any double-tap input

    private bool jumpInput;

    private bool bypassCollisionDown;
    
    private float hitTimeStamp = 0;         // the time when the player attacked // needs to be redone to be modular and good-feeling and knockback logic etc
    private float comboTimeStamp = 0;       // the time when the player attacked

    //health stuff
    public int maxHealth = 5;
    private int currentHealth;

    Coroutine doAttackCoroutine;
    Coroutine pausePhysicsCoroutine;
    Coroutine pauseInputMoveCoroutine;
    Coroutine pauseInputAttackCoroutine;
    Coroutine attackSequenceCoroutine;
    public float deathTime;

    private Vector3 checkpoint;

    public bool IsDownSlash => IsAtkActive(PState.A_AtkDown);

    public bool isGrounded => _isGrounded;
    bool _isGrounded;
    bool _isSloping;
    bool isHittingCeiling;
    bool isHittingRightWall;
    bool isHittingLeftWall;
    // Start is called before the first frame update

    bool canAerialAttack;
    bool PhysicsPaused => body.constraints == RigidbodyConstraints2D.FreezeAll;
    private Timer invTimer;
    private bool isHurting;
    private Timer jumpTimer;
    private Timer earlyJumpTimer;
    private Timer fallthroughTimer;

    private bool slopeDropPrep;

    private bool runMode;
    public static bool aimMode => Instance.hittingEnemyScript != null;
    [HideInInspector]public PawnEnemy hittingEnemyScript;
    private Timer stopRunningTimer;


    public bool climbMode => state == PState.FenceIdle || state == PState.FenceMove ||
        state == PState.LadderIdle || state == PState.LadderMove;
    bool canLadder;
    bool canFence;
    float ladderX;

    [HideInInspector]public bool onRockwall;

    override public void Awake()
    {
        base.Awake();
        collidableTags.Add("Ground");
        collidableTags.Add("Hidden");
        collidableTags.Add("TwoWayPlatform");
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"));
    }

    override public void Start()
    {
        base.Start();
        jumpInput = false;
        lastState = PState.Unassigned;
        canLadder = false;
        canFence = false;
        currentHealth = maxHealth;
        checkpoint = this.gameObject.transform.position;
        Instance = this;
        isHurting = false;
        state = PState.Idle;
        lastAtk = PState.Unassigned;
        canAerialAttack = false;
        inputAttackPaused = false;
        inputMovePaused = false;
        onRockwall = false;
        animationOverride = null;

        slopeDropPrep = false;

        bypassCollisionDown = false;
        boxCollider.enabled = true;
        body.gravityScale = originalGravityScale;
        body.velocity = Vector2.zero;

        DeactivateAllAtkboxes();

        animator.PlayDefault();

        body.constraints = RigidbodyConstraints2D.FreezeRotation;

        activeFloorHit = new HitInfo();

        StopAllCoroutines();

        doAttackCoroutine = null;
        pausePhysicsCoroutine = null;
        pauseInputMoveCoroutine = null;
        pauseInputAttackCoroutine = null;
        attackSequenceCoroutine = null;

        runMode = false;
        hittingEnemyScript = null;

        if (invTimer)
            invTimer.Cancel();
        if (jumpTimer)
            jumpTimer.Cancel();
        if (stopRunningTimer)
            stopRunningTimer.Cancel();
        if (earlyJumpTimer)
            earlyJumpTimer.Cancel();
        if (fallthroughTimer)
            fallthroughTimer.Cancel();

        animator.SetVisible(true);
    }

    override protected void ActionUpdate()
    {
        if (skitMode)
        {
            ApplyStopFriction(stopFriction);
            state = PState.Idle;
            body.gravityScale = originalGravityScale;
            
            Animate();
            
            return;
        }

        if (!Game.gameStarted)  return;
        if (Game.isPaused)  return;        


        if (aimMode)
        {
            hittingEnemyScript.RedirectKnockback(PlayerInput.GetVectorDiagonal());
        }       

        bool wasGrounded = _isGrounded || _isSloping;
        bool wasSloping = _isSloping;

        HitInfo groundHit = GroundCheck(collidableTags);
        HitInfo slopeHit = SlopeCheck(collidableTags);

        // miscontact correction //
        if (wasSloping && !slopeHit && nearestGroundHit.normal.x != 0 && nearestGroundHit.normal.y != 0)
        {
            float groundDistance = nearestGroundHit.contact.y - boxCollider.Bottom();
            float minSnapDistance = Game.PIXEL;
            if (groundDistance >= 0 && groundDistance <= minSnapDistance)
            {
                body.MovePosition(new Vector2(body.position.x, groundDistance));
                slopeHit = nearestGroundHit;
            }
        }

        activeFloorHit = slopeHit ? slopeHit : groundHit;
        _isGrounded = activeFloorHit;
        _isSloping = slopeHit;

        // if prepped for the slope drop edge case (to conform to the slope when going from a slope of 0 to a negative slope)
        if (slopeDropPrep)
        {
            // if the player is no longer grounded AND it's time to slope drop
            if (wasGrounded && !_isGrounded && nearestGroundHit && false)
            {
                // snap to the nearest downward ground
                Vector2 contactPointWee = nearestGroundHit.contact + nearestGroundHit.normal * boxCollider.edgeRadius;
                body.MovePosition(contactPointWee);
                Debug.Log(" bingo");
                slopeHit = activeFloorHit = nearestGroundHit;
                _isGrounded = true;
                _isSloping = true;
                slopeDropPrep = false;
            }
        }


        if (PlayerInput.Pressed(Button.A))
        {
            if (earlyJumpTimer)
                earlyJumpTimer.Cancel();
            else
            {
                jumpInput = true;
                earlyJumpTimer = Timer.Set(earlyJumpWindow, () => jumpInput = false);
            }
        }
        
        if (groundHit.layerName == "TwoWayPlatform")
        {
            if (body.velocity.y <= 0)
                _isGrounded = true;
            if (bypassCollisionDown)
                _isGrounded = false;
                
            if (_isGrounded && PlayerInput.Held(Button.Down) && jumpInput)    // make shift fallthrough
            {
                _isGrounded = false;
                
                if (earlyJumpTimer)
                    earlyJumpTimer.Cancel();
                if (!fallthroughTimer)
                    fallthroughTimer = Timer.Set(.5F, () => bypassCollisionDown = false);
                bypassCollisionDown = true;
                jumpInput = false;
            }
            if (groundHit.collider.tag == "Rockwall" && !wasGrounded && !TwoWayPlatform.rockwallCondition)
                _isGrounded = false;
        }
        
        if (_isGrounded && groundHit.layerName == "TwoWayPlatform" && !wasGrounded)
        {
            float feetY = boxCollider.Bottom();
            float surfaceY = groundHit.collider.bounds.Top();
                if (surfaceY > feetY) _isGrounded = false;
        }

        isHittingCeiling = CeilingCheck(collidableTags);
        isHittingRightWall = RightCheck(collidableTags);
        isHittingLeftWall = LeftCheck(collidableTags);

        Game.debugText = activeFloorHit ? "hitty" : "nope";

        if (_isGrounded)
        {
            if (_isSloping && groundHit.tangent.x == 1)
            {
                body.velocity = new Vector2(moveSpeed * Mathf.Sign(body.velocity.x), 0);
            }

            if (jumpTimer != null && jumpTimer.done) jumpTimer.Cancel();
            
            if (state == PState.A_AtkDown)   // if is falling fast
            {
                Game.VertShake(8);  // shake screen
                DoPhysicsPause(.4F);
                attackBoxes[PState.A_AtkDown].SetActive(false);
                DoInputAttackPause(.4F);
                Timer.Set(.4F, () => state = PState.Unassigned);
            }

            body.gravityScale = originalGravityScale;

            canAerialAttack = true;
            
            if (!wasGrounded && body.velocity.y < 0)    // check if moving down
            {                
                attackBoxes[PState.A_AtkUp].SetActive(false);
                body.velocity = new Vector2(body.velocity.x, 0);
            }
        }
        else if (!climbMode)   // not grounded
        {
            if (body.velocity.y < 0 && state != PState.A_AtkDown)
                state = PState.Fall;

            if (wasGrounded)        // only set as unassigned if the player was JUST on the ground
            {
                state = PState.Unassigned;
            }
        }

        if (inputMovePaused)
        {
            if (IsAtkActive(PState.G_AtkTwist))
            {
                body.velocity = new Vector2(FacingDirection * 3.0F, body.velocity.y);
            }
            if (IsAtkActive(PState.G_AtkSide1) && _isGrounded)
            {
                if (Time.time - hitTimeStamp > .15F)
                {
                    body.velocity = new Vector2(0, body.velocity.y);
                }
            }
        }
        else
        {
            if (PlayerInput.Held(Button.Up) || PlayerInput.Held(Button.Down))
            {
                if (canLadder)
                {
                    state = PState.LadderIdle;
                    if (lastState != PState.LadderIdle)
                    {
                        transform.position = new Vector3(ladderX, transform.position.y, transform.position.z);
                    }
                }
                else if (canFence)
                {
                    state = PState.FenceIdle;
                }
            }
            
            if (state == PState.LadderIdle || state == PState.LadderMove)
            {
                body.gravityScale = 0f;
                body.velocity = new Vector2(0f, 0f);
                state = PState.LadderIdle;
                if (PlayerInput.Held(Button.Up))
                {
                    state = PState.LadderMove;
                    body.velocity = new Vector2(0f, climbSpeed);
                }
                if (PlayerInput.Held(Button.Down))
                {
                    state = PState.LadderMove;
                    body.velocity = new Vector2(0f, -climbSpeed);
                }
                if (!canLadder)
                {
                    state = PState.Unassigned;
                    body.gravityScale = originalGravityScale;
                }
            }

            if (state == PState.FenceIdle || state == PState.FenceMove)
            {
                body.gravityScale = 0f;
                body.velocity = new Vector2(body.velocity.x, 0f);
                state = PState.FenceIdle;
                if (PlayerInput.Held(Button.Up))
                {
                    state = PState.FenceMove;
                    body.velocity = new Vector2(body.velocity.x, climbSpeed);
                }
                if (PlayerInput.Held(Button.Down))
                {
                    state = PState.FenceMove;
                    body.velocity = new Vector2(body.velocity.x, -climbSpeed);
                }
                if (!canFence)
                {
                    state = PState.Unassigned;
                    body.gravityScale = originalGravityScale;
                }
            }

            if (_isGrounded &&
            ((PlayerInput.DidDoubleTap(Button.Left) && body.velocity.x < 0) ||
            (PlayerInput.DidDoubleTap(Button.Right) && body.velocity.x > 0)))
            {
                runMode = true;
                if (stopRunningTimer)
                    stopRunningTimer.Cancel();
            }
            
            if (runMode)
            {
                if (Mathf.Abs(body.velocity.x) < moveSpeed)
                {
                    if (!stopRunningTimer)
                        stopRunningTimer = Timer.Set(.3F, () => {runMode = false;});
                }
                else
                {
                    if (stopRunningTimer)
                        stopRunningTimer.Cancel();
                }
            }

            //Game.debugText =  lastActiveFloorHit ? "" + lastActiveFloorHit.normal : "";

            // SLOPE LOGIC //
            SlopeLogic(wasGrounded, wasSloping, slopeHit);

            // CORE MOVEMENT LOGIC //
            if (PlayerInput.Held(Button.Down) && _isGrounded)
            {
                if (!slopeHit)
                    ApplyStopFriction(stopFriction * .5F);

                if (state == PState.Idle || state == PState.Walk)
                    state = PState.Duck;
            }
            else if (PlayerInput.Held(Button.Left))
            {
                if (climbMode && !canLadder)
                    body.velocity = new Vector2(-climbSpeed, body.velocity.y);
                else
                    MoveWithFriction(_isGrounded ? runMode ? -startFriction * 2 : -startFriction : -startFrictionAir);
            }
            else if (PlayerInput.Held(Button.Right))
            {
                if (climbMode && !canLadder)
                    body.velocity = new Vector2(climbSpeed, body.velocity.y);
                else                    
                    MoveWithFriction(_isGrounded ? runMode ? startFriction * 2 : startFriction : startFrictionAir);
            }
            else if (!IsAtkActive(PState.G_AtkTwist))    // add friction
            {
                if (_isGrounded)
                {
                    if (!slopeHit && wasGrounded && !isJumping)
                        body.velocity = new Vector2(body.velocity.magnitude * Mathf.Sign(body.velocity.x), 0);
                    ApplyStopFriction(stopFriction);
                }
                else if (climbMode)
                    body.velocity = new Vector2(0, body.velocity.y);
                else
                    ApplyStopFriction(stopFrictionAir);
            }

            if (!PlayerInput.Held(Button.Down) && state == PState.Duck)
                state = PState.Unassigned;

            // JUMP LOGIC //
            if (_isGrounded && jumpInput)
            {
                isJumping = true;
                jumpInput = false;
                body.velocity = new Vector2(body.velocity.x, jumpSpeed);
                float startY = transform.position.y;
                jumpTimer = Timer.Set(jumpTime, () =>
                {
                    isJumping = false;
                });
                SoundSystem.PlaySfx(sfx_jump, 2);
            }

            if (isJumping && PlayerInput.Held(Button.A))
            {
                body.velocity = new Vector2(body.velocity.x, jumpSpeed);
                if (isHittingCeiling)
                {
                    if(jumpTimer != null && jumpTimer.done) jumpTimer.Cancel();
                    isJumping = false;
                }
            }

            if (PlayerInput.Released(Button.A))
            {
                isJumping = false;
                jumpInput = false;
                if (earlyJumpTimer)
                    earlyJumpTimer.Cancel();
            }
        }

        // MOVEMENT EDGE CASING //

        // > SLOPE CORNERS
        
        if (footingHit && groundHit)
        {
            if (footingHit.normal.y < activeFloorHit.normal.y && Mathf.Sign(body.velocity.x) == Mathf.Sign(footingHit.normal.x))
            {
                if (body.velocity.x != 0)
                {
                    // prep the slope drop snap
                    slopeDropPrep = true;
                    //Game.debugText = "drop snap prep";
                }
                else
                {
                    // do not delete slope drop prep if movement has stopped
                }
                //body.velocity = footingHit.tangent * multiplier * Game.relativeTime;
            }
            else
            {
                slopeDropPrep = false;
                //Game.debugText = "";
            }
        }
        else
        {
            slopeDropPrep = false;
            //Game.debugText = "";
        }

        if (onRockwall && body.velocity.x != 0)
            body.velocity = new Vector2(Mathf.Sign(body.velocity.x) * shimmySpeed, 0);

        if (state == PState.Fall && body.velocity.y < -maxFallSpeed)
            body.velocity = new Vector2(body.velocity.x, -maxFallSpeed);

        AttackInputs();

        if (state == PState.Idle || state == PState.Walk || state == PState.Run || state == PState.Jump || state == PState.Fall)
            state = PState.Unassigned;

        if (state == PState.Unassigned)        // catch-all for unassigned states
        {
            if (_isGrounded)
                SetStateFromHorizontal();
            else
                SetStateFromVertical();
        }

        if (currentHealth > 0)
            Animate();

        lastState = state;
    }

    public void ApplyStopFriction(float multiplier)
    {
        float newVelX = body.velocity.x;
        newVelX -= Mathf.Sign(newVelX) * multiplier * Game.relativeTime;
        if (Mathf.Sign(newVelX) == Mathf.Sign(body.velocity.x)) // checking the sign so the friction doesn't reverse movement direction
            body.velocity = new Vector2(newVelX, body.velocity.y);
        else
            body.velocity = new Vector2(0, body.velocity.y);
    }

    public void MoveWithSlopeFriction(float multiplier, HitInfo hit)
    {
        Vector2 tangent = hit ? hit.tangent : new Vector2(1, 0);

        float movement = body.velocity.magnitude * Mathf.Sign(body.velocity.x);
        float topSpeed = runMode ? runSpeed : moveSpeed;
        bool alreadyFaster = Mathf.Abs(movement) > topSpeed;

        Vector2 nextMovementVector = tangent * multiplier * Game.relativeTime;

        if (!alreadyFaster || Mathf.Sign(multiplier) != Mathf.Sign(body.velocity.x))  // if the object is not already at top speed, or is trying to change direction
        {
            body.velocity += nextMovementVector;     // move the object incrementally faster
        }
        
        if (multiplier > 0)
        {
            if (body.velocity.x > topSpeed && !alreadyFaster)   // cap the horizontal speed of the object if isn't already faster
                body.velocity = new Vector2(0, body.velocity.y) + tangent * topSpeed;
            transform.rotation = new Quaternion(0F, 0F, transform.rotation.z, transform.rotation.w);
        }
        else
        {
            if (body.velocity.x < -topSpeed)    // cap the horizontal speed of the object if isn't already faster
                body.velocity = new Vector2(0, body.velocity.y) - tangent * topSpeed;
            transform.rotation = new Quaternion(0F, 180F, transform.rotation.z, transform.rotation.w);
        }
    }

    public void MoveWithFriction(float multiplier)
    {
        float topSpeed = runMode ? runSpeed : moveSpeed;
        bool alreadyFaster = Mathf.Abs(body.velocity.x) > topSpeed;
        float lastVelocityX = body.velocity.x;

        Vector2 nextMovementVector = Vector2.right * multiplier * Game.relativeTime;

        if (!alreadyFaster || Mathf.Sign(multiplier) != Mathf.Sign(lastVelocityX))  // if the object is not already at top speed, or is trying to change direction
        {
            body.velocity += nextMovementVector;     // move the object incrementally faster
        }
        
        
        if (multiplier > 0)
        {
            if (body.velocity.x > topSpeed && !alreadyFaster)   // cap the horizontal speed of the object if isn't already faster
                body.velocity = new Vector2(topSpeed, body.velocity.y);
            transform.rotation = new Quaternion(0F, 0F, transform.rotation.z, transform.rotation.w);
        }
        else
        {
            if (body.velocity.x < -topSpeed)    // cap the horizontal speed of the object if isn't already faster
                body.velocity = new Vector2(-topSpeed, body.velocity.y);
            transform.rotation = new Quaternion(0F, 180F, transform.rotation.z, transform.rotation.w);
        }
    }   

    void SlopeLogic(bool wasGrounded, bool wasSloping, HitInfo slopeHit)
    {
        // main logic //
        if (slopeHit)
        {
            if (!wasGrounded)
            {
                Vector2 contactPointSlope = slopeHit.contact + slopeHit.normal * boxCollider.edgeRadius;
                body.MovePosition(contactPointSlope);
            }

            body.gravityScale = 0;

            if (slopeHit.normal.x > 0)
            {
                if (!wasSloping && !PlayerInput.Held(Button.Left) && !PlayerInput.Held(Button.Right))   // replace the current velocity with a slope-conforming velocity if the object landed this frame
                    body.velocity = slopeHit.tangent * body.velocity.magnitude;

                if (PlayerInput.Held(Button.Down))
                {
                    body.gravityScale = originalGravityScale * .75F;
                    state = PState.Sliding;
                }
                else 
                {
                    state = PState.Unassigned;
                    if (PlayerInput.Held(Button.Right))
                    {
                        body.gravityScale = originalGravityScale * 4;                        
                        MoveWithSlopeFriction(startFriction, slopeHit);
                    }
                    else if (PlayerInput.Held(Button.Left))
                    {
                        body.gravityScale = 0;
                        MoveWithSlopeFriction(-startFriction, slopeHit);
                    }
                }
            }

            if (slopeHit.normal.x < 0)
            {
                if (!wasSloping && !PlayerInput.Held(Button.Left) && !PlayerInput.Held(Button.Right))   // replace the current velocity with a slope-conforming velocity if the object landed this frame
                    body.velocity = -slopeHit.tangent * body.velocity.magnitude;

                if (PlayerInput.Held(Button.Down))
                {
                    body.gravityScale = originalGravityScale * .75F;
                    state = PState.Sliding;
                }
                else 
                {
                    state = PState.Unassigned;
                    if (PlayerInput.Held(Button.Left))
                    {
                        body.gravityScale = originalGravityScale * 4;
                        MoveWithSlopeFriction(-startFriction, slopeHit);
                    }
                    else if (PlayerInput.Held(Button.Right))
                    {
                        body.gravityScale = 0;
                        MoveWithSlopeFriction(startFriction, slopeHit);
                    }
                }
            }

            // removed because I like the natural speed a little better right now
            if (state != PState.Sliding)
            {

                float topSpeed = runMode ? runSpeed : moveSpeed;

                topSpeed *= .95F;
                
                // speed limiting slope climbing only
                if (body.velocity.magnitude > topSpeed)
                    body.velocity = body.velocity.normalized * topSpeed;
            }
        }
        else
        {
            if (wasSloping && state == PState.Sliding)
                state = PState.Unassigned;
            else
                body.gravityScale = originalGravityScale;
        }
    }

    void SetStateFromHorizontal()
    {
        if (climbMode)
        {
            if (IsAttackState())
            {
                state = canLadder ? PState.LadderIdle : PState.FenceIdle;
            }
        }
        if (body.velocity.x == 0)
        {
            state = PState.Idle;
            if (onRockwall)
                state = PState.G_Rockclimb;
        }
        else if (Mathf.Abs(body.velocity.x) <= moveSpeed || onRockwall)
        {
            state = PState.Walk;
            if (onRockwall)
                state = PState.G_RockclimbShimmy;
        }
        else
        {
            state = PState.Run;
        }
    }

    void SetStateFromVertical()
    {
        if (body.velocity.y >= 0)
            state = PState.Jump;
        else
            state = PState.Fall;
    }

    bool IsAttackState() => AttackStates.Contains(state);

    public void SnapToPixel()       // NOTE: This function created a lotta collision jank. So I removed it. It should still work but be careful when it's called
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Round(pos.x / Game.PIXEL) * Game.PIXEL;
        pos.y = Mathf.Floor(pos.y / Game.PIXEL) * Game.PIXEL;
        body.MovePosition(pos);
    }

    public void SetGrounded(bool value, string type)
    {
        _isGrounded = value;
        switch (type)
        {
            case "Rockwall":
                if (IsMovesetLocked(PState.G_Rockclimb))
                    onRockwall = value;
                break;
        }
    }

    public void SetInvTime(float seconds)
    {
        if (invTimer)
            invTimer.Cancel();
        invTimer = Timer.Set(seconds, () =>
        {
            isHurting = false;
            animator.Renderer.color = Color.white;
        });
    }

    void FixedUpdate()
    {
        if (invTimer)
            animator.Renderer.color = animator.Renderer.color.a == 0 ? Color.white : new Color(0,0,0,0);

        //Game.debugText = "HP: " + Hp;
    }

    override protected void OnCollisionStay2D(Collision2D other)
    {
        if (other.transform.childCount > 0 && other.transform.GetChild(0).CompareTag("Hidden") && other.contacts[0].normal.y > 0)
        {
            Vector3 pos = transform.position;
            pos.y = other.collider.bounds.center.y + 1;
            transform.position = pos;
        }

        base.OnCollisionStay2D(other);
    }

    void Animate()
    {
        if (IsHurting && inputMovePaused && inputAttackPaused)
            animator.Play(AnimMode.Looped, "hurt");
        else if (animationOverride != null)
        {
            animator.Play(AnimMode.Once, animationOverride, () => animationOverride = null);
        }        
        else switch (state)
        {
            case PState.Idle:
                animator.PlayDefault();
                break;
            case PState.Walk:
                animator.Play(AnimMode.Looped, "walk");
                break;
            case PState.Run:
                animator.Play(AnimMode.Looped, "run");
                break;
            case PState.Duck:
                animator.Play(AnimMode.Looped, "duck");
                break;
            case PState.G_AtkLow:
                animator.Play(AnimMode.Once, "slash 2", () => state = PState.Unassigned);
                break;
            case PState.Jump:
                animator.Play(AnimMode.Hang, "jump");
                break;
            case PState.Fall:
            case PState.Sliding:
                animator.Play(AnimMode.Hang, "fall");
                break;
                
            case PState.G_AtkSide1:
                animator.Play(AnimMode.Once, "slash 1", () => state = PState.Unassigned);
                break;
            case PState.G_AtkSide2:
                animator.Play(AnimMode.Once, "slash 2", () => state = PState.Unassigned);
                break;
            case PState.G_AtkTwist:
                animator.Play(AnimMode.Looped, "spin");
                break;
            case PState.G_AtkUp:
                animator.Play(AnimMode.Once, "upslash ground", () => state = PState.Unassigned);
                break;

            case PState.A_AtkDown:
                animator.Play(AnimMode.Hang, "downslash");
                break;
            case PState.A_AtkUp:
                animator.Play(AnimMode.Hang, "upslash air");
                break;
            case PState.A_AtkSide:
                animator.Play(AnimMode.Hang, "slash 1");
                break;

            case PState.G_Rockclimb:
                animator.Play(AnimMode.Looped, "duck");
                break;
            case PState.G_RockclimbShimmy:
                animator.Play(AnimMode.Looped, "duck");
                break;
        }
    }

    public void DoPhysicsPause(float time)
    {
        if (pausePhysicsCoroutine != null)
            StopCoroutine(pausePhysicsCoroutine);
        pausePhysicsCoroutine = StartCoroutine(PausePhysics(time));
    }

    public void DoInputMovePause(float time)
    {
        if (pauseInputMoveCoroutine != null)
            StopCoroutine(pauseInputMoveCoroutine);
        pauseInputMoveCoroutine = StartCoroutine(PauseInputMove(time));
    }

    public void DoInputAttackPause(float time)
    {
        if (pauseInputAttackCoroutine != null)
            StopCoroutine(pauseInputAttackCoroutine);
        pauseInputAttackCoroutine = StartCoroutine(PauseInputAttack(time));
    }

    public void InflictDamage(int damageAmount)
    {
        if (invTimer || currentHealth <= 0 || isHurting)    return;


        isHurting = true;
        SoundSystem.PlaySfx(sfx_hurt, 3);
        
        currentHealth--;
        DeactivateAllAtkboxes();

        if (doAttackCoroutine != null)
        {
            StopCoroutine(doAttackCoroutine);
            doAttackCoroutine = null;
        }
        if (attackSequenceCoroutine != null)
        {
            StopCoroutine(attackSequenceCoroutine);
            attackSequenceCoroutine = null;
        }

        DoInputMovePause(.25F);
        DoInputAttackPause(.25F);
        body.velocity = Vector2.up - (FacingDirection * Vector2.right * 5F);

        invTimer = Timer.Set(hurtDuration, () => {
            isHurting = false;
            animator.Renderer.color = Color.white;
        });

        if (attackSequenceCoroutine != null)
        {
            StopCoroutine(attackSequenceCoroutine);
            attackSequenceCoroutine = null;
        }

        if (doAttackCoroutine != null)
        {
            StopCoroutine(doAttackCoroutine);
            doAttackCoroutine = null;
        }

        if(currentHealth <= 0)
        {
            //for now this is death
            body.velocity = Vector2.zero;
            StopAllCoroutines();
            StartCoroutine(Die());
        }
    }

    private void AttackInputs()
    {
        if (climbMode) return;
        if (PlayerInput.Pressed(Button.B))
        {
            if (!PhysicsPaused && !inputAttackPaused)
            {
                PState executeState = DetermineAttack(PlayerInput.GetVector());
                if (!IsAttackRepeat(executeState) || Time.time - hitTimeStamp >= hitCooldown) // cancel attack if cooldown is not completed AND it's the same attack
                {
                    if (attackSequenceCoroutine != null)
                        StopCoroutine(attackSequenceCoroutine);
                    attackSequenceCoroutine = StartCoroutine(AttackSequence(executeState));
                }
            }
        }
    }

    Vector2 GetAttackDirection(PState atk)
    {   
        switch (atk)
        {
            case PState.A_AtkSide:
            case PState.G_AtkLow:
            case PState.G_AtkSide2:
            case PState.G_AtkSide1:
                return Vector2.right;
            case PState.G_AtkUp:
            case PState.A_AtkUp:
                return Vector2.up;
            case PState.A_AtkDown:
            case PState.A_AtkNeutral:
                return Vector2.down;
            default:
                return Vector3.zero;
        }
    }

    private void DeactivateAllAtkboxes()
    {
        foreach (GameObject atkbox in attackBoxes.Values)
            atkbox.SetActive(false);
    }

    // Determies if newAtk and the previous attack are considered the same move in terms of cooldown
    bool IsAttackRepeat(PState atk) => GetAttackDirection(atk) == GetAttackDirection(lastAtk);
    
    PState DetermineAttack(Vector2 lastInputDirection)
    {
        PState executeState = PState.Unassigned;
        if (lastInputDirection == Vector2.down)
        {
            if (_isGrounded)
            {
                executeState = PState.G_AtkLow;
            }
            else
            {
                executeState = PState.A_AtkDown;
            }
        }
        else if (lastInputDirection == Vector2.up)
        {
            if (_isGrounded)
            {
                executeState = PState.G_AtkUp;
            }
            else
            {
                executeState = PState.A_AtkUp;
            }
        }
        else
        {
            if (_isGrounded)
            {
                if (state == PState.G_AtkSide1)
                    executeState = PState.G_AtkSide2;
                else if (state == PState.G_AtkSide2)
                    executeState = PState.G_AtkTwist;
                else if (state != PState.G_AtkTwist)
                    executeState = PState.G_AtkSide1;
            }
            else
            {
                if (lastInputDirection.x == 0)
                    executeState = PState.A_AtkNeutral;
                else
                    executeState = PState.A_AtkSide;
            }
        }

        if (IsMovesetLocked(executeState))
            switch (executeState)
            {
                case PState.A_AtkDown:
                    if (!IsMovesetLocked(PState.A_AtkNeutral))
                        executeState = PState.A_AtkNeutral;
                    else
                        executeState = PState.A_AtkSide;
                    break;
                case PState.A_AtkUp:
                case PState.A_AtkNeutral:
                    executeState = PState.A_AtkSide;
                    break;
                case PState.G_AtkSide2:
                case PState.G_AtkTwist:
                case PState.G_AtkUp:
                    executeState = PState.G_AtkSide1;
                    break;
            }
        
        if (IsMovesetLocked(executeState))  // STILL locked
        {
            executeState = PState.Unassigned;
        }
        return executeState;
    }

    IEnumerator AttackSequence(PState executeState)
    {
        if (!_isGrounded)
        {
            if (canAerialAttack)
                canAerialAttack = false;
            else
            {
                attackSequenceCoroutine = null;
                yield break;
            }
        }


        if (_isGrounded)
            animator.PlayDefault();

        switch (executeState)
        {
            case PState.G_AtkSide1:                       
                SoundSystem.PlaySfx(sfx_swordSounds[0], 4);//play attack sfx
                DoInputMovePause(.3F);
                body.velocity = new Vector2(FacingDirection * 1.5F, body.velocity.y);
                if (doAttackCoroutine != null)
                    StopCoroutine(doAttackCoroutine);
                state = PState.G_AtkSide1;
                doAttackCoroutine = StartCoroutine(DoAttack(state));
                comboTimeStamp = Time.time;
                hitTimeStamp = Time.time;
                break;

            case PState.G_AtkSide2:
                SoundSystem.PlaySfx(sfx_swordSounds[1], 4);//play attack sfx
                DoInputMovePause(.3F);
                body.velocity = Vector2.zero;
                if (doAttackCoroutine != null)
                    StopCoroutine(doAttackCoroutine);
                state = PState.G_AtkSide2;
                doAttackCoroutine = StartCoroutine(DoAttack(state));
                comboTimeStamp = Time.time;
                hitTimeStamp = Time.time;
                break;

            case PState.G_AtkTwist: //do spin attack here and reset the other trigger here
                SoundSystem.PlaySfx(sfx_swordSounds[3], 4);
                DoInputMovePause(.33F);
                DoInputAttackPause(.66F);
                if (doAttackCoroutine != null)
                    StopCoroutine(doAttackCoroutine);
                state = PState.G_AtkTwist;
                doAttackCoroutine = StartCoroutine(DoAttack(state));
                Timer.Set(.5F, () => state = PState.Unassigned);
                comboTimeStamp = Time.time;
                hitTimeStamp = Time.time;
                break;

            case PState.G_AtkUp:
                SoundSystem.PlaySfx(sfx_swordSounds[0], 4);//play attack sfx
                DoInputMovePause(.3F);
                hitTimeStamp = Time.time;
                state = PState.G_AtkUp;
                SoundSystem.PlaySfx(sfx_swordSounds[0], 4);
                if (doAttackCoroutine != null)
                    StopCoroutine(doAttackCoroutine);
                doAttackCoroutine = StartCoroutine(DoAttack(state));
                break;

            case PState.G_AtkLow:
                SoundSystem.PlaySfx(sfx_swordSounds[1], 4);//play attack sfx
                DoInputMovePause(.3F);
                body.velocity = new Vector2(FacingDirection * 1.5F, body.velocity.y);
                if (doAttackCoroutine != null)
                    StopCoroutine(doAttackCoroutine);
                state = PState.G_AtkLow;
                doAttackCoroutine = StartCoroutine(DoAttack(state));
                comboTimeStamp = Time.time;
                hitTimeStamp = Time.time;
                break;
            
            case PState.A_AtkNeutral:
                SoundSystem.PlaySfx(sfx_swordSounds[0], 4);//play attack sfx
                state = PState.A_AtkNeutral;
                if (doAttackCoroutine != null)
                    StopCoroutine(doAttackCoroutine);
                doAttackCoroutine = StartCoroutine(DoAttack(PState.A_AtkNeutral));
                break;
            
            case PState.A_AtkSide:
                SoundSystem.PlaySfx(sfx_swordSounds[1], 4);//play attack sfx
                state = PState.A_AtkSide;
                //body.velocity = new Vector2(0, body.velocity.y);
                if (doAttackCoroutine != null)
                    StopCoroutine(doAttackCoroutine);
                doAttackCoroutine = StartCoroutine(DoAttack(PState.A_AtkSide));
                break;

            case PState.A_AtkUp:
                SoundSystem.PlaySfx(sfx_swordSounds[0], 4);//play attack sfx
                state = PState.A_AtkUp;
                body.velocity = Vector3.up * 15;
                body.gravityScale = originalGravityScale * .97F;
                if (doAttackCoroutine != null)
                    StopCoroutine(doAttackCoroutine);
                doAttackCoroutine = StartCoroutine(DoAttack(state));
                break;

            case PState.A_AtkDown:
                SoundSystem.PlaySfx(sfx_swordSounds[0], 4);//play attack sfx
                
                body.velocity = Vector3.down * 38;
                DoInputAttackPause(.5F);
                body.gravityScale = 30;
                if (doAttackCoroutine != null)
                    StopCoroutine(doAttackCoroutine);
                state = PState.A_AtkDown;
                doAttackCoroutine = StartCoroutine(DoAttack(state));
                break;
        }

        attackSequenceCoroutine = null;
        yield break;
    }

    private IEnumerator DoAttack(PState atkState)
    {
        lastAtk = atkState;
        isHitting = true;//set isHitting to true.

        float hitLengthMod = 1.0F;

        DeactivateAllAtkboxes();
        attackBoxes[atkState].SetActive(true);
            
        switch (atkState)
        {
            case PState.A_AtkUp:
                hitLengthMod = 1.2F;
                break;
            case PState.G_AtkUp:
                hitLengthMod = .85F;
                break;
            case PState.G_AtkTwist:         
                hitLengthMod = 2;
                break;
            case PState.A_AtkDown:
                hitLengthMod = 500; //for down attacks we want the hitbox active for as long as possible
                break;
        }

        yield return new WaitForSeconds(hitTime * hitLengthMod);//wait the established amount of seconds.

        DeactivateAllAtkboxes();

        isHitting = false;//set isHitting to false.
    }

    public void AttackFeedback(Vector2 force, Vector2 direction, AtkBonusAbility ability)
    {
        float newVelocityX = force.x == 0 ? body.velocity.x : force.x * 12 * FacingDirection;
        float newVelocityY = force.y == 0 ? body.velocity.y : force.y * 12;

        body.velocity = new Vector2(newVelocityX, newVelocityY);

        switch (ability)
        {
            case AtkBonusAbility.RenewAerialAttack:
                canAerialAttack = true;
                animationOverride = "roll";
                SetInvTime(Game.FRAME_TIME * 2);
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            canLadder = true;
            ladderX = other.bounds.center.x;
        }
        if (other.CompareTag("Fence"))
        {
            canFence = true;
        }

        if (other.CompareTag("Reset"))
        {
            this.gameObject.transform.position = checkpoint;
        }

        if (other.CompareTag("Scene"))
        {
            DoInputMovePause(100);
            DoInputAttackPause(100);
            DoPhysicsPause(100);
            Knaz.DoScene();
            animator.PlayDefault();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            canLadder = true;
            ladderX = other.bounds.center.x;
        }
        if (other.CompareTag("Fence"))
        {
            canFence = true;
        }
        if (other.gameObject.CompareTag("Door") && PlayerInput.Held(Button.Up) && CandleHandler.canUseDoor)
        {
            transform.position = new Vector3(199.5F, 100, transform.position.z);
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            canLadder = false;
        }
        if (other.CompareTag("Fence"))
        {
            canFence = false;
        }
    }

    private IEnumerator Die()
    {
        animator.Play(AnimMode.Looped, "hurt");

        invTimer = Timer.Set(1000);
        DoInputAttackPause(30);
        DoInputMovePause(30);

        yield return new WaitForSeconds(.44F);
        animator.Play(AnimMode.Hang, "die");
        
        SoundSystem.Pause();

        yield return new WaitForSeconds(2.5F);//wait the established amount of seconds.

        if (Game.lives > 0)
        {
            Game.lives--;
            ResetToLastCheckPoint();
            SoundSystem.Unpause();
            SoundSystem.PlayBgm(SoundSystem.Instance.defaultSong, SoundSystem.Instance.defaultSongLoopPoint, false);
        }
        else
        {
            HUD.Write("\n\n\n\n\n      GAME OVER");
            yield return new WaitForSeconds(4.5F);
            Game.Reset();
        }

    }
    public void ResetToLastCheckPoint()
    {
        this.gameObject.transform.position = checkpoint;
        currentHealth = maxHealth;
        Start();
    }

    private bool IsAtkActive(PState atk)
    {
        if (!attackBoxes.ContainsKey(atk)) return false;
        return attackBoxes[atk].activeSelf;
    }

    IEnumerator PausePhysics(float duration)
    {
        Vector2 oldVelocity = body.velocity;
        body.constraints = RigidbodyConstraints2D.FreezeAll;
        yield return new WaitForSeconds(duration);
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        pausePhysicsCoroutine = null;
        body.velocity = oldVelocity;
        yield break;
    }

    IEnumerator PauseInputMove(float duration)
    {
        inputMovePaused = true;
        body.velocity = Vector2.zero;
        yield return new WaitForSeconds(duration);
        inputMovePaused = false;
        yield break;
    }

    IEnumerator PauseInputAttack(float duration)
    {
        inputAttackPaused = true;
        yield return new WaitForSeconds(duration);
        inputAttackPaused = false;
        yield break;
    }
}
