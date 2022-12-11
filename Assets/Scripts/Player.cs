using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    static public int Hp => Instance.currentHealth;
    public AudioClip[] sfx_swordSounds;
    public AudioClip sfx_jump;
    public AudioClip sfx_hurt;

    private int consecutiveHits = 0;

    public static Player Instance { get; protected set; }
    private Rigidbody2D myRB;
    public float moveSpeed;
    //private bool canPlay;

    public float invincibilityTime = 1.2F;
    public float invincibilityTimeFactor;

    private BoxCollider2D boxCollider2D;

    //for jumping
    //public float originalJumpSpeed;
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


    //for attacking
    //[SerializeField] private GameObject leftAttackHitbox;
    [SerializeField] private GameObject rightAttackHitbox;
    [SerializeField] private GameObject spinAttackHitbox;
    [SerializeField] private GameObject upAttackHitbox;
    [SerializeField] private GameObject downAttackHitbox;
    [SerializeField] private GameObject radialAttackHitbox;
    private bool isHitting = false;
    public float hitTime;
    public float hitCooldown;   // should be less than hit time
    public float hitComboWindow;   // should be less than hit time

    Coroutine hitCoroutine = null;
    
    private float hitTimeStamp = 0; // the time when the player attacked
    private float comboTimeStamp = 0; // the time when the player attacked

    //health stuff
    public int maxhealth;
    private int currentHealth;
    public float knockBackForce;

    Coroutine pausePhysicsCoroutine = null;
    Coroutine pauseInputMoveCoroutine = null;
    Coroutine pauseInputAttackCoroutine = null;
    public float deathTime;


    [HideInInspector]public SpriteAnimator animator;

    private bool FacingRight => transform.rotation.y == 0;




    //[SerializeField] private float bouncyJumpTimeModifier;
    //[SerializeField] private float bouncyJumpSpeedModifier;

    //for firing the Essence Absorb bullet
    //public GameObject bullet;
    //public Transform firePosition;
    //public float firingSpeed;
    //private Vector2 lookDirection;
    //private float lookAngle;
    //private float shootCooldownCounter;
    //public float shootCooldown;
    //for escaping a squit
    //private Vector3 escapeOffset = new Vector3(-0.5f,0.5f,0f);
    //for the essence that is absorbed;
    //public GameObject essence;

    private Vector3 checkpoint;

    //sfx
    //[SerializeField] private AudioSource shotSFX;
    //[SerializeField] private AudioSource jumpSFX;

    //animator
    //[SerializeField] private Animator animatorController;

    //for groundChecks
    bool isGrounded;
    bool isHittingCeiling;
    bool IsHittingRightWall;
    bool IsHittingLeftWall;
    // Start is called before the first frame update

    bool canAerialAttack = false;


    bool isGroundPounding = false;
    bool hitState;
    Coroutine attackSequenceCoroutine = null;

    bool PhysicsPaused => myRB.constraints == RigidbodyConstraints2D.FreezeAll;

    float hurtTimeStamp = 0;


    void Awake()
    {
        hitState = false;
        currentHealth = maxhealth;
        //shootCooldownCounter = 0f;
        //essence = null;
        //jumpTimeCountdown = jumpTime;
        myRB = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        //canPlay = true;
        checkpoint = this.gameObject.transform.position;
        Instance = this;

        animator = GetComponent<SpriteAnimator>();
    }

    void Start()
    {
        isGroundPounding = false;
        myRB.gravityScale = originalGravityScale;
        myRB.velocity = Vector2.zero;

        upAttackHitbox.SetActive(false);
        downAttackHitbox.SetActive(false);
        rightAttackHitbox.SetActive(false);
        spinAttackHitbox.SetActive(false);
        radialAttackHitbox.SetActive(false);
        animator.PlayDefault(); //this is for safety's sake
        hurtTimeStamp = -invincibilityTime;
    }

    void Update()
    {
        bool wasGrounded = isGrounded;
        isGrounded = boxCollider2D.IsGrounded(bottomLeftRaycast.position, bottomRightRaycast.position, this.transform.localScale.x, rayCastMagnitude);
        isHittingCeiling = boxCollider2D.IsHittingCeiling(topLeftRaycast.position, topRightRaycast.position, this.transform.localScale.x, rayCastMagnitude);
        IsHittingRightWall = boxCollider2D.IsHittingRightWall(topRightRaycast.position, bottomRightRaycast.position, this.transform.localScale.x, rayCastMagnitude);
        IsHittingLeftWall = boxCollider2D.IsHittingLeftWall(topLeftRaycast.position, bottomLeftRaycast.position, this.transform.localScale.x, rayCastMagnitude);
        if (isGrounded)
        {
            if (isGroundPounding)   // if is falling fast
            {
                CameraController.Instance.HorShake(3);          // shake screen
                DoPhysicsPause(.2F);
                isGroundPounding = false;
            }

            myRB.gravityScale = originalGravityScale;
            if (radialAttackHitbox.activeSelf)
            {
                radialAttackHitbox.SetActive(false);
                DoPhysicsPause(.05F);
                animator.PlayDefault();
            }
            canAerialAttack = true;

            if (!wasGrounded)
            {
                upAttackHitbox.SetActive(false);
                myRB.velocity = new Vector2(myRB.velocity.x, 0);
                SnapToPixel();
            }
        }

        if (inputMovePaused)
        {
            if (spinAttackHitbox.activeSelf)
            {
                myRB.velocity = new Vector2(FacingRight ? 3.0F : -3.0F, myRB.velocity.y);
            }

            if (rightAttackHitbox.activeSelf && isGrounded)
            {
                if (Time.time - hitTimeStamp > .15F)
                {
                    myRB.velocity = new Vector2(0, myRB.velocity.y);
                }
            }
        }
        else
        {
            
            myRB.velocity = new Vector2(0, myRB.velocity.y);
            if (PlayerInput.IsPressingLeft())
            {
                myRB.velocity = new Vector2(-moveSpeed, myRB.velocity.y);
                this.transform.rotation = new Quaternion(0f, 180f, this.transform.rotation.z, this.transform.rotation.w);
            }
            else if (PlayerInput.IsPressingRight())
            {
                myRB.velocity = new Vector2(moveSpeed, myRB.velocity.y);
                this.transform.rotation = new Quaternion(0f, 0f, this.transform.rotation.z, this.transform.rotation.w);
            }
            else            
            {
                SnapToPixel();
            }
        }

        if (!hitState)
        {
            myRB.velocity = new Vector2(0f, myRB.velocity.y);
            if (PlayerInput.IsPressingLeft())
            {
                myRB.velocity = new Vector2(-moveSpeed, myRB.velocity.y);
                this.transform.rotation = new Quaternion(0f, 180f, this.transform.rotation.z, this.transform.rotation.w);
            }
            else if (PlayerInput.IsPressingRight())
            {
                myRB.velocity = new Vector2(moveSpeed, myRB.velocity.y);
                this.transform.rotation = new Quaternion(0f, 0f, this.transform.rotation.z, this.transform.rotation.w);
            }

            if (PlayerInput.IsPressingDown())
            {
                myRB.velocity = new Vector2(0, myRB.velocity.y);
            }

            if (isGrounded && PlayerInput.HasPressedJumpKey())
            {
                isJumping = true;
                jumpTimeCountdown = jumpTime;
                myRB.velocity = new Vector2(myRB.velocity.x, jumpSpeed);
                SoundSystem.PlaySfx(sfx_jump, 2);
            }

            if (isJumping && PlayerInput.HasHeldJumpKey())
            {
                if (jumpTimeCountdown > 0)
                {
                    myRB.velocity = new Vector2(myRB.velocity.x, jumpSpeed);
                    jumpTimeCountdown -= Time.deltaTime;
                    if (isHittingCeiling)
                    {
                        jumpTimeCountdown = 0;
                    }
                }
                else
                {
                    isJumping = false;
                }
            }

            if (PlayerInput.IsPressingDown())
            {
                myRB.velocity = new Vector2(0, myRB.velocity.y);
            }

            if (isGrounded && PlayerInput.HasPressedJumpKey())
            {
                isJumping = true;
                jumpTimeCountdown = jumpTime;
                myRB.velocity = new Vector2(myRB.velocity.x, jumpSpeed);
                SoundSystem.PlaySfx(sfx_jump, 2);
            }

            if (isJumping && PlayerInput.HasHeldJumpKey())
            {
                if (jumpTimeCountdown > 0)
                {
                    myRB.velocity = new Vector2(myRB.velocity.x, jumpSpeed);
                    jumpTimeCountdown -= Time.deltaTime;
                    if (isHittingCeiling)
                    {
                        jumpTimeCountdown = 0;
                    }
                }
                else
                {
                    isJumping = false;
                }
            }


            if (PlayerInput.HasReleasedJumpKey())
            {
                isJumping = false;
            }
        }

        AttackInputs();


        if (PlayerInput.HasPressedEscapeKey())
        {
            PauseMenu.Instance.ActivateMenu();
        }


        Animate();
    }

    void SnapToPixel()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Round(pos.x / Game.PIXEL) * Game.PIXEL;
        pos.y = Mathf.Round(pos.y / Game.PIXEL) * Game.PIXEL;
        transform.position = pos;
    }


    void FixedUpdate()
    {
        if (Time.time - hurtTimeStamp < invincibilityTime)
        {
            animator.Ren.enabled = !animator.Ren.enabled;
        }
        else
        {
            animator.Ren.enabled = true;
        }

        HUD.Instance.Flash(animator.Ren.enabled);

        if(Time.time - hurtTimeStamp >= invincibilityTime / invincibilityTimeFactor)
        {
            hitState = false;
        }
    }


    void Animate()
    {
        if (isGrounded)
        {
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
                animator.Play(AnimMode.Looped, "run");
            }
            else if (!PhysicsPaused)
            {
                animator.Play(AnimMode.Looped, "idle");
            }
        }
        else
        {
            if (spinAttackHitbox.activeSelf)
            {
                animator.Play(AnimMode.Looped, "spin");
            }
            else if (downAttackHitbox.activeSelf)
            {
                animator.Play(AnimMode.Hang, "downslash");
            }
            else if (radialAttackHitbox.activeSelf)
            {
                animator.Play(AnimMode.Looped, "somersault");
            }
            else if (upAttackHitbox.activeSelf)
            {
                animator.Play(AnimMode.Hang, "upslash air");
            }
            else if (myRB.velocity.y > 0)
            {
                animator.Play(AnimMode.Hang, "jump");
            }
            else
            {
                animator.Play(AnimMode.Hang, "fall");
            }

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

    public void TakeKnockBack()
    {
        if (!FacingRight)
        {
            myRB.velocity = new Vector2(-knockBackForce, myRB.velocity.y);
        }
        else
        {
            myRB.velocity = new Vector2(knockBackForce, myRB.velocity.y);
        }
    }

    public void TakeDamage()
    {
        hitState = true;

        if (Time.time - hurtTimeStamp >= invincibilityTime)
        {
            hurtTimeStamp = Time.time;
            TakeKnockBack();
            SoundSystem.PlaySfx(sfx_hurt, 3);
            
            currentHealth--;
            if(currentHealth <= 0)
            {
                //for now this is death
                StartCoroutine(Die());
            }
        }
        /*if (Time.time - hurtTimeStamp >= invincibilityTime / invincibilityTimeFactor)
        {
            //myRB.velocity = new Vector2(0f, myRB.velocity.y);
            if (!FacingRight)
            {
                myRB.velocity = new Vector2(-knockBackForce, myRB.velocity.y);
            }
            else
            {
                myRB.velocity = new Vector2(knockBackForce, myRB.velocity.y);
            }
        }*/
    }

    private void AttackInputs()
    {
        
        if (Time.time - comboTimeStamp > hitComboWindow)
            consecutiveHits = 0;

        if (PlayerInput.HasPressedAttackKey())
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

        float hitMod = 1.0F;

        upAttackHitbox.SetActive(false);
        downAttackHitbox.SetActive(false);
        rightAttackHitbox.SetActive(false);
        spinAttackHitbox.SetActive(false);
        radialAttackHitbox.SetActive(false);
            
        switch (hitBoxIndex)
        {
            case 1:
                // Upwards attack
                upAttackHitbox.SetActive(true);
                if (!isGrounded)
                    hitMod = 1.2F;
                else
                    hitMod = .85F;
                break;
            case 2:
                //Downwards attack
                downAttackHitbox.SetActive(true);
                break;
            case 3:
                //Spin attack
                //take the leftAttackHitBox and turn that into the spin attack that hits both the left and the right of the player
                spinAttackHitbox.SetActive(true);
                hitMod = 2;
                break;
            case 4:
                //Right attack
                rightAttackHitbox.SetActive(true);

                break;
            case 5:
                //radial attack
                radialAttackHitbox.SetActive(true);
                hitMod = 2;
                break;
            default:
                Debug.Log("AN ERROR HAS OCCURRED WHILE TRYING TO ATTACK");
            // Statements to Execute if No Case Matches
            break;
        }



        yield return new WaitForSeconds(hitTime * hitMod);   //wait the established amount of seconds.

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
            case 5:
                //radial attack
                radialAttackHitbox.SetActive(false);
                break;
            default:
                Debug.Log("AN ERROR HAS OCCURRED WHILE TRYING TO ATTACK");
                // Statements to Execute if No Case Matches
                break;
        }
        isHitting = false;                          //set isHitting to false.

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground") || other.CompareTag("Hidden"))
        {
            //checkpoint = new Vector2(other.gameObject.transform.position.x, other.gameObject.transform.position.y + 1);
        }
        if (other.CompareTag("Reset"))
        {
            this.gameObject.transform.position = checkpoint;
        }

    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Teleport"))
        {
            if (PlayerInput.IsPressingUp())
            {
                other.gameObject.GetComponent<EndLevelChecker>().HandleEndOfLevel();
            }
        }
        if (other.gameObject.GetComponent<Collider2D>().CompareTag("EnemyMelee"))
        {
            other.gameObject.GetComponent<Collider2D>().transform.parent.GetComponent<MeleeEnemy>().jumpBackwardsMode = true;
            TakeDamage();
            //also take knockback
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.GetComponent<Collider2D>().CompareTag("Enemy"))
        {
            TakeDamage();
            //also take knockback
        }
    }

    /*private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Squit"))
        {
            this.gameObject.transform.position = other.GetComponent<SquitController>().topRightVertexEscape.position + escapeOffset;
        }
    }*/
    private IEnumerator Die()
    {
        //animatorController.SetBool("playerDed", true);
        yield return new WaitForSeconds(deathTime);   //wait the established amount of seconds.

        if (Game.lives > 0)
        {
            Game.lives--;
            ResetToLastCheckPoint();
        }
        else
        {
            // game over
        }

    }
    public void ResetToLastCheckPoint()
    {
        this.gameObject.transform.position = checkpoint;
        currentHealth = maxhealth;
        Start();
        //animatorController.SetBool("playerDed", false);
    }

    IEnumerator PausePhysics(float duration)
    {
        Vector2 velocity = myRB.velocity;
        myRB.constraints = RigidbodyConstraints2D.FreezeAll;
        yield return new WaitForSeconds(duration);
        myRB.constraints = RigidbodyConstraints2D.FreezeRotation;
        pausePhysicsCoroutine = null;
        myRB.velocity = velocity;
        SnapToPixel();
        yield break;
    }

    IEnumerator PauseInputMove(float duration)
    {
        inputMovePaused = true;
        myRB.velocity = Vector2.zero;
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
                myRB.velocity = new Vector2(FacingRight ? 4F : -4F, myRB.velocity.y);
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
                
                myRB.velocity = Vector3.down * 38;
                isGroundPounding = true;
                myRB.gravityScale = - 0; 
                if (hitCoroutine != null)
                    StopCoroutine(hitCoroutine);
                hitCoroutine = StartCoroutine(Hit(2));
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
            }
            else
            {
                myRB.velocity = Vector3.up * 15;
                myRB.gravityScale = originalGravityScale * .97F;
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
                consecutiveHits++;

                switch (consecutiveHits)
                {
                    case 1:
                        SoundSystem.PlaySfx(sfx_swordSounds[0], 4);   //play attack sfx
                        DoPhysicsPause(.08F);
                        DoInputMovePause(.3F);
                        myRB.velocity = Vector2.zero;
                        if (hitCoroutine != null)
                            StopCoroutine(hitCoroutine);
                        hitCoroutine = StartCoroutine(Hit(4));
                        
                        break;
                    case 2:
                        SoundSystem.PlaySfx(sfx_swordSounds[1], 4);   //play attack sfx

                        DoPhysicsPause(.08F);
                        DoInputMovePause(.3F);
                        myRB.velocity = Vector2.zero;
                        if (hitCoroutine != null)
                            StopCoroutine(hitCoroutine);
                        hitCoroutine = StartCoroutine(Hit(4));
                        
                        break;
                    case 3: //do spin attack here and reset the other trigger here
                        SoundSystem.PlaySfx(sfx_swordSounds[3], 4);
                        DoInputMovePause(.5F);
                        DoInputAttackPause(.5F);
                        if (hitCoroutine != null)
                            StopCoroutine(hitCoroutine);
                        hitCoroutine = StartCoroutine(Hit(3));
                        consecutiveHits = 0;
                        break;

                }
                comboTimeStamp = Time.time;
                hitTimeStamp = Time.time;
            }
            else
            {
                // radial spin
                SoundSystem.PlaySfx(sfx_swordSounds[3], 4);
                if (hitCoroutine != null)
                    StopCoroutine(hitCoroutine);
                hitCoroutine = StartCoroutine(Hit(5));
            }
        }

        attackSequenceCoroutine = null;
        yield break;
    }
}
