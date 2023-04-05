using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Pawn : MonoBehaviour
{
    protected Rect spawnerBounds;
    public void SetBounds(Rect rect) => spawnerBounds = rect;

    protected List<Collider2D> hits;
    protected List<HitInfo> hitsGround;
    protected List<HitInfo> hitsCeiling;
    protected List<HitInfo> hitsLeft;
    protected List<HitInfo> hitsRight;
    [HideInInspector]public Rigidbody2D body;
    [HideInInspector]public BoxCollider2D boxCollider;
    [HideInInspector]public List<string> collidableTags;

    public virtual void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        collidableTags = new List<string>();
    }

    public virtual void Start()
    {
        hits = new List<Collider2D>();
        hitsGround = new List<HitInfo>();
        hitsCeiling = new List<HitInfo>();
        hitsLeft = new List<HitInfo>();
        hitsRight = new List<HitInfo>();
    }

    protected virtual void Update()
    {
        if (Game.isPaused)  return;

        PhysicsPass();
    }

    protected virtual void PhysicsPass()
    {
        // check to remove old positive collisions every frame as opposed to OnCollisionExit2D
        
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


    public HitInfo GroundCheck(params string[] tags) => HitCheck(hitsGround, new List<string>(tags));
    public HitInfo GroundCheck(List<string> tags) => HitCheck(hitsGround, tags);

    public HitInfo CeilingCheck(params string[] tags) => HitCheck(hitsCeiling, new List<string>(tags));
    public HitInfo CeilingCheck(List<string> tags) => HitCheck(hitsCeiling, tags);

    public HitInfo LeftCheck(params string[] tags) => HitCheck(hitsLeft, new List<string>(tags));
    public HitInfo LeftCheck(List<string> tags) => HitCheck(hitsLeft, tags);

    public HitInfo RightCheck(params string[] tags) => HitCheck(hitsRight, new List<string>(tags));
    public HitInfo RightCheck(List<string> tags) => HitCheck(hitsRight, tags);

    private bool GroundHitExists(Collider2D collider, Vector2 contact)
    {
        foreach (HitInfo hit in hitsGround)
        {
            if (hit.collider == collider && boxCollider.CheckHitDown(contact))
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

                if (contact.point.y < transform.position.y &&
                contact.point.x >= boxCollider.Left() &&
                contact.point.x <= boxCollider.Right() &&
                !GroundHitExists(other.collider, contact.point))
                    hitsGround.Add(new HitInfo(other.collider, contact.point));

                if (contact.point.y > transform.position.y &&
                contact.point.x >= boxCollider.Left() &&
                contact.point.x <= boxCollider.Right() &&
                !CeilingHitExists(other.collider, contact.point))
                    hitsCeiling.Add(new HitInfo(other.collider, contact.point));

                if (contact.point.x < transform.position.x &&
                contact.point.y >= boxCollider.Bottom() &&
                contact.point.y <= boxCollider.Top() &&
                !LeftHitExists(other.collider, contact.point))
                    hitsLeft.Add(new HitInfo(other.collider, contact.point));

                if (contact.point.x > transform.position.x &&
                contact.point.y >= boxCollider.Bottom() &&
                contact.point.y <= boxCollider.Top() &&
                !RightHitExists(other.collider, contact.point))
                    hitsRight.Add(new HitInfo(other.collider, contact.point));                
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
