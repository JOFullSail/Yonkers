using UnityEngine;
using UnityEngine.UI;

public class SettingsData : MonoBehaviour
{
    public static SettingsData instance;

    [SerializeField] ButtonFunctions btnFunctions;

	[Header("Gameplay")]
    public float FOV = 70f;
    public float mouSens = 500f;
	public float brightness = 50f;
    public float MasterVol = 0.5f;
    public float MusicVol = 0.5f;
    public float SFXVol = 0.5f;
    public float CharacterVoicesVol = 50f;

    [Header("Developer Settings")]
    public int FOVMin = 30;
    public int FOVMax = 100;
    public bool FOVWholeNumbers = true;
    
    public int mouSensMin = 1;
    public int mouSensMax = 1000;
    public bool mouSensWholeNumbers = true;

	public int brightnessMin = 10;
	public int brightnessMax = 90;
	public int brightnessHalf = 50;
    public bool brightnessWholeNumbers = true;
     
    public float MasterVolMin = 0.0001f;
    public float MasterVolMax = 1f;
    public bool MasterVolWholeNumbers = false;

    public float MusicVolMin = 0.0001f;
    public float MusicVolMax = 1f;
    public bool MusicVolWholeNumbers = false;

    public float SFXVolMin = 0.0001f;
    public float SFXVolMax = 1f;
    public bool SFXVolWholeNumbers = false;

    public float CharacterVoicesVolMin = 0.0001f;
    public float CharacterVoicesVolMax = 1f;
    public bool CharacterVoicesVolWholeNumbers = false;


    // Initializes instance and sets all sliders to their correct position.
    void Awake()
    {
        instance = this;

        Slider slider;
        if (btnFunctions.FOVSliderParent != null)
        {
            slider = btnFunctions.FOVSliderParent.GetComponentInChildren<Slider>();
            if (slider)
            {
                float FOVOrig = FOV;
                if (FOVWholeNumbers)
                {

                    slider.minValue = (int)FOVMin;
                    slider.maxValue = (int)FOVMax;
                    slider.value = FOVOrig;
                }
                else
                {
                    slider.minValue = FOVMin;
                    slider.maxValue = FOVMax;
                    slider.value = FOVOrig;
                }

                FOV = FOVOrig;
            }

            slider = btnFunctions.mouSensSliderParent.GetComponentInChildren<Slider>();

            if (slider)
            {
                float mouSensOrig = mouSens;
                if (mouSensWholeNumbers)
                {
                    slider.minValue = (int)mouSensMin;
                    slider.maxValue = (int)mouSensMax;
                    slider.value = (int)mouSensOrig;
                }
                else
                {
                    slider.minValue = mouSensMin;
                    slider.maxValue = mouSensMax;
                    slider.value = mouSensOrig;
                }

                mouSens = mouSensOrig;
            }

            slider = btnFunctions.brightnessSliderParent.GetComponentInChildren<Slider>();

            if (slider)
            {
                float brightnessOrig = brightness;
                if (brightnessWholeNumbers)
                {
                    slider.minValue = (int)brightnessMin;
                    slider.maxValue = (int)brightnessMax;
                    slider.value = (int)brightnessOrig;
                }
                else
                {
                    slider.minValue = brightnessMin;
                    slider.maxValue = brightnessMax;
                    slider.value = brightnessOrig;
                }

                brightness = brightnessOrig;
            }


            slider = btnFunctions.MasterVolSliderParent.GetComponentInChildren<Slider>();

            if (slider)
            {
                float MasVolOrig = MasterVol;
                slider.minValue = MasterVolMin;
                slider.maxValue = MasterVolMax;
                slider.value = MasVolOrig;
                MasterVol = MasVolOrig;
            }

            slider = btnFunctions.MusicVolSliderParent.GetComponentInChildren<Slider>();

            if (slider)
            {
                float MusicVolOrig = MusicVol;

                    slider.minValue = MusicVolMin;
                    slider.maxValue = MusicVolMax;
                    slider.value = MusicVolOrig;
                    MusicVol = MusicVolOrig;
            }

            slider = btnFunctions.SFXVolSliderParent.GetComponentInChildren<Slider>();

            if (slider)
            {
                float SFXVolOrig = SFXVol;

                    slider.minValue = SFXVolMin;
                    slider.maxValue = SFXVolMax;
                    slider.value = SFXVolOrig;
                    SFXVol = SFXVolOrig;
            }

            slider = btnFunctions.CharacterVoicesSliderParent.GetComponentInChildren<Slider>();

            if (slider)
            {
                float CharacterVoicesVolOrig = CharacterVoicesVol;
                    slider.minValue = CharacterVoicesVolMin;
                    slider.maxValue = CharacterVoicesVolMax;
                    slider.value = CharacterVoicesVolOrig;
                    CharacterVoicesVol = CharacterVoicesVolOrig;
            }
        }

    }
    
}