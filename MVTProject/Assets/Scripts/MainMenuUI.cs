using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] GameObject optPanel;
    [SerializeField] Slider bgmSlider;
    [SerializeField] Slider seSlider;

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void ShowOptionPanel()
    {
        bgmSlider.value = PlayerPrefs.GetFloat("bgmVol", 1f);
        seSlider.value = PlayerPrefs.GetFloat("sfxVol", 1f);
        optPanel.SetActive(true);
    }

    public void HideOptionPanel()
    {
        optPanel.SetActive(false);
    }

}
