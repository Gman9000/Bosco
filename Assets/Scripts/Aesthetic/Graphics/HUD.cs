using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD : MonoBehaviour
{
    public static HUD Instance;
    [HideInInspector] public Dictionary<string, SpriteRenderer> renderers = new Dictionary<string, SpriteRenderer>();
    [HideInInspector] public Dictionary<string, TMPro.TMP_Text> texts = new Dictionary<string, TMPro.TMP_Text>();

    private bool flash;
    private Coroutine skitCoroutine;

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
        
        Write("");
    }

    void Start()
    {
        flash = true;
        skitCoroutine = null;
    }

    void FixedUpdate()
    {

    }

    void Update()
    {
        if (PlayerInput.Pressed(Button.Select))
        {
            PerformSkit("Template");
        }

        if (SkitRunner.active)
        {
            Pawn.skitMode = true;
            renderers["Dialogue Box"].enabled = true;            
            renderers["Name Box"].enabled = true; 

            texts["Dialogue Text"].enabled = true;

            if (PlayerInput.Pressed(Button.A) || PlayerInput.Pressed(Button.B) )      // skip dialogue
                SkitRunner.currentSkit.currentBeat.complete = true;
        }
        else
        { 
            Pawn.skitMode = false;

            renderers["Dialogue Box"].enabled = false;
            renderers["Name Box"].enabled = false;

            texts["Dialogue Text"].enabled = false;

            if (skitCoroutine != null)
            {
                StopCoroutine(skitCoroutine);
                skitCoroutine = null;
            }
        }
    }

    public static void PerformSkit(string skitName)
    {
        SkitRunner.active = true;
        if (Instance.skitCoroutine == null)
            Instance.skitCoroutine = Instance.StartCoroutine(SkitRunner.BeginSkit(skitName));
    }

    public static void Write(string str)
    {
        TMPro.TMP_Text txt = Instance.texts["Main Text Layer"];
        txt.text = str;
        txt.enabled = str != null && str.Length > 0;
    }
}
