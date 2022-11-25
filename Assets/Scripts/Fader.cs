using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fader : MonoBehaviour
{
    public static Fader Instance { get; protected set; } //instance of Fader that can be accessed by other scripts easily
    public SpriteRenderer faderEntity;
    public bool isDoneFading;
    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FadeBlackOutSquare(false));

    }

    public IEnumerator FadeBlackOutSquare(bool fadeToBlack = true, int fadeSpeed = 1)
    {
        Color objectColor = faderEntity.color;
        float fadeAmount;
        isDoneFading = false;
        if (fadeToBlack)
        {

            while (faderEntity.color.a < 1)
            {
                fadeAmount = objectColor.a + (fadeSpeed * Time.deltaTime);

                objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
                faderEntity.color = objectColor;
                yield return null;
            }
            isDoneFading = true;
        }
        else
        {
            while (faderEntity.color.a > 0)
            {
                fadeAmount = objectColor.a - (fadeSpeed * Time.deltaTime);

                objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
                faderEntity.color = objectColor;
                yield return null;
            }
            isDoneFading = true;
        }
    }
    public IEnumerator FadeInAndOutBlackOutSquare(int fadeSpeed = 1)
    {
        //we will always fade to black first and then fade back out later
        Color objectColor = faderEntity.color;
        float fadeAmount;
        isDoneFading = false;
        while (faderEntity.color.a < 1)
        {
            fadeAmount = objectColor.a + (fadeSpeed * Time.deltaTime);
            objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
            faderEntity.color = objectColor;
            yield return null;
        }
        while (faderEntity.color.a > 0)
        {
            fadeAmount = objectColor.a - (fadeSpeed * Time.deltaTime);
            objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
            faderEntity.color = objectColor;
            yield return null;
        }
        isDoneFading = true;
        yield return null;

    }
}
