using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Pawn : MonoBehaviour
{
    static public bool skitMode = false;
    static public Dictionary<string, Pawn> Instances;
    protected Rect spawnerBounds;    
    public void SetBounds(Rect rect) => spawnerBounds = rect;
    public string pawnID = "";
    public int facingDirection
    {
        get => (int)transform.rotation.y == 0 ? 1 : -1;
        set => transform.rotation = new Quaternion(0F, value > 0 ? 0 : 180F, transform.rotation.z, transform.rotation.w);
    }

    protected List<Collider2D> hits;
    protected List<HitInfo> hitsGround;
    protected List<HitInfo> hitsCeiling;
    protected List<HitInfo> hitsLeft;
    protected List<HitInfo> hitsRight;
    protected List<HitInfo> hitsSlope;

    protected HitInfo footingHit;   // the results of a ray placed slightly in front of where the player is going on the x-axis, facing downward
    protected HitInfo nearestGroundHit;   // the results of a ray placed directly in the bottom-center of the player, facing downward
    

    [HideInInspector]public SpriteAnimator animator;
    [HideInInspector]public Rigidbody2D body;
    [HideInInspector]public BoxCollider2D boxCollider;
    [HideInInspector]public List<string> collidableTags;

    public virtual void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        collidableTags = new List<string>();
        animator = GetComponent<SpriteAnimator>();
    }

    public virtual void Start()
    {
        hits = new List<Collider2D>();
        hitsGround = new List<HitInfo>();
        hitsCeiling = new List<HitInfo>();
        hitsLeft = new List<HitInfo>();
        hitsRight = new List<HitInfo>();
        hitsSlope = new List<HitInfo>();

        footingHit = new HitInfo();
        nearestGroundHit = new HitInfo();

        if (pawnID != "" && pawnID != null)
            Instances.Add(pawnID, this);
    }

    private void Update()
    {
        if (Game.isPaused)  return;

        PhysicsPass();
        ActionUpdate();
    }

    protected virtual void ActionUpdate()
    {

    }

    void OnDestroy()
    {
        Instances.Remove(pawnID);
    }

    protected virtual void PhysicsPass()
    {       

        // ADDITIONAL RAYCAST COLLISION LOGIC //
        RaycastHit2D[] rayhits;

        // > calculate and store footing data for slopes and such
        Vector2 footPoint = new Vector2(boxCollider.Center().x + facingDirection, boxCollider.Bottom());
        rayhits = Physics2D.RaycastAll(footPoint, Vector2.down, 1.5F);
        Debug.DrawRay(footPoint, Vector2.down * 1.5F, Color.red);

        footingHit = new HitInfo();
        foreach (RaycastHit2D rayhit in rayhits)
            if (rayhit.collider != null && collidableTags.Contains(rayhit.collider.tag))
                footingHit = new HitInfo(rayhit.collider, rayhit.normal, rayhit.point);


        // > calculate and store footing data for slopes and such
        Vector2 bottomPoint = new Vector2(boxCollider.Center().x, boxCollider.Bottom());
        rayhits = Physics2D.RaycastAll(bottomPoint, Vector2.down, 3);
        Debug.DrawRay(bottomPoint, Vector2.down * 3F, Color.cyan);

        nearestGroundHit = new HitInfo();
        foreach (RaycastHit2D rayhit in rayhits)
        {
            if (rayhit.collider != null && rayhit.collider.CompareTag("Ground"))
            {
                nearestGroundHit = new HitInfo(rayhit.collider, rayhit.normal, rayhit.point);
            }
        }


        // Check to remove old positive collisions every frame as opposed to OnCollisionExit2D //        
        for (int i = hitsGround.Count - 1; i >= 0; i--)
        {
            HitInfo hit = hitsGround[i];

            if (!boxCollider.CheckHitDown(hit.contact))
            {
                hitsGround.RemoveAt(i);
            }
        }

        for (int i = hitsCeiling.Count - 1; i >= 0; i--)
        {
            HitInfo hit = hitsCeiling[i];

            if (!boxCollider.CheckHitUp(hit.contact))
            {
                hitsCeiling.RemoveAt(i);
            }
        }

        for (int i = hitsLeft.Count - 1; i >= 0; i--)
        {
            HitInfo hit = hitsLeft[i];

            if (!boxCollider.CheckHitLeft(hit.contact))
            {
                hitsLeft.RemoveAt(i);
            }
        }

        for (int i = hitsRight.Count - 1; i >= 0; i--)
        {
            HitInfo hit = hitsRight[i];

            if (!boxCollider.CheckHitRight(hit.contact))
            {
                hitsRight.RemoveAt(i);
            }
        }

        for (int i = hitsSlope.Count - 1; i >= 0; i--)
        {
            HitInfo hit = hitsSlope[i];

            if (!boxCollider.CheckHitSlope(hit.normal, hit.contact))
            {
                hitsSlope.RemoveAt(i);
            }
        }
    }

    public HitInfo HitCheck(List<HitInfo> hitList, List<string> tagList)
    {
        foreach (HitInfo hit in hitList)
        {
            if (tagList.Contains(hit.collider.tag))
            {
                return hit;
            }
        }

        return new HitInfo();
    }


    public void CommandInSkit(string command, string[] parameters)
    {
        // todo: implemtation

        switch (command)
        {
            case "animate":
                AnimMode mode = AnimMode.None;

                if (parameters[0] == "loop")
                    mode = AnimMode.Looped;
                if (parameters[0] == "hang")
                    mode = AnimMode.Hang;
                string animationName = parameters[1];

                if (mode != AnimMode.None)
                    animator.Play(mode, animationName);
                else
                    Debug.LogError("parsing error. no such AnimMode as \"" + parameters[0] + "\"");
                break;

            case "move":
                // todo movement command logic
                break;
        }
    }

    public HitInfo GroundCheck(params string[] tags) => HitCheck(hitsGround, new List<string>(tags));
    public HitInfo GroundCheck(List<string> tags) => HitCheck(hitsGround, tags);

    public HitInfo CeilingCheck(params string[] tags) => HitCheck(hitsCeiling, new List<string>(tags));
    public HitInfo CeilingCheck(List<string> tags) => HitCheck(hitsCeiling, tags);

    public HitInfo LeftCheck(params string[] tags) => HitCheck(hitsLeft, new List<string>(tags));
    public HitInfo LeftCheck(List<string> tags) => HitCheck(hitsLeft, tags);

    public HitInfo RightCheck(params string[] tags) => HitCheck(hitsRight, new List<string>(tags));
    public HitInfo RightCheck(List<string> tags) => HitCheck(hitsRight, tags);

    public HitInfo SlopeCheck(params string[] tags) => HitCheck(hitsSlope, new List<string>(tags));
    public HitInfo SlopeCheck(List<string> tags) => HitCheck(hitsSlope, tags);

    private bool GroundHitExists(Collider2D collider, Vector2 contact)
    {
        foreach (HitInfo hit in hitsGround)
        {
            if (hit.collider == collider && boxCollider.CheckHitDown(contact))
                return true;
        }
        return false;
    }
    private bool SlopeHitExists(Collider2D collider, Vector2 contact)
    {
        foreach (HitInfo hit in hitsSlope)
        {
            if (hit.collider == collider && boxCollider.CheckHitSlope(hit.normal, hit.contact))
                return true;
        }
        return false;
    }
    private bool CeilingHitExists(Collider2D collider, Vector2 contact)
    {
        foreach (HitInfo hit in hitsCeiling)
        {
            if (hit.collider == collider && boxCollider.CheckHitUp(contact))
                return true;
        }
        return false;
    }
    private bool LeftHitExists(Collider2D collider, Vector2 contact)
    {
        foreach (HitInfo hit in hitsLeft)
        {
            if (hit.collider == collider && boxCollider.CheckHitLeft(contact))
                return true;
        }
        return false;
    }
    private bool RightHitExists(Collider2D collider, Vector2 contact)
    {
        foreach (HitInfo hit in hitsRight)
        {
            if (hit.collider == collider && boxCollider.CheckHitRight(contact))
                return true;
        }
        return false;
    }

    protected virtual void OnCollisionStay2D(Collision2D other)
    {
        foreach (ContactPoint2D contact in other.contacts)
        {        
            if (collidableTags.Contains(contact.collider.tag))
            {
                if (!hits.Contains(contact.collider))
                    hits.Add(contact.collider);

                Vector2 point = contact.point;
                Vector2 normal = contact.normal;

                if (boxCollider.CheckHitSlope(normal, point) &&
                !SlopeHitExists(other.collider, contact.point) &&
                other.collider.gameObject.CompareTag("Ground"))
                    hitsSlope.Add(new HitInfo(other.collider, contact.normal, contact.point));

                
                if (contact.point.y < transform.position.y &&
                contact.point.x >= boxCollider.Left() &&
                contact.point.x <= boxCollider.Right() &&
                !GroundHitExists(other.collider, contact.point))
                    hitsGround.Add(new HitInfo(other.collider, contact.normal, contact.point));

                if (contact.point.y > transform.position.y &&
                contact.point.x >= boxCollider.Left() &&
                contact.point.x <= boxCollider.Right() &&
                !CeilingHitExists(other.collider, contact.point))
                    hitsCeiling.Add(new HitInfo(other.collider, contact.normal, contact.point));

                if (contact.point.x < transform.position.x &&
                contact.point.y >= boxCollider.Bottom() &&
                contact.point.y <= boxCollider.Top() &&
                !LeftHitExists(other.collider, contact.point))
                    hitsLeft.Add(new HitInfo(other.collider, contact.normal, contact.point));

                if (contact.point.x > transform.position.x &&
                contact.point.y >= boxCollider.Bottom() &&
                contact.point.y <= boxCollider.Top() &&
                !RightHitExists(other.collider, contact.point))
                    hitsRight.Add(new HitInfo(other.collider, contact.normal, contact.point));                
            }
        }
    }

    protected virtual void OnCollisionExit2D(Collision2D other)
    {
        if (hits.Contains(other.collider))
            hits.Remove(other.collider);
    }

    private void MoveToContact(Vector2 contact, Vector2 direction)
    {
        if (direction.y < 0)
        {
            body.MovePosition(new Vector2(transform.position.x, contact.y + (transform.position.y - boxCollider.Bottom())));
        }
    }
}
