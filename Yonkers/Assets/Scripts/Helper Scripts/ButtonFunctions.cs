using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonFunctions : MonoBehaviour
{
    public Image FOVSliderParent;
    public Image mouSensSliderParent;
    public Image brightnessSliderParent;
    public Image MasterVolSliderParent;
    public Image MusicVolSliderParent;
    public Image SFXVolSliderParent;
    public Image CharacterVoicesSliderParent;

    public float MasterVolorig;
    public float MusicVolorig;
    public float SFXVolorig;
    public float CVVolorig;
    void Start()
    {
        if (SettingsData.instance != null)
        {
            if (GameManager.instance.cam != null)
            {
                GameManager.instance.cam.fieldOfView = SettingsData.instance.FOV;
            }
            if (FOVSliderParent != null && mouSensSliderParent != null && brightnessSliderParent != null)
            {
                FOVSliderParent.fillAmount = normalize(SettingsData.instance.FOVMin, SettingsData.instance.FOVMax, SettingsData.instance.FOV);
                mouSensSliderParent.fillAmount = normalize(SettingsData.instance.mouSensMin, SettingsData.instance.mouSensMax, SettingsData.instance.mouSens);
                brightnessSliderParent.fillAmount = normalize(SettingsData.instance.brightnessMin, SettingsData.instance.brightnessMax, SettingsData.instance.brightness);
                MasterVolSliderParent.fillAmount = normalize(SettingsData.instance.MasterVolMin, SettingsData.instance.MasterVolMax, SettingsData.instance.MasterVol);
                MusicVolSliderParent.fillAmount = normalize(SettingsData.instance.MusicVolMin, SettingsData.instance.MusicVolMax, SettingsData.instance.MusicVol);
                SFXVolSliderParent.fillAmount = normalize(SettingsData.instance.SFXVolMin, SettingsData.instance.SFXVolMax, SettingsData.instance.SFXVol);
                CharacterVoicesSliderParent.fillAmount = normalize(SettingsData.instance.CharacterVoicesVolMin, SettingsData.instance.CharacterVoicesVolMax, SettingsData.instance.CharacterVoicesVol);
            }
        }
    }
    private void Update()
    {
        if (GameManager.instance.playerScript != null)
        {
            float FOVOrig = SettingsData.instance.FOV;
            if (GameManager.instance.playerScript.IsDashing == true)
            {
                GameManager.instance.cam.fieldOfView += 20f * Time.deltaTime;
            }
            else
            {
                if (GameManager.instance.cam.fieldOfView > FOVOrig)
                {
                    if (GameManager.instance.cam.fieldOfView <= FOVOrig)
                    {
                        GameManager.instance.cam.fieldOfView = FOVOrig;
                    }
                    else
                    {
                        GameManager.instance.cam.fieldOfView -= 80f * Time.deltaTime;
                    }
                }
            }
        }
    }

    public static float normalize(float min, float max, float value)
    {
        return (value - min) / (max - min);
    }

    public static float deNormalize(float min, float max, float value)
    {
        return min + value * (max - min);
    }
    public void resume()
    {
        GameManager.instance.stateUnpause();
    }

    public void respawn()
    {
        if (LevelManager.instance.pointsLostOnRespawn > 0)
            LevelManager.instance.CurrentScore -= LevelManager.instance.pointsLostOnRespawn;

        GameManager.instance.stateUnpause();
        GameManager.instance.RespawnFromCheckpoint(true);
    }
    public void restart()
    {
        GameManager.instance.ResetSave();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void play()
    {
        GameManager.instance.stateUnpause();
    }
    public void QuitGame() //Exit on main menu uses this!!!
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public void fov(Slider slider)
    {
        // Whole Number Settings
        float sliderValue;
        if (SettingsData.instance.FOVWholeNumbers) sliderValue = (int)slider.value;
        else sliderValue = slider.value;

        // Applying Values
        SettingsData.instance.FOV = sliderValue;
        GameManager.instance.FOVcurrentNumber.text = sliderValue.ToString();
        if (GameManager.instance.cam.fieldOfView > 0) GameManager.instance.cam.fieldOfView = sliderValue;

        // Updating UI Bars
        FOVSliderParent.fillAmount = normalize(slider.minValue, slider.maxValue, sliderValue);
    }
    public void masterVol(Slider slider)
    {
        // Whole Number Settings
        float sliderValue;
        sliderValue = slider.value;
        // Applying Values
        GameManager.instance.audMix.SetFloat("MasterVolume", Mathf.Log10(sliderValue) * 20);
        SettingsData.instance.MasterVol = sliderValue;
        GameManager.instance.MastervolcurrentNumber.text = (sliderValue * 100).ToString("F0");
        // Updating UI Bars
        MasterVolSliderParent.fillAmount = normalize(slider.minValue, slider.maxValue, sliderValue);
    }
    public void musicVol(Slider slider)
    {
        // Whole Number Settings
        float sliderValue;
        sliderValue = slider.value;
        // Applying Values
        GameManager.instance.audMix.SetFloat("MusicVolume", Mathf.Log10(sliderValue) * 20);
        SettingsData.instance.MusicVol = sliderValue;
        GameManager.instance.MusicvolcurrentNumber.text = (sliderValue * 100).ToString("F0");
        // Updating UI Bars
        MusicVolSliderParent.fillAmount = normalize(slider.minValue, slider.maxValue, sliderValue);
    }
    public void SFXVol(Slider slider)
    {
        // Whole Number Settings
        float sliderValue;
        sliderValue = slider.value;
        // Applying Values
        SettingsData.instance.SFXVol = sliderValue;
        GameManager.instance.audMix.SetFloat("SFXVolume", Mathf.Log10(sliderValue) * 20);
        GameManager.instance.SFXvolcurrentNumber.text = (sliderValue * 100).ToString("F0");
        // Updating UI Bars
        SFXVolSliderParent.fillAmount = normalize(slider.minValue, slider.maxValue, sliderValue);
    }
    public void charactervoicesVol(Slider slider)
    {
        // Whole Number Settings
        float sliderValue;
        if (SettingsData.instance.CharacterVoicesVolWholeNumbers) sliderValue = (int)slider.value;
        else sliderValue = slider.value;
        // Applying Values
        SettingsData.instance.CharacterVoicesVol = sliderValue;
        GameManager.instance.audMix.SetFloat("VoiceVolume", Mathf.Log10(sliderValue) * 20);
        GameManager.instance.CVoicescurrentNumber.text = (sliderValue * 100).ToString("F0");
        // Updating UI Bars
        CharacterVoicesSliderParent.fillAmount = normalize(slider.minValue, slider.maxValue, sliderValue);
    }

    public void mouSens(Slider slider)
    {
        float sliderValue;
        if (SettingsData.instance.mouSensWholeNumbers) sliderValue = (int)slider.value;
        else sliderValue = slider.value;

        // Applying Values
        SettingsData.instance.mouSens = sliderValue;
        GameManager.instance.MouseSenscurrentNumber.text = sliderValue.ToString();
        if (GameManager.instance.CameraScript) GameManager.instance.CameraScript.mouSens = sliderValue;

        // Updating UI Bars
        mouSensSliderParent.fillAmount = normalize(slider.minValue, slider.maxValue, sliderValue);
    }

    public void brightness(Slider slider)
    {
        float sliderValue;
        if (SettingsData.instance.brightnessWholeNumbers) sliderValue = (int)slider.value;
        else sliderValue = slider.value;

        // Applying Values
        if (sliderValue > 50f)
        {
            float alphaValue;
            GameManager.instance.playerBrightnessOverlay.color = Color.white;
            alphaValue = normalize(SettingsData.instance.brightnessHalf, 100f, sliderValue);
            Color clr = GameManager.instance.playerBrightnessOverlay.color;
            clr.a = alphaValue;
            GameManager.instance.playerBrightnessOverlay.color = clr;
        }
        else if (sliderValue < 50f)
        {
            float alphaValue;
            GameManager.instance.playerBrightnessOverlay.color = Color.black;
            alphaValue = normalize(SettingsData.instance.brightnessHalf, 0f, sliderValue);
            Color clr = GameManager.instance.playerBrightnessOverlay.color;
            clr.a = alphaValue;
            GameManager.instance.playerBrightnessOverlay.color = clr;
        }
        GameManager.instance.BrightnesscurrentNumber.text = sliderValue.ToString();
        brightnessSliderParent.fillAmount = normalize(slider.minValue, slider.maxValue, sliderValue);
    }

    //new game goes to level select and refreshes player data
    public void NewGametoLevelSelect()
    {
        GameManager.instance.ResetSave();
        GameManager.instance.ResetProgression();
        GameManager.instance.ResetInventory();
        GameManager.instance.levelLocks();
        GameManager.instance.statetoLevelSelect();

    }
    //to settings menu
    public void toSettings()
    {
        GameManager.instance.statetoSettings();

    }
    //Open Gameplay section in settings menu
    public void SettingsGameplay()
    {
        GameManager.instance.openGameplaySubmenu();

    }
    //Open Sound section in settings menu
    public void SettingsSound()
    {
        GameManager.instance.openAudioSubmenu();

    }
    //back to main menu
    public void toMainmenu() //exit during a level uses this!
    {
        GameManager.instance.LoadNextLevel("Main Menu Scene");
        GameManager.instance.clearActive();
        GameManager.instance.LoadProgression();
        GameManager.instance.disablePlayerUI();
        GameManager.instance.backtoMainmenu();
    }
    public void toCredits() //credits in main menu uses this!
    {
        GameManager.instance.menuToCredits();
    }
    public void Backfrom() //back from settings uses this! also "back" while in main menu scene!
    {

        if (SceneManager.GetActiveScene().name == "Main Menu Scene" || SceneManager.GetActiveScene().name == "Main Menu Scene First Open")
        {
            GameManager.instance.backtoMainmenu();

        }
        else
        {
            GameManager.instance.backtoPausemenu();
        }
    }
    //to level select menu
    public void backtoLevelselect()
    {

        GameManager.instance.LoadNextLevel("Main Menu Scene", false);
        GameManager.instance.clearActive();
        GameManager.instance.levelLocks();
        GameManager.instance.menuTolevel();
        GameManager.instance.disablePlayerUI();

    }
    //Continue goes to level select but uses player's current save data
    public void ContinuetoLevelSelect()
    {
        GameManager.instance.levelLocks();
        GameManager.instance.menuTolevel();
    }
    public void Nextlevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        string scenePath = SceneUtility.GetScenePathByBuildIndex(nextSceneIndex);

        string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

        GameManager.instance.LoadNextLevel(sceneName);
    }
    public void gotoMainmenu()
    {

        GameManager.instance.statePause();
        GameManager.instance.LoadNextLevel("Main Menu Scene", false);
        GameManager.OpenMainMenu = true;

    }
    public void gotoLevel1()//head to level 1
    {
        GameManager.instance.stateUnpause();
        GameManager.instance.enablePlayerUI();
        GameManager.instance.LoadNextLevel("Level 1 - Jorg Plains", false);
    }
    public void gotoLevel2()//head to level 2
    {
        GameManager.instance.stateUnpause();
        GameManager.instance.enablePlayerUI();
        GameManager.instance.LoadNextLevel("Level 2 - Yonk Hill", false);
    }
    public void gotoLevel3()//head to level 3
    {
        GameManager.instance.stateUnpause();
        GameManager.instance.enablePlayerUI();
        GameManager.instance.LoadNextLevel("Level 3 - Yonk Factory", false);
    }
    public void gotoLevel4()//head to level 4
    {
        GameManager.instance.stateUnpause();
        GameManager.instance.enablePlayerUI();
        GameManager.instance.LoadNextLevel("Level 4 - The Wall", false);
    }
    public void gotoLevel5()//head to level 5
    {
        GameManager.instance.stateUnpause();
        GameManager.instance.enablePlayerUI();
        GameManager.instance.LoadNextLevel("Level 5 - THE FINAL YONK", false);
    }
}
