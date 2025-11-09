using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class ButtonFunctions : MonoBehaviour
{
    public Image FOVSliderParent;
    public Image mouSensSliderParent;
    public Image brightnessSliderParent;

    List<GameObject> menuStack;

    Camera cam;
    void Start()
    {
        if(GameManager.instance.CameraScript != null)
            cam = GameManager.instance.CameraScript.GetComponent<Camera>();

        if(SettingsData.instance != null)
        {
            if (cam) cam.fieldOfView = SettingsData.instance.FOV;
            if(FOVSliderParent != null && mouSensSliderParent != null && brightnessSliderParent != null)
            {
                FOVSliderParent.fillAmount = normalize(SettingsData.instance.FOV, SettingsData.instance.FOVMin, SettingsData.instance.FOVMax);
                mouSensSliderParent.fillAmount = normalize(SettingsData.instance.mouSens, SettingsData.instance.mouSensMin, SettingsData.instance.mouSensMax);
                brightnessSliderParent.fillAmount = normalize(SettingsData.instance.brightness, SettingsData.instance.brightnessMin, SettingsData.instance.brightnessMax);
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
        GameManager.instance.playerScript.respawnPlayer(true, true);
    }
    public void restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        GameManager.instance.stateUnpause();
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
        if (cam && cam.fieldOfView > 0) cam.fieldOfView = sliderValue;

        // Updating UI Bars
        FOVSliderParent.fillAmount = normalize(sliderValue, slider.minValue, slider.maxValue);
    }

    public void mouSens(Slider slider)
    {
        float sliderValue;
        if (SettingsData.instance.mouSensWholeNumbers) sliderValue = (int)slider.value;
        else sliderValue = slider.value;

        // Applying Values
        SettingsData.instance.mouSens = sliderValue;
        if (GameManager.instance.CameraScript) GameManager.instance.CameraScript.mouSens = sliderValue;

        // Updating UI Bars
        mouSensSliderParent.fillAmount = normalize(sliderValue, slider.minValue, slider.maxValue);
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
        brightnessSliderParent.fillAmount = normalize(sliderValue, slider.minValue, slider.maxValue);
    }
    //new game goes to level select and refreshes player data
    public void NewGametoLevelSelect()
    {
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

        GameManager.instance.LoadNextLevel("Main Menu Scene");
        GameManager.instance.clearActive();
        GameManager.instance.menuTolevel();
        GameManager.instance.disablePlayerUI();

    }
    //Continue goes to level select but uses player's current save data
    public void ContinuetoLevelSelect()
    {
        GameManager.instance.menuTolevel();
    }
    //to credits section not added yet
    //public void toCredits()
    //{

    //}
    public void gotoMainmenu()
    {
        
        GameManager.instance.statePause();
        GameManager.instance.LoadNextLevel("Main Menu Scene");
        GameManager.OpenMainMenu = true;

    }
    public void gotoLevel1()//head to level 1
    {
        GameManager.instance.stateUnpause();
        GameManager.instance.enablePlayerUI();
        GameManager.instance.LoadNextLevel("Level 1 - Jorg Plains");
        
    }
    public void gotoLevel2()//head to level 2
    {
        GameManager.instance.stateUnpause();
        GameManager.instance.enablePlayerUI();
        GameManager.instance.LoadNextLevel("Level 2- Gold's Springway");
    }
    public void gotoLevel3()//head to level 3
    {
        //GameManager.instance.stateUnpause();
        //GameManager.instance.enablePlayerUI();
        //GameManager.instance.LoadNextLevel(levelName);
    } 
    public void gotoLevel4()//head to level 4
    {
        //GameManager.instance.stateUnpause();
        //GameManager.instance.enablePlayerUI();
        //GameManager.instance.LoadNextLevel(levelName);
    }
    public void gotoLevel5()//head to level 5
    {
        //GameManager.instance.stateUnpause();
        //GameManager.instance.enablePlayerUI();
        //GameManager.instance.LoadNextLevel(levelName);
    }
    public void gotoShowcase()//head to Showcase
    {
        //GameManager.instance.stateUnpause();
        //GameManager.instance.enablePlayerUI();
        //GameManager.instance.LoadNextLevel(levelName);
    }
}
