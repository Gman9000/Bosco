using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PState {Unassigned, 
    Idle, Walk, Run, Duck,
Jump, Fall,
ClimbIdle, ClimbMove,
G_SwordSide1, G_SwordSide2, G_SwordSide3, G_SwordTwist, G_SwordUp, 
A_SwordUp, A_SwordRoll, A_SwordSide, A_SwordDown,
A2G_Land, A2G_SwordLand,
}
public class Player : Pawn
{
    static public bool IsHurting => Time.time - Instance.hurtTimeStamp < Instance.invincibilityTime;
    static public int Hp => Instance.currentHealth;
    static public int FacingDirection => (int)Instance.transform.rotation.y == 0 ? 1 : -1;
    static public Vector3 Position => Instance.transform.position;

    public readonly List<PState> AttackStates = new List<PState>(){
        PState.G_SwordSide1,
        PState.G_SwordSide2,
        PState.G_SwordSide3,
        PState.G_SwordTwist,
        PState.G_SwordUp,
        PState.A_SwordRoll,
        PState.A_SwordDown,
        PState.A_SwordUp
    };

    public AudioClip[] sfx_swordSounds;
    public AudioClip sfx_jump;
    public AudioClip sfx_hurt;

    public static Player Instance { get; protected set; }
    private Rigidbody2D body;
    public float moveSpeed;
    public float invincibilityTime = 1.2F;

    private BoxCollider2D boxCollider2D;

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
    public float DownThrustGravityModifier;

    public Transform topLeftRaycast;
    public Transform topRightRaycast;
    public Transform bottomLeftRaycast;
    public Transform bottomRightRaycast;
    public float rayCastMagnitude;

    private bool inputMovePaused = false;
    private bool inputAttackPaused = false;

    private bool tbc = false;


    //for attacking
    //[SerializeField] private GameObject leftAttackHitbox;
    [SerializeField] private GameObject rightAttackHitbox;
    [SerializeField] private GameObject spinAttackHitbox;
    [SerializeField] private GameObject upAttackHitbox;
    [SerializeField] private GameObject downAttackHitbox;
    private bool isHitting = false;
    public float hitTime;
    public float hitCooldown;   // should be less than hit time
    public float hitComboWindow;   // should be less than hit time


    
    private float hitTimeStamp = 0; // the time when the player attacked
    private float comboTimeStamp = 0; // the time when the player attacked

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

    bool isGrounded;
    bool isHittingCeiling;
    bool isHittingRightWall;
    bool isHittingLeftWall;
    // Start is called before the first frame update

    //for ladder checks
    bool canClimb = false;
    bool canAerialAttack = false;

    bool PhysicsPaused => body.constraints == RigidbodyConstraints2D.FreezeAll;

    float hurtTimeStamp = 0;
    private Timer jumpTimer;

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

        
        boxCollider2D.enabled = true;
        body.gravityScale = originalGravityScale;
        body.velocity = Vector2.zero;

