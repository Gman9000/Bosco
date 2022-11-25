using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; protected set; }
    private Rigidbody2D myRB;
    public float moveSpeed;
    //private bool canPlay;
    private BoxCollider2D boxCollider2D;

    //for jumping
    public float originalJumpSpeed;
    private float jumpSpeed;
    public float jumpTime;
    private float jumpTimeCountdown;
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
    [SerializeField] private Animator animatorController;

    //for groundChecks
    bool isGrounded;
    // Start is called before the first frame update
    void Awake()
    {
        //shootCooldownCounter = 0f;
        //essence = null;
        //jumpTimeCountdown = jumpTime;
        myRB = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        //canPlay = true;
        checkpoint = this.gameObject.transform.position;
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        animatorController.SetFloat("horizontalVelocity", myRB.velocity.x);
        animatorController.SetFloat("verticalVelocity", myRB.velocity.y);
        //animatorController.ResetTrigger("shoot");
        /*if (shootCooldownCounter >= 0f)
        {
            shootCooldownCounter -= Time.deltaTime;
        }*/
        isGrounded = boxCollider2D.IsGrounded(myRB.position, this.transform.localScale.x);
        //myRB.velocity = new Vector2(0f, 0f);
        myRB.velocity = new Vector2(0f, myRB.velocity.y);
        //if (LevelManager.Instance.GetGameStartStatus() && !PauseMenu.Instance.Paused())
        //{
            if (PlayerInput.IsPressingLeft())
            {
                myRB.velocity = new Vector2(-moveSpeed, myRB.velocity.y);
                this.transform.rotation = new Quaternion(0f, 180f, this.transform.rotation.z, this.transform.rotation.w);
                animatorController.SetFloat("horizontalVelocity", Mathf.Abs(myRB.velocity.x));
            }

            if (PlayerInput.IsPressingRight())
            {
                myRB.velocity = new Vector2(moveSpeed, myRB.velocity.y);
                this.transform.rotation = new Quaternion(0f, 0f, this.transform.rotation.z, this.transform.rotation.w);
                animatorController.SetFloat("horizontalVelocity", myRB.velocity.x);
            }

            if (PlayerInput.HasPressedJumpKey())
            {
                //Debug.Log("We pressed jump");
                //Debug.Log("Grounded: " + isGrounded);
                //Debug.Log("Bouncy Grounded: " + isOnBouncyGround);
                if (isGrounded)
                {
                    //jumpSFX.Play();
                    //myRB.AddForce(Vector2.up * jumpSpeed,ForceMode2D.Impulse);
                    //myRB.velocity = new Vector2(myRB.velocity.x, jumpSpeed);
                    //myRB.velocity = new Vector2(myRB.velocity.x, jumpSpeed);
                    jumpSpeed = originalJumpSpeed;
                    jumpTimeCountdown = jumpTime;
                }
            }
            /*if (PlayerInput.HasPressedAttackKey())
            {
                shotSFX.Play();
                lookDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
                lookAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
                firePosition.rotation = Quaternion.Euler(0f, 0f, lookAngle - 90f);
                GameObject firedBullet = Instantiate(bullet, firePosition.position, firePosition.rotation);
                firedBullet.GetComponent<Rigidbody2D>().velocity = firePosition.up * firingSpeed;

                //do something
            }*/

            /*if (PlayerInput.HasPressedAbsorbPlacementKey() && essence == null && shootCooldownCounter <= 0f)
            {
                shotSFX.Play();
                animatorController.SetTrigger("shoot");
                lookDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
                lookAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
                firePosition.rotation = Quaternion.Euler(0f, 0f, lookAngle - 90f);
                GameObject firedBullet = Instantiate(bullet, firePosition.position, firePosition.rotation);
                firedBullet.GetComponent<Rigidbody2D>().velocity = firePosition.up * firingSpeed;
                shootCooldownCounter = shootCooldown;
                //do something
            }

            if (PlayerInput.HasPressedAbsorbPlacementKey() && essence != null && shootCooldownCounter <= 0f)
            {
                shotSFX.Play();
                animatorController.SetTrigger("shoot");
                lookDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
                lookAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
                firePosition.rotation = Quaternion.Euler(0f, 0f, lookAngle - 90f);
                GameObject firedBullet = Instantiate(bullet, firePosition.position, firePosition.rotation);
                firedBullet.GetComponent<Rigidbody2D>().velocity = firePosition.up * firingSpeed;
                shootCooldownCounter = shootCooldown;
                //do something
            }*/

            if (PlayerInput.HasPressedResetKey())
            {
                LevelManager.Instance.ResetToLastCheckPoint();
            }

            if (PlayerInput.HasPressedEscapeKey())
            {
                PauseMenu.Instance.ActivateMenu();
            }

            if (jumpTimeCountdown > 0f)
            {
                jumpTimeCountdown -= Time.deltaTime;
                //myRB.gravityScale = 0;
                myRB.velocity = new Vector2(myRB.velocity.x, jumpSpeed);
                animatorController.SetFloat("verticalVelocity", Mathf.Abs(myRB.velocity.y));
            }
        //}
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            checkpoint = new Vector2(other.gameObject.transform.position.x, other.gameObject.transform.position.y + 1);
        }
        if (other.CompareTag("Reset"))
        {
            this.gameObject.transform.position = checkpoint;
        }
    }
    /*private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Squit"))
        {
            this.gameObject.transform.position = other.GetComponent<SquitController>().topRightVertexEscape.position + escapeOffset;
        }
    }*/

    public void ResetToLastCheckPoint()
    {
        this.gameObject.transform.position = checkpoint;
    }
}
