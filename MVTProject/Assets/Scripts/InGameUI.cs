using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InGameUI : MonoBehaviour
{
    [SerializeField] Button pauseBtn;
    [SerializeField] GameObject optionPanel;
    [SerializeField] GameObject pausePanel;
    [SerializeField] Button restartBtn;
    [SerializeField] Button resumeBtn;
    [SerializeField] Button optionBtn;
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] GameObject clearPanel;
    [SerializeField] Slider bgmVol;
    [SerializeField] Slider seVol;

    private void Start()
    {
        bgmVol.value = PlayerPrefs.GetFloat("bgmVol", 1f);
        seVol.value = PlayerPrefs.GetFloat("sfxVol", 1f);
    }


    private void FixedUpdate()
    {
        GameOverOn();
        ShowClearPanel();
    }

    public void PauseClick()
    {
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        GameManager.Instance.gameState = GameManager.State.Pause;
    }

    public void RestartClick()
    {
        GameManager.Instance.gameState = GameManager.State.Play;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        if (gameOverPanel.activeSelf)
            gameOverPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void ResumeClick()
    {
        GameManager.Instance.gameState = GameManager.State.Play;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OptionClick()
    {
        optionPanel.SetActive(true);
    }

    public void CancleClick()
    {
        optionPanel.SetActive(false);
    }

    public void GameOverOn()
    {
        if(GameManager.Instance.gameState == GameManager.State.Gameover)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void ShowClearPanel()
    {
        if (GameManager.Instance.gameState == GameManager.State.Clear)
        {
            clearPanel.SetActive(true);
            Time.timeScale = 0f;
        };
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }

}
