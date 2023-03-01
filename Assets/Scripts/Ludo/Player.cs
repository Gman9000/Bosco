using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PState {Unassigned, 
    Idle, Walk, Run, Duck,
Jump, Fall, 
G_AtkSide1, G_AtkSide2, G_AtkSide3, G_AtkTwist, G_AtkUp, 
G_Rockclimbing, G_RockclimbingShimmy,
A_AtkUp, A_AtkSide, A_AtkDown, A_AtkNeutral, A_AtkRoll, 
A2G_Land, A2G_SwordLand,
}

public class Player : Pawn
{
    static public bool IsHurting => Time.time - Instance.hurtTimeStamp < Instance.invincibilityTime;
    static public int Hp => Instance.currentHealth;
    static public int FacingDirection => (int)Instance.transform.rotation.y == 0 ? 1 : -1;
    static public Vector3 Position => Instance.transform.position;

    public readonly List<PState> AttackStates = new List<PState>(){
        PState.G_AtkSide1,
        PState.G_AtkSide2,
        PState.G_AtkSide3,
        PState.G_AtkTwist,
        PState.G_AtkUp,
        PState.A_AtkRoll,
        PState.A_AtkDown,
        PState.A_AtkUp,
        PState.A_AtkNeutral
    };

    public AudioClip[] sfx_swordSounds;
    public AudioClip sfx_jump;
    public AudioClip sfx_hurt;

    public static Player Instance { get;protected set;}

    [HideInInspector]public Rigidbody2D body;
    public float moveSpeed;
    public float startFriction = 1;
    public float stopFriction = 1;
    public float maxFallSpeed = .1F;
    public float invincibilityTime = 1.2F;

    [HideInInspector]public BoxCollider2D boxCollider2D;

    private PState _state;

    public PState state {
        get => _state;
        set
        {
            _state = value;
        }
    }
    
    //for jumping
    public float jumpSpeed;
    public float jumpTime;
    private float jumpTimeCountdown;
    private bool isJumping;
    public float originalGravityScale;
    public float shimmySpeed = 2;
    public float rockJumpSpeed = 5;

    public Transform topLeftRaycast;
    public Transform topRightRaycast;
    public Transform bottomLeftRaycast;
    public Transform bottomRightRaycast;
    public float rayCastMagnitude;

    private bool inputMovePaused = false;
    private bool inputAttackPaused = false;

    private bool tbc = false;

    static public bool IsMovesetLocked(PState move) => _movesetLocks.Contains(move);
    static private List<PState> _movesetLocks = new List<PState>() {
        PState.G_AtkSide2,
        PState.A_AtkUp,
        PState.A_AtkDown,
        PState.G_AtkTwist,
        PState.G_Rockclimbing
    };


    // For attacking
    [SerializeField] private GameObject atkboxSide;
    [SerializeField] private GameObject atkboxTwist;
    [SerializeField] private GameObject atkboxUp;
    [SerializeField] private GameObject atkboxDown;
    [SerializeField] private GameObject atkboxAirNeutral;

    private bool isHitting = false;
    public float hitTime;
    public float hitCooldown;// should be less than hit time
    public float hitComboWindow;// should be less than hit time


    
    private float hitTimeStamp = 0;// the time when the player attacked
    private float comboTimeStamp = 0;// the time when the player attacked

    //health stuff
    public int maxhealth;
    private int currentHealth;

    Coroutine hitCoroutine = null;
    Coroutine pausePhysicsCoroutine = null;
    Coroutine pauseInputMoveCoroutine = null;
    Coroutine pauseInputAttackCoroutine = null;
    Coroutine attackSequenceCoroutine = null;
    public float deathTime;


    [HideInInspector]public SpriteAnimator animator;

    private Vector3 checkpoint;

    public bool IsDownSlash => atkboxDown.activeSelf;
    bool isGrounded;
    bool isHittingCeiling;
    bool isHittingRightWall;
    bool isHittingLeftWall;
    // Start is called before the first frame update