        upAttackHitbox.SetActive(false);
        downAttackHitbox.SetActive(false);
        rightAttackHitbox.SetActive(false);
        spinAttackHitbox.SetActive(false);
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
        }
        else
        {
            isGrounded = groundCheck;
        }
        
        //GMAN REMEMBER TO COMMENT THIS LATER
        if (isGrounded && groundCheck.layerName == "TwoWayPlatform" && !wasGrounded)
        {
            float feetY = boxCollider2D.bounds.min.y;
            float surfaceY = groundCheck.hit.collider.bounds.max.y;
                if (surfaceY > feetY) isGrounded = false;
        }

        Debug.Log(isGrounded);

        isHittingCeiling = boxCollider2D.IsHittingCeiling(topLeftRaycast.position, topRightRaycast.position, rayCastMagnitude);
        isHittingRightWall = boxCollider2D.IsHittingRight(topRightRaycast.position, bottomRightRaycast.position, rayCastMagnitude);
        isHittingLeftWall = boxCollider2D.IsHittingLeft(topLeftRaycast.position, bottomLeftRaycast.position, rayCastMagnitude);

        if (isGrounded)
        {
            if (jumpTimer != null && jumpTimer.Done) jumpTimer.Cancel();
            
            if (state == PState.A_SwordDown)   // if is falling fast
            {
                CameraController.Instance.VertShake(6);          // shake screen
                DoPhysicsPause(.4F);
                downAttackHitbox.SetActive(false);
                DoInputAttackPause(.4F);
                Timer.Set(.4F, () => state = PState.Unassigned);
                state = PState.A2G_SwordLand;
            }

            body.gravityScale = originalGravityScale;

            canAerialAttack = true;
            
            if (!wasGrounded && body.velocity.y < 0)    // check if moving down
            {                
                upAttackHitbox.SetActive(false);
                body.velocity = new Vector2(body.velocity.x, 0);   
                SnapToPixel();             
            }
        }
        else    // not grounded
        {
            if (body.velocity.y < 0 && state != PState.A_SwordDown)
                state = PState.Fall;

            if (wasGrounded)        // only set as unassigned if the player was JUST on the ground
            {
                state = PState.Unassigned;
            }
        }


        if (inputMovePaused)
        {
            if (spinAttackHitbox.activeSelf)
            {
                body.velocity = new Vector2(FacingDirection * 3.0F, body.velocity.y);
            }
            if (rightAttackHitbox.activeSelf && isGrounded)
            {
                if (Time.time - hitTimeStamp > .15F)
                {
                    body.velocity = new Vector2(0, body.velocity.y);
                }
            }
        }
        else
        {
            if (!spinAttackHitbox.activeSelf)
                body.velocity = new Vector2(0, body.velocity.y);

            //GMAN COMMENT HERE
            if (PlayerInput.IsPressingUp())
            {
                if (canClimb)
                {
                    state = PState.ClimbIdle;
                }
            }

            if (state == PState.ClimbIdle || state == PState.ClimbMove)
            {
                body.gravityScale = 0f;
                body.velocity = new Vector2(body.velocity.x, 0f);
                state = PState.ClimbIdle;
                if (PlayerInput.IsPressingUp())
                {
                    state = PState.ClimbMove;
                    body.velocity = new Vector2(body.velocity.x, moveSpeed);
                }
                if (PlayerInput.IsPressingDown())
                {
                    state = PState.ClimbMove;
                    body.velocity = new Vector2(body.velocity.x, -moveSpeed);
                }
            }
            ///GMAN COMMENT ENDS
            if (PlayerInput.IsPressingLeft())
            {
                body.velocity = new Vector2(-moveSpeed, body.velocity.y);
                this.transform.rotation = new Quaternion(0f, 180f, this.transform.rotation.z, this.transform.rotation.w);                
            }
            else if (PlayerInput.IsPressingRight())
            {
                body.velocity = new Vector2(moveSpeed, body.velocity.y);
                this.transform.rotation = new Quaternion(0f, 0f, this.transform.rotation.z, this.transform.rotation.w);
            }
            else
            {
                SnapToPixel();
            }

            if (PlayerInput.IsPressingDown())
            {
                body.velocity = new Vector2(0, body.velocity.y);
                if (state == PState.Idle || state == PState.Walk)
                    state = PState.Duck;
            }
            else if (state == PState.Duck)
                state = PState.Unassigned;

            if (isGrounded && PlayerInput.HasPressedA())
            {
                isJumping = true;
                body.velocity = new Vector2(body.velocity.x, jumpSpeed);
                float startY = transform.position.y;
                jumpTimer = Timer.Set(jumpTime, () =>
                {
                    isJumping = false;
                    Debug.Log(transform.position.y - startY);
                });
                SoundSystem.PlaySfx(sfx_jump, 2);
            }

            if (isJumping && PlayerInput.HasHeldA())
            {
                body.velocity = new Vector2(body.velocity.x, jumpSpeed);
                if (isHittingCeiling)
                {
                    if(jumpTimer != null && jumpTimer.Done) jumpTimer.Cancel();
                    isJumping = false;
                }
            }

            if (PlayerInput.HasReleasedA())
            {
                isJumping = false;
            }
        }

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
            state = PState.Idle;
        else
            state = PState.Walk;
    }

    void SetStateFromVertical()
    {
        if (body.velocity.y >= 0)
            state = PState.Jump;
        else
            state = PState.Fall;
    }

    bool IsAttackState() => AttackStates.Contains(state);

    void SnapToPixel()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Round(pos.x / Game.PIXEL) * Game.PIXEL;
        pos.y = Mathf.Floor(pos.y / Game.PIXEL) * Game.PIXEL;
        transform.position = pos;
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
            SnapToPixel();
        }            
    }


    void Animate()
    {
        if (IsHurting && inputMovePaused && inputAttackPaused)
            animator.Play(AnimMode.Looped, "hurt");
        else if (isGrounded)
        {

            /*
            if (upAttackHitbox.activeSelf)
            {
                animator.Play(AnimMode.Hang, "upslash ground");
            }
            else if (spinAttackHitbox.activeSelf)
            {
                animator.Play(AnimMode.Looped, "spin");
            }
            else if (rightAttackHitbox.activeSelf)
            {
                animator.Play(AnimMode.Hang, consecutiveHits <= 1 ? "slash 1" : "slash 2");
            }
            else if (!PhysicsPaused && !inputMovePaused && PlayerInput.IsPressingDown())
            {
                animator.Play(AnimMode.Looped, "duck");
            }
            else if (!PhysicsPaused && !inputMovePaused && (PlayerInput.IsPressingLeft() || PlayerInput.IsPressingRight()))
            {
                animator.Play(AnimMode.Looped, "walk");
            }
            else if (!PhysicsPaused)
            {
                animator.Play(AnimMode.Looped, "idle");
            }*/
        }
        else
        {
            /*if (spinAttackHitbox.activeSelf)
            {
                animator.Play(AnimMode.Looped, "spin");
            }
            else if (downAttackHitbox.activeSelf)
            {
                animator.Play(AnimMode.Hang, "downslash");
            }
            else if (upAttackHitbox.activeSelf)
            {
                animator.Play(AnimMode.Hang, "upslash air");
            }
            else if (body.velocity.y > 0)
            {
                animator.Play(AnimMode.Hang, "jump");
            }
            else
            {
                animator.Play(AnimMode.Hang, "fall");
            }*/

        }

        switch (state)
        {
            case PState.Idle:
                animator.PlayDefault();
                break;
            case PState.Walk:
                animator.Play(AnimMode.Looped, "walk");
                break;
            case PState.Duck:
                animator.Play(AnimMode.Hang, "duck");
                break;
            case PState.Jump:
                animator.Play(AnimMode.Hang, "jump");
                break;
            case PState.Fall:
                animator.Play(AnimMode.Hang, "fall");
                break;

                
            case PState.G_SwordSide1:
                animator.Play(AnimMode.Once, "slash 1", () => state = PState.Unassigned);
                break;
            case PState.G_SwordSide2:
                animator.Play(AnimMode.Once, "slash 2", () => state = PState.Unassigned);
                break;
            case PState.G_SwordTwist:
                animator.Play(AnimMode.Looped, "spin");
                break;
            case PState.G_SwordUp:
                animator.Play(AnimMode.Once, "upslash ground", () => state = PState.Unassigned);
                break;


            case PState.A_SwordDown:
                animator.Play(AnimMode.Hang, "downslash");
                break;
            case PState.A_SwordUp:
                animator.Play(AnimMode.Hang, "upslash air");
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

    private IEnumerator Hit(int hitBoxIndex)
    {

        isHitting = true;       //set isHitting to true.

        float hitLengthMod = 1.0F;

        upAttackHitbox.SetActive(false);
        downAttackHitbox.SetActive(false);
        rightAttackHitbox.SetActive(false);
        spinAttackHitbox.SetActive(false);
            
        switch (hitBoxIndex)
        {
            case 1:
                // Upwards attack
                upAttackHitbox.SetActive(true);
                if (!isGrounded)
                    hitLengthMod = 1.2F;
                else
                    hitLengthMod = .85F;
                break;
            case 2:
                //Downwards attack
                downAttackHitbox.SetActive(true);
                hitLengthMod = 500;//for down attacks we want the hitbox active for as long as possible
                break;
            case 3:
                //Spin attack
                //take the leftAttackHitBox and turn that into the spin attack that hits both the left and the right of the player
                spinAttackHitbox.SetActive(true);
                hitLengthMod = 2;
                break;
            case 4:
                //Right attack
                rightAttackHitbox.SetActive(true);

                break;
            default:
                Debug.LogError("AN ERROR HAS OCCURRED WHILE TRYING TO ATTACK");
            // Statements to Execute if No Case Matches
            break;
        }



        yield return new WaitForSeconds(hitTime * hitLengthMod);   //wait the established amount of seconds.

        switch (hitBoxIndex)
        {
            case 1:
                // Upwards attack
                upAttackHitbox.SetActive(false);
                break;
            case 2:
                //Downwards attack
                downAttackHitbox.SetActive(false);
                break;
            case 3:
                //Spin attack
                spinAttackHitbox.SetActive(false);                
                break;
            case 4:
                //Right attack
                rightAttackHitbox.SetActive(false);
                break;
            default:
                Debug.LogError("AN ERROR HAS OCCURRED WHILE TRYING TO ATTACK");
                // Statements to Execute if No Case Matches
                break;
        }
        isHitting = false;                          //set isHitting to false.
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            canClimb = true;
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
            tbc = true;
            Knaz.DoScene();
            animator.PlayDefault();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            canClimb = true;
        }
        if (other.gameObject.CompareTag("Door") && PlayerInput.IsPressingUp() && CandleHandler.canUseDoor)
        {
            transform.position = new Vector3(199.5F, 100, transform.position.z);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            if (state == PState.ClimbIdle || state == PState.ClimbMove)
            {
                state = PState.Unassigned;
                body.gravityScale = originalGravityScale;
            }
            canClimb = false;
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

        yield return new WaitForSeconds(2.5F);   //wait the established amount of seconds.

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
        Vector2 velocity = body.velocity;
        body.constraints = RigidbodyConstraints2D.FreezeAll;
        yield return new WaitForSeconds(duration);
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        pausePhysicsCoroutine = null;
        body.velocity = velocity;
        SnapToPixel();
        yield break;
    }

    IEnumerator PauseInputMove(float duration)
    {
        inputMovePaused = true;
        body.velocity = Vector2.zero;
        yield return new WaitForSeconds(duration);
        inputMovePaused = false;
        SnapToPixel();
        yield break;
    }

    IEnumerator PauseInputAttack(float duration)
    {
        inputAttackPaused = true;
        yield return new WaitForSeconds(duration);
        inputAttackPaused = false;
        SnapToPixel();
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
                body.velocity = new Vector2(FacingDirection * 4F, body.velocity.y);
            }
        }
        else
            DoPhysicsPause(.08F);

        do
        {
            yield return new WaitForEndOfFrame();
            if (PlayerInput.IsPressingDown() && !isGrounded)
                lastInputDirection = Vector2.down;
            else if (PlayerInput.IsPressingUp() && !upAttackHitbox.activeSelf)
                lastInputDirection = Vector2.up;
            else if (PlayerInput.IsPressingLeft())
                lastInputDirection = Vector2.left;
            else if (PlayerInput.IsPressingRight())
                lastInputDirection = Vector2.right;

        }
        while ((PhysicsPaused && !isGrounded) || (inputMovePaused && isGrounded));


        if (isGrounded)
            animator.PlayDefault();


        if (lastInputDirection == Vector2.down)
        {
            if (isGrounded)
            {

            }
            else
            {
                SoundSystem.PlaySfx(sfx_swordSounds[0], 4);   //play attack sfx
                
                body.velocity = Vector3.down * 38;
                DoInputAttackPause(.5F);
                body.gravityScale = 30;
                if (hitCoroutine != null)
                    StopCoroutine(hitCoroutine);
                hitCoroutine = StartCoroutine(Hit(2));
                state = PState.A_SwordDown;
            }
        }
        else if (lastInputDirection == Vector2.up)
        {
            if (isGrounded)
            {
                SoundSystem.PlaySfx(sfx_swordSounds[0], 4);   //play attack sfx
                DoPhysicsPause(.12F);
                DoInputMovePause(.3F);
                hitTimeStamp = Time.time;

                state = PState.G_SwordUp;
            }
            else
            {
                body.velocity = Vector3.up * 15;
                body.gravityScale = originalGravityScale * .97F;
                state = PState.A_SwordUp;
            }

            SoundSystem.PlaySfx(sfx_swordSounds[0], 4);
            if (hitCoroutine != null)
                StopCoroutine(hitCoroutine);
            hitCoroutine = StartCoroutine(Hit(1));
        }
        else
        {
            if (isGrounded)
            {
                switch (state)
                {
                    default:
                        if (state == PState.G_SwordTwist)  break;

                        SoundSystem.PlaySfx(sfx_swordSounds[0], 4);   //play attack sfx
                        DoPhysicsPause(.08F);
                        DoInputMovePause(.3F);
                        body.velocity = Vector2.zero;
                        if (hitCoroutine != null)
                            StopCoroutine(hitCoroutine);
                        hitCoroutine = StartCoroutine(Hit(4));
                        state = PState.G_SwordSide1;
                        break;
                    case PState.G_SwordSide1:
                        SoundSystem.PlaySfx(sfx_swordSounds[1], 4);   //play attack sfx
                        DoPhysicsPause(.08F);
                        DoInputMovePause(.3F);
                        body.velocity = Vector2.zero;
                        if (hitCoroutine != null)
                            StopCoroutine(hitCoroutine);
                        hitCoroutine = StartCoroutine(Hit(4));
                        state = PState.G_SwordSide2;
                        break;
                    case PState.G_SwordSide2: //do spin attack here and reset the other trigger here
                        SoundSystem.PlaySfx(sfx_swordSounds[3], 4);
                        DoInputMovePause(.33F);
                        DoInputAttackPause(.66F);
                        if (hitCoroutine != null)
                            StopCoroutine(hitCoroutine);
                        hitCoroutine = StartCoroutine(Hit(3));
                        state = PState.G_SwordTwist;
                        Timer.Set(.5F, () => state = PState.Unassigned);
                        break;
                }
                comboTimeStamp = Time.time;
                hitTimeStamp = Time.time;
            }
            else
            {
                // default aerial
            }
        }

        attackSequenceCoroutine = null;
        yield break;
    }
}
