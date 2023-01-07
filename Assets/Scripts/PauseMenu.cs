using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; protected set; }
    /*[SerializeField] private Image pauseBackground;
    [SerializeField] private Text gamePaused;
    [SerializeField] private Button resumeGame;
    [SerializeField] private Button returnToMainMenu;*/
    [SerializeField] private GameObject pauseMenu;
    //[SerializeField] private GameObject player;
    private bool isPaused = false;

    //activate the pause menu and enable assets.

    void Awake()
    {
        Instance = this;
    }
    public void ActivateMenu()
    {
        Time.timeScale = 0;
        isPaused = true;
        pauseMenu.SetActive(true);
        /*pauseBackground.gameObject.SetActive(true);
        gamePaused.gameObject.SetActive(true);
        resumeGame.gameObject.SetActive(true);
        returnToMainMenu.gameObject.SetActive(true);*/
    }

    // Start the game
    public void ResumeGame()
    {
        Time.timeScale = 1;
        isPaused = false;
        pauseMenu.SetActive(false);
        /*pauseBackground.gameObject.SetActive(false);
        gamePaused.gameObject.SetActive(false);
        resumeGame.gameObject.SetActive(false);
        returnToMainMenu.gameObject.SetActive(false);*/
    }

    // Return to the main menu
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1;
        isPaused = false;
        if (LevelManager.Instance)
        {
            LevelManager.Instance.BackToMainMenu();
        }
    }

    public void turnOffPauseUI()
    {
        pauseMenu.SetActive(false);
    }

    //returns boolean indicating whether or not the game is paused.
    public bool Paused()
    {
        return isPaused;
    }
}