    bool canAerialAttack = false;
    bool PhysicsPaused => body.constraints == RigidbodyConstraints2D.FreezeAll;
    float hurtTimeStamp = 0;
    private Timer jumpTimer;

    [HideInInspector]public bool onRockwall;

    void Awake()
    {
        currentHealth = maxhealth;
        body = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        checkpoint = this.gameObject.transform.position;
        Instance = this;

        animator = GetComponent<SpriteAnimator>();
    }

    override public void Start()
    {
        state = PState.Idle;
        canAerialAttack = false;
        jumpTimer = null;
        inputAttackPaused = false;
        inputMovePaused = false;
        onRockwall = false;

        
        boxCollider2D.enabled = true;
        body.gravityScale = originalGravityScale;
        body.velocity = Vector2.zero;

        atkboxUp.SetActive(false);
        atkboxDown.SetActive(false);
        atkboxSide.SetActive(false);
        atkboxTwist.SetActive(false);
        atkboxAirNeutral.SetActive(false);

        animator.PlayDefault();
        hurtTimeStamp = -invincibilityTime;

        body.constraints = RigidbodyConstraints2D.FreezeRotation;

        StopAllCoroutines();

        hitCoroutine = null;
        pausePhysicsCoroutine = null;
        pauseInputMoveCoroutine = null;
        pauseInputAttackCoroutine = null;
        attackSequenceCoroutine = null;
    }

