using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD : MonoBehaviour
{
    static public HUD Instance;

    Dictionary<string, SpriteRenderer> renderers = new Dictionary<string, SpriteRenderer>();
    Dictionary<string, TMPro.TMP_Text> texts = new Dictionary<string, TMPro.TMP_Text>();

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

        texts["Main Text Layer"].enabled = false;
        renderers["candle 0"].enabled = false;
        renderers["candle 1"].enabled = false;
        renderers["candle 2"].enabled = false;
        renderers["candle 3"].enabled = false;
    }

    void FixedUpdate()
    {
        texts["Lives Text Layer"].text = "" + Game.lives;

        renderers["candle 0"].enabled = false;
        renderers["candle 1"].enabled = false;
        renderers["candle 2"].enabled = false;
        renderers["candle 3"].enabled = false;

        renderers["hp 0"].enabled = false;
        renderers["hp 1"].enabled = false;
        renderers["hp 2"].enabled = false;

        if (flash)
        {
            for (int i = 0; i < 4 && i < Game.litCandlesCount; i++)
            {
                renderers["candle " + i].enabled = true;
            }


            for (int i = 0; i < 3 && i < Player.Hp; i++)
            {
                renderers["hp " + i].enabled = true;
            }

        }
    }

    public void Flash(bool visible)
    {
        if (flash == visible)   return;

        flash = visible;
        foreach (SpriteRenderer ren in renderers.Values)
            if (ren.gameObject.name != "BG")
                ren.enabled = visible;

        foreach (TMPro.TMP_Text txt in texts.Values)
            if (txt.gameObject.name != "Main Text Layer")
                txt.enabled = visible;
    }
}
