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
        cam = GameManager.instance.CameraScript.GetComponent<Camera>();
        if (cam) cam.fieldOfView = SettingsData.instance.FOV;
        FOVSliderParent.fillAmount = normalize(SettingsData.instance.FOV, SettingsData.instance.FOVMin,  SettingsData.instance.FOVMax);
        mouSensSliderParent.fillAmount = normalize(SettingsData.instance.mouSens, SettingsData.instance.mouSensMin,  SettingsData.instance.mouSensMax);
        brightnessSliderParent.fillAmount = normalize(SettingsData.instance.brightness, SettingsData.instance.brightnessMin, SettingsData.instance.brightnessMax);
    }
    
    public static float normalize(float min, float max, float value)
    {
        return (value - min) / (max - min);
    }

    public static float deNormalize(float min, float max, float value)
    {
        return min + value * (max - min);
    }

    // Main Menu
    public void play()
    {
        GameManager.instance.stateUnpause();
    }
    
    // Pause Menu
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
    
    public void quit()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif 
    }
    
    // Settings Menu
	
    // - Gameplay
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
        else
        {
            Color clr = GameManager.instance.playerBrightnessOverlay.color;
            clr.a = 0;
            GameManager.instance.playerBrightnessOverlay.color = clr;
        }
        
        // Updating UI Bars
        brightnessSliderParent.fillAmount = normalize(sliderValue, slider.minValue, slider.maxValue);
    }
}