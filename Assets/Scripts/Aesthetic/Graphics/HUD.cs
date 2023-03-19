using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD : MonoBehaviour
{
    static public HUD Instance;
    [HideInInspector] public Dictionary<string, SpriteRenderer> renderers = new Dictionary<string, SpriteRenderer>();
    [HideInInspector] public Dictionary<string, TMPro.TMP_Text> texts = new Dictionary<string, TMPro.TMP_Text>();

    bool flash = true;

    void Awake()
    {
        Instance = this;
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject obj = transform.GetChild(i).gameObject;
            SpriteRenderer spr = obj.GetComponent<SpriteRenderer>();
            TMPro.TMP_Text txt = obj.GetComponent<TMPro.TMP_Text>();

            if (spr != null)
            {
                renderers.Add(obj.name, spr);
            }
            if (txt != null)
            {
                texts.Add(obj.name, txt);
            }
        }
        renderers["candle 0"].gameObject.SetActive(false);
        renderers["candle 1"].gameObject.SetActive(false);
        renderers["candle 2"].gameObject.SetActive(false);
        renderers["candle 3"].gameObject.SetActive(false);
        
        Write("");
    }

    void FixedUpdate()
    {
        texts["Lives Text Layer"].text = "" + Game.lives;

        renderers["hp 0"].gameObject.SetActive(false);
        renderers["hp 1"].gameObject.SetActive(false);
        renderers["hp 2"].gameObject.SetActive(false);

        if (flash)
        {
            for (int i = 0; i < 4 && i < Game.litCandlesCount; i++)
            {
                renderers["candle " + i].gameObject.SetActive(true);
            }


            for (int i = 0; i < 3 && i < Player.Hp; i++)
            {
                renderers["hp " + i].gameObject.SetActive(true);
            }
        }

        Game.debugText = "HP: " + Player.Hp;
    }

    void Update()
    {
        if (Game.gameStarted)
        {
            if (!Player.IsHurting || Game.isPaused)
                Flash(!texts["Main Text Layer"].enabled, "Main Text Layer");
            renderers["BG"].enabled = !Game.isPaused;
        }
        else
        {
            Flash(false, "Main Text Layer", "Title", "Bosco Sprite", "Credits");
            renderers["BG"].enabled = false;
        }
    }

    public void Flash(bool visible, params string[] exclude)
    {
        if (flash == visible)   return;

        List<string> exclusions = new List<string>(exclude);

        exclusions.Add("BG");
        exclusions.Add("Title"); //this animated title might cause problems so we are excluding it


        flash = visible;
        foreach (SpriteRenderer ren in renderers.Values)
            if (!exclusions.Contains(ren.gameObject.name))
                ren.enabled = visible;

        foreach (TMPro.TMP_Text txt in texts.Values)
            if (!exclusions.Contains(txt.gameObject.name))
                txt.enabled = visible;
    }

    static public void Write(string str)
    {
        TMPro.TMP_Text txt = Instance.texts["Main Text Layer"];
        txt.text = str;
        txt.enabled = str != null && str.Length > 0;            
    }
}
