using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; protected set; }
    private bool gameStarted;
    public GameObject MainMenu;
    public GameObject WinMenu;
    public TextMeshProUGUI winScreenFinalTime;
    public GameObject mainLevelUI;
    private Vector3 startingLevelPlayerPosition;
    //public string tutorialText;
    // Start is called before the first frame update
    void Start()
    {
        startingLevelPlayerPosition = Player.Instance.transform.position;
        Instance = this;
        gameStarted = false;
        WinMenu.SetActive(false);
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/




    public void ResetToLastCheckPoint()
    {
        Player.Instance.ResetToLastCheckPoint();
        //Player.Instance.ResetEssenceAbsorbed();
    }


    public void WinGame()
    {
        if (Fader.Instance && Fader.Instance.isDoneFading)
        {
            StartCoroutine(TransitionToWinMenu());
        }
        /*Debug.Log("YOU WIN");
        MainMenu.SetActive(false);
        WinMenu.SetActive(true);
        Timer.Instance.timerIsRunning = false;*/

    }

    public void BackToMainMenu()
    {
        if (Fader.Instance && Fader.Instance.isDoneFading)
        {
            StartCoroutine(TransitionToMainMenu());
        }
        /*MainMenu.SetActive(true);
        WinMenu.SetActive(false);*/
    }
    public void PlayGame()
    {

        if (Fader.Instance && Fader.Instance.isDoneFading)
        {
            StartCoroutine(TransitionToMainGame());
        }

    }
    public IEnumerator TransitionToMainMenu()
    {
        StartCoroutine(Fader.Instance.FadeInAndOutBlackOutSquare());
        yield return new WaitForSeconds(1);
        Player.Instance.transform.position = startingLevelPlayerPosition;
        WinMenu.SetActive(false);
        MainMenu.SetActive(true);
        gameStarted = false;
        mainLevelUI.SetActive(false);
        mainLevelUI.SetActive(false);
        PauseMenu.Instance.turnOffPauseUI();
        yield return null;

    }
    public IEnumerator TransitionToWinMenu()
    {
        StartCoroutine(Fader.Instance.FadeInAndOutBlackOutSquare());
        yield return new WaitForSeconds(1);
        WinMenu.SetActive(true);
        MainMenu.SetActive(false);
        gameStarted = false;
        mainLevelUI.SetActive(false);
        yield return null;

    }



    public IEnumerator TransitionToMainGame()
    {
        StartCoroutine(Fader.Instance.FadeInAndOutBlackOutSquare());
        yield return new WaitForSeconds(1);
        MainMenu.SetActive(false);
        gameStarted = true;
        mainLevelUI.SetActive(true);
        yield return null;

    }
    public bool GetGameStartStatus()
    {
        return gameStarted;
    }
}
