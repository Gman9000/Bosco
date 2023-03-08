using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knaz : MonoBehaviour
{
    public static Knaz Instance;

    Vector3 place;
    void Start()
    {
        Instance = this;
        place = transform.position;
        transform.position += Vector3.up * 50;
    }

    
    static public void DoScene()
    {
        Instance.StartCoroutine(Instance.SceneTime());
    }

    IEnumerator SceneTime()
    {
        while (transform.position.y > place.y)
        {
            transform.position += Vector3.down * Game.PIXEL * 20;
            yield return new WaitForFixedUpdate();
            yield return null;
        }        
        transform.position = place;
        Game.VertShake(10);
        yield return new WaitForSeconds(.75F);
        GetComponent<SpriteAnimator>().Play(AnimMode.Looped, "laugh");
        yield return new WaitForSeconds(2);
        GetComponent<SpriteAnimator>().Play(AnimMode.Hang, "idle");
        yield return new WaitForSeconds(.725F);
        HUD.Write("\n\n\n\n\n\n  TO BE CONTINUED...");
    }
}
