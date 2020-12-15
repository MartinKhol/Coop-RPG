using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class EscapeMenuUI : MonoBehaviour
{
    public AudioMixer mainMixer;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Toggle autoSaveToggle;

    const string musicParam = "Music";
    const string sfxParam = "SFX";
    const string autoSaveKey = "autoSave";

    void Start()
    {
        if (PlayerPrefs.HasKey(musicParam))
        {
            var musicVol = PlayerPrefs.GetFloat(musicParam);
            mainMixer.SetFloat(musicParam, musicVol);
            musicSlider.value = musicVol;
        }
        if (PlayerPrefs.HasKey(sfxParam))
        {
            var musicVol = PlayerPrefs.GetFloat(sfxParam);
            mainMixer.SetFloat(sfxParam, musicVol);
            sfxSlider.value = musicVol;
        }
        if (PlayerPrefs.HasKey(autoSaveKey))
        {
            bool save = PlayerPrefs.GetInt(autoSaveKey) == 1;
            AutoSave(save);
            autoSaveToggle.isOn = save;
        }
    }

    public void SetMusicVolume(float value)
    {
        mainMixer.SetFloat(musicParam, value);
        PlayerPrefs.SetFloat(musicParam, value);
    }
    public void SetSfxVolume(float value)
    {
        mainMixer.SetFloat(sfxParam, value);
        PlayerPrefs.SetFloat(sfxParam, value);
    }

    public void AutoSave(bool set)
    {
        Settings.autoSave = set;
        PlayerPrefs.SetInt(autoSaveKey, set ? 1 : 0);
    }
}