    void Update()
    {
        if (!Game.gameStarted || tbc)  return;
        
        if (PlayerInput.HasPressedStart())
        {
            if (Game.isPaused)
                Game.Unpause();
            else
                Game.Pause();
        }

        if (Game.isPaused)  return;

        bool wasGrounded = isGrounded;
        HitInfo groundCheck = boxCollider2D.IsGrounded(bottomLeftRaycast.position, bottomRightRaycast.position, rayCastMagnitude);
        if (groundCheck.layerName == "TwoWayPlatform")
        {
            if (body.velocity.y <= 0)
                isGrounded = true;
            if (isGrounded && PlayerInput.IsPressingDown() && PlayerInput.HasPressedA())    // make shift fallthrough
                isGrounded = false;
            if (groundCheck.hit.collider.tag == "Rockwall" && !wasGrounded && !PassThroughPlatform.rockwallCondition)
                isGrounded = false;
        }
        else
        {
            isGrounded = groundCheck;
        }
        
        //GMAN REMEMBER TO COMMENT THIS LATER
        if (isGrounded && (groundCheck.layerName == "TwoWayPlatform") && !wasGrounded)
        {
            float feetY = boxCollider2D.Bottom();
            float surfaceY = groundCheck.hit.collider.bounds.Top();
                if (surfaceY > feetY) isGrounded = false;
        }

        isHittingCeiling = boxCollider2D.IsHittingCeiling(topLeftRaycast.position, topRightRaycast.position, rayCastMagnitude);
        isHittingRightWall = boxCollider2D.IsHittingRight(topRightRaycast.position, bottomRightRaycast.position, rayCastMagnitude);
        isHittingLeftWall = boxCollider2D.IsHittingLeft(topLeftRaycast.position, bottomLeftRaycast.position, rayCastMagnitude);

        if (isGrounded)
        {
            if (jumpTimer != null && jumpTimer.done) jumpTimer.Cancel();
            
            if (state == PState.A_AtkDown)   // if is falling fast
            {
                Game.VertShake(8);// shake screen
                DoPhysicsPause(.4F);
                atkboxDown.SetActive(false);
                DoInputAttackPause(.4F);
                Timer.Set(.4F, () => state = PState.Unassigned);
                state = PState.A2G_SwordLand;
            }

            body.gravityScale = originalGravityScale;

            canAerialAttack = true;
            
            if (!wasGrounded && body.velocity.y < 0)    // check if moving down
            {                
                atkboxUp.SetActive(false);
                body.velocity = new Vector2(body.velocity.x, 0);
            }
        }
        else    // not grounded
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
            if (atkboxTwist.activeSelf)
            {
                body.velocity = new Vector2(FacingDirection * 3.0F, body.velocity.y);
            }
            if (atkboxSide.activeSelf && isGrounded)
            {
                if (Time.time - hitTimeStamp > .15F)
                {
                    body.velocity = new Vector2(0, body.velocity.y);
                }
            }
        }
        else
        {
            if (PlayerInput.IsPressingDown())
            {
                if (isGrounded)
                {
                    float newVelX = body.velocity.x;
                    newVelX -= Mathf.Sign(newVelX) * stopFriction * 2 * Game.relativeTime;
                    if (Mathf.Sign(newVelX) == Mathf.Sign(body.velocity.x)) // checking the sign so the friction doesn't reverse movement direction
                        body.velocity = new Vector2(newVelX, body.velocity.y);
                    else
                        body.velocity = new Vector2(0, body.velocity.y);
                }

                if (state == PState.Idle || state == PState.Walk)
                    state = PState.Duck;
            }
            else if (PlayerInput.IsPressingLeft())
            {
                body.velocity = new Vector2(body.velocity.x - startFriction * Game.relativeTime, body.velocity.y);
                if (body.velocity.x < -moveSpeed)
                    body.velocity = new Vector2(-moveSpeed, body.velocity.y);
                this.transform.rotation = new Quaternion(0f, 180f, this.transform.rotation.z, this.transform.rotation.w);
            }
            else if (PlayerInput.IsPressingRight())
            {
                body.velocity = new Vector2(body.velocity.x + startFriction * Game.relativeTime, body.velocity.y);
                if (body.velocity.x > moveSpeed)
                    body.velocity = new Vector2(moveSpeed, body.velocity.y);
                this.transform.rotation = new Quaternion(0f, 0f, this.transform.rotation.z, this.transform.rotation.w);
            }
            else if (!atkboxTwist.activeSelf)    // add friction
            {
                float newVelX = body.velocity.x;
                newVelX -= Mathf.Sign(newVelX) * stopFriction * Game.relativeTime;
                if (Mathf.Sign(newVelX) == Mathf.Sign(body.velocity.x)) // checking the sign so the friction doesn't reverse movement direction
                    body.velocity = new Vector2(newVelX, body.velocity.y);
                else
                    body.velocity = new Vector2(0, body.velocity.y);
            }

            if (!PlayerInput.IsPressingDown() && state == PState.Duck)
                state = PState.Unassigned;

            if (isGrounded && PlayerInput.HasPressedA())
            {
                isJumping = true;
                body.velocity = new Vector2(body.velocity.x, jumpSpeed);
                float startY = transform.position.y;
                jumpTimer = Timer.Set(jumpTime, () =>
                {
                    isJumping = false;
                });
                SoundSystem.PlaySfx(sfx_jump, 2);
            }

            if (isJumping && PlayerInput.HasHeldA())
            {
                body.velocity = new Vector2(body.velocity.x, jumpSpeed);
                if (isHittingCeiling)
                {
                    if(jumpTimer != null && jumpTimer.done) jumpTimer.Cancel();
                    isJumping = false;
                }
            }

            if (PlayerInput.HasReleasedA())
            {
                isJumping = false;
            }
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
            if (isGrounded)
                SetStateFromHorizontal();
            else
                SetStateFromVertical();
        }

