using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Sound
{
    public string soundName;
    public AudioClip clip;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] Sound[] bgmSounds;
    [SerializeField] Sound[] sfxSounds;

    [SerializeField] AudioSource bgmPlayer;
    [SerializeField] AudioSource[] sfxPlayer;

    // Start is called before the first frame update
    void Start()
    {
        bgmPlayer.volume = PlayerPrefs.GetFloat("bgmVol", 1f);
        for(int i = 0; i < sfxPlayer.Length; i++)
        {
            sfxPlayer[i].volume = PlayerPrefs.GetFloat("sfxVol", 1f);
        }

        instance = this;
        if (SceneManager.GetActiveScene().buildIndex == 0)
            PlayBGM(0);
        else
            PlayBGM(1);
    }

    public void PlayBGM(int idx)
    {
        bgmPlayer.clip = bgmSounds[idx].clip;
        bgmPlayer.Play();
    }

    public void PlaySE(string _soundName)
    {
        for(int i = 0; i < sfxSounds.Length; i++)
        {
            if(_soundName == sfxSounds[i].soundName)
            {
                for(int x = 0; x < sfxPlayer.Length; x++)
                {
                    if (!sfxPlayer[x].isPlaying)
                    {
                        sfxPlayer[x].clip = sfxSounds[i].clip;
                        sfxPlayer[x].Play();
                        return;
                    }
                }
                Debug.Log("효과음을 재생할 플레이어가 부족합니다.");
                return;
            }
        }
        Debug.Log("등록된 효과음이 없습니다");
    }


    public void ChangeBGMVol(float val)
    {
        PlayerPrefs.SetFloat("bgmVol", val);
        bgmPlayer.volume = val;
    }

    public void ChangeSEVol(float val)
    {
        for(int i = 0; i < sfxPlayer.Length; i++)
        {
            sfxPlayer[i].volume = val;
        }
        PlayerPrefs.SetFloat("sfxVol", val);
    }

}