        if (currentHealth > 0)
            Animate();
    }

    void SetStateFromHorizontal()
    {
        if (body.velocity.x == 0)
        {
            state = PState.Idle;
            if (onRockwall)
                state = PState.G_Rockclimbing;
        }
        else
        {
            state = PState.Walk;
            if (onRockwall)
                state = PState.G_RockclimbingShimmy;
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
        isGrounded = value;
        switch (type)
        {
            case "Rockwall":
                if (IsMovesetLocked(PState.G_Rockclimbing))
                    onRockwall = value;
                break;
        }
    }

    void FixedUpdate()
    {
        if (Time.time - hurtTimeStamp < invincibilityTime)
        {
            animator.Renderer.color = animator.Renderer.color.a == 0 ? Color.white : new Color(0,0,0,0);
        }
        else
        {
            animator.Renderer.color = Color.white;
        }

        if (HUD.Instance)
            HUD.Instance.Flash(animator.Renderer.color.a == 1, "Main Text Layer", "BG");
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.transform.childCount > 0 && other.transform.GetChild(0).CompareTag("Hidden") && other.contacts[0].normal.y > 0)
        {
            Vector3 pos = transform.position;
            pos.y = other.collider.bounds.center.y + 1;
            transform.position = pos;
        }            
    }


    void Animate()
    {
        if (IsHurting && inputMovePaused && inputAttackPaused)
            animator.Play(AnimMode.Looped, "hurt");
        else switch (state)
        {
            case PState.Idle:
                animator.PlayDefault();
                break;
            case PState.Walk:
                animator.Play(AnimMode.Looped, "walk");
                break;
            case PState.Duck:
                animator.Play(AnimMode.Looped, "duck");
                break;
            case PState.Jump:
                animator.Play(AnimMode.Hang, "jump");
                break;
            case PState.Fall:
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

            case PState.G_Rockclimbing:
                animator.Play(AnimMode.Looped, "duck");
                break;
            case PState.G_RockclimbingShimmy:
                animator.Play(AnimMode.Looped, "duck");
                break;


            case PState.A2G_Land:
                animator.Play(AnimMode.Once, "duck", () => state = PState.Unassigned);
                break;
            case PState.A2G_SwordLand:
                animator.Play(AnimMode.Hang, "downslash");
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

    public void TakeDamage()
    {
        if (Time.time - hurtTimeStamp >= invincibilityTime && currentHealth > 0)
        {
            hurtTimeStamp = Time.time;
            SoundSystem.PlaySfx(sfx_hurt, 3);
            
            currentHealth--;
            DoInputMovePause(.25F);
            DoInputAttackPause(.25F);
            body.velocity = Vector2.up - (FacingDirection * Vector2.right * 5F);

            if(currentHealth <= 0)
            {
                //for now this is death
                StartCoroutine(Die());
            }
        }
    }

    private void AttackInputs()
    {
        if (PlayerInput.HasPressedB())
        {
            if (Time.time - hitTimeStamp >= hitCooldown && !PhysicsPaused && !inputAttackPaused)
            {
                if (attackSequenceCoroutine != null)
                    StopCoroutine(attackSequenceCoroutine);
                attackSequenceCoroutine = StartCoroutine(AttackSequence());
            }
        }
    }

    private IEnumerator Attack(PState atkState)
    {
        isHitting = true;//set isHitting to true.

        float hitLengthMod = 1.0F;

        atkboxUp.SetActive(false);
        atkboxDown.SetActive(false);
        atkboxSide.SetActive(false);
        atkboxTwist.SetActive(false);
        atkboxAirNeutral.SetActive(false);
            
        switch (atkState)
        {
            case PState.A_AtkUp:
            case PState.G_AtkUp:
                atkboxUp.SetActive(true);
                if (!isGrounded)
                    hitLengthMod = 1.2F;
                else
                    hitLengthMod = .85F;
                break;
            case PState.A_AtkDown:
                atkboxDown.SetActive(true);
                hitLengthMod = 500;//for down attacks we want the hitbox active for as long as possible
                break;
            case PState.G_AtkTwist:                
                atkboxTwist.SetActive(true);
                hitLengthMod = 2;
                break;
            case PState.A_AtkNeutral:
                atkboxAirNeutral.SetActive(true);
                hitLengthMod = 1;
                break;
            case PState.G_AtkSide1:
            case PState.G_AtkSide2:
            case PState.G_AtkSide3:
            case PState.A_AtkSide:
                atkboxSide.SetActive(true);
                break;
        }



        yield return new WaitForSeconds(hitTime * hitLengthMod);//wait the established amount of seconds.

        switch (atkState)
        {
            case PState.A_AtkUp:
            case PState.G_AtkUp:
                atkboxUp.SetActive(false);
                break;
            case PState.A_AtkDown:
                atkboxDown.SetActive(false);
                break;
            case PState.G_AtkTwist:
                atkboxTwist.SetActive(false);
                break;
            case PState.A_AtkNeutral:
                atkboxAirNeutral.SetActive(false);
                break;
            case PState.G_AtkSide1:
            case PState.G_AtkSide2:
            case PState.G_AtkSide3:
            case PState.A_AtkSide:
                atkboxSide.SetActive(false);
                break;
        }
        isHitting = false;//set isHitting to false.
    }

    public void AttackFeedback(Vector2 force, Vector2 direction)
    {
        float newVelocityX = force.x == 0 ? body.velocity.x : force.x * 12 * FacingDirection;
        float newVelocityY = force.y == 0 ? body.velocity.y : force.y * 12;

        Debug.Log(force.x);
        body.velocity = new Vector2(newVelocityX, newVelocityY);
        
        Game.VertShake(2);
        Game.FreezeFrame(Game.FRAME_TIME * 4);
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Reset"))
        {
            this.gameObject.transform.position = checkpoint;
        }

        if (other.CompareTag("Scene"))
        {
            DoInputMovePause(100);
            DoInputAttackPause(100);
            DoPhysicsPause(100);
            tbc = true;
            Knaz.DoScene();
            animator.PlayDefault();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Door") && PlayerInput.IsPressingUp() && CandleHandler.canUseDoor)
        {
            transform.position = new Vector3(199.5F, 100, transform.position.z);
        }
    }

    private IEnumerator Die()
    {
        animator.Play(AnimMode.Looped, "hurt");

        hurtTimeStamp = 0;
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
        currentHealth = maxhealth;
        Start();
    }

    IEnumerator PausePhysics(float duration)
    {
        yield break;
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

    IEnumerator AttackSequence()
    {
        if (!isGrounded)
        {
            if (canAerialAttack)
                canAerialAttack = false;
            else
            {
                attackSequenceCoroutine = null;
                yield break;
            }
        }

        
        Vector2 lastInputDirection = Vector2.zero;
        if (isGrounded)
        {
            if (PlayerInput.IsPressingUp())
            {
                DoInputMovePause(.08F);
            }
            else
            {
                DoInputMovePause(0.001F);
                body.velocity = new Vector2(FacingDirection * 1.5F, body.velocity.y);
            }
        }
        else
            DoPhysicsPause(.08F);

        do
        {
            yield return new WaitForEndOfFrame();
            if (PlayerInput.IsPressingDown() && !isGrounded)
                lastInputDirection = Vector2.down;
            else if (PlayerInput.IsPressingUp() && !atkboxUp.activeSelf)
                lastInputDirection = Vector2.up;
            else if (PlayerInput.IsPressingLeft())
                lastInputDirection = Vector2.left;
            else if (PlayerInput.IsPressingRight())
                lastInputDirection = Vector2.right;

        }
        while ((PhysicsPaused && !isGrounded) || (inputMovePaused && isGrounded));


        if (isGrounded)
            animator.PlayDefault();


        PState executeState = PState.Unassigned;

        if (lastInputDirection == Vector2.down)
        {
            if (isGrounded)
            {
                // todo: duck stab
            }
            else
            {
                executeState = PState.A_AtkDown;
            }
        }
        else if (lastInputDirection == Vector2.up)
        {
            if (isGrounded)
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
            if (isGrounded)
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
               executeState = PState.A_AtkNeutral;
            }
        }

        if (IsMovesetLocked(executeState))
            switch (executeState)
            {
                case PState.A_AtkDown:
                case PState.A_AtkUp:
                    executeState = PState.A_AtkNeutral;
                    break;
                case PState.G_AtkSide2:
                case PState.G_AtkTwist:
                case PState.G_AtkUp:
                    executeState = PState.G_AtkSide1;
                    break;
                case PState.G_AtkSide1:
                case PState.A_AtkNeutral:
                    executeState = PState.Unassigned;
                    break;
            }

        switch (executeState)
        {
            case PState.G_AtkSide1:                       
                SoundSystem.PlaySfx(sfx_swordSounds[0], 4);//play attack sfx
                DoInputMovePause(.3F);
                body.velocity = new Vector2(FacingDirection * 1.5F, body.velocity.y);
                if (hitCoroutine != null)
                    StopCoroutine(hitCoroutine);
                state = PState.G_AtkSide1;
                hitCoroutine = StartCoroutine(Attack(state));
                comboTimeStamp = Time.time;
                hitTimeStamp = Time.time;
                break;

            case PState.G_AtkSide2:
                if (IsMovesetLocked(PState.G_AtkSide2))
                {
                    attackSequenceCoroutine= null;
                    yield break;
                }
                SoundSystem.PlaySfx(sfx_swordSounds[1], 4);//play attack sfx
                DoInputMovePause(.3F);
                body.velocity = Vector2.zero;
                if (hitCoroutine != null)
                    StopCoroutine(hitCoroutine);
                state = PState.G_AtkSide2;
                hitCoroutine = StartCoroutine(Attack(state));
                comboTimeStamp = Time.time;
                hitTimeStamp = Time.time;
                break;

            case PState.G_AtkTwist: //do spin attack here and reset the other trigger here
                if (IsMovesetLocked(PState.G_AtkTwist))
                {
                    attackSequenceCoroutine= null;
                    yield break;
                }
                SoundSystem.PlaySfx(sfx_swordSounds[3], 4);
                DoInputMovePause(.33F);
                DoInputAttackPause(.66F);
                if (hitCoroutine != null)
                    StopCoroutine(hitCoroutine);
                state = PState.G_AtkTwist;
                hitCoroutine = StartCoroutine(Attack(state));
                Timer.Set(.5F, () => state = PState.Unassigned);
                comboTimeStamp = Time.time;
                hitTimeStamp = Time.time;
                break;

            case PState.A_AtkNeutral:
                state = PState.A_AtkNeutral;
                if (hitCoroutine != null)
                    StopCoroutine(hitCoroutine);
                hitCoroutine = StartCoroutine(Attack(PState.A_AtkNeutral));
                break;

            case PState.G_AtkUp:
                SoundSystem.PlaySfx(sfx_swordSounds[0], 4);//play attack sfx
                DoInputMovePause(.3F);
                hitTimeStamp = Time.time;
                state = PState.G_AtkUp;
                SoundSystem.PlaySfx(sfx_swordSounds[0], 4);
                if (hitCoroutine != null)
                    StopCoroutine(hitCoroutine);
                hitCoroutine = StartCoroutine(Attack(state));
                break;

            case PState.A_AtkUp:
                body.velocity = Vector3.up * 15;
                body.gravityScale = originalGravityScale * .97F;
                state = PState.A_AtkUp;
                SoundSystem.PlaySfx(sfx_swordSounds[0], 4);
                if (hitCoroutine != null)
                    StopCoroutine(hitCoroutine);
                hitCoroutine = StartCoroutine(Attack(state));
                break;

            case PState.A_AtkDown:
                SoundSystem.PlaySfx(sfx_swordSounds[0], 4);//play attack sfx
                
                body.velocity = Vector3.down * 38;
                DoInputAttackPause(.5F);
                body.gravityScale = 30;
                if (hitCoroutine != null)
                    StopCoroutine(hitCoroutine);
                state = PState.A_AtkDown;
                hitCoroutine = StartCoroutine(Attack(state));
                break;
        }        

        attackSequenceCoroutine = null;
        yield break;
    }
}
