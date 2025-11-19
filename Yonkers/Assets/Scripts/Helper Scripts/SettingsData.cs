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
    public float MasterVol = 50f;
    public float MusicVol = 50f;
    public float SFXVol = 50f;
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

    public int MasterVolMin = 0;
    public int MasterVolMax = 100;
    public bool MasterVolWholeNumbers = true;

    public int MusicVolMin = 0;
    public int MusicVolMax = 100;
    public bool MusicVolWholeNumbers = true;

    public int SFXVolMin = 0;
    public int SFXVolMax = 100;
    public bool SFXVolWholeNumbers = true;

    public int CharacterVoicesVolMin = 0;
    public int CharacterVoicesVolMax = 100;
    public bool CharacterVoicesVolWholeNumbers = true;


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
                if (MasterVolWholeNumbers)
                {
                    slider.minValue = (int)MasterVolMin;
                    slider.maxValue = (int)MasterVolMax;
                    slider.value = (int)MasVolOrig;
                }
                else
                {
                    slider.minValue = MasterVolMin;
                    slider.maxValue = MasterVolMax;
                    slider.value = MasVolOrig;
                }

                MasterVol = MasVolOrig;
            }

            slider = btnFunctions.MusicVolSliderParent.GetComponentInChildren<Slider>();

            if (slider)
            {
                float MusicVolOrig = MusicVol;
                if (MusicVolWholeNumbers)
                {
                    slider.minValue = (int)MusicVolMin;
                    slider.maxValue = (int)MusicVolMax;
                    slider.value = (int)MusicVolOrig;
                }
                else
                {
                    slider.minValue = MusicVolMin;
                    slider.maxValue = MusicVolMax;
                    slider.value = MusicVolOrig;
                }

                MusicVol = MusicVolOrig;
            }

            slider = btnFunctions.SFXVolSliderParent.GetComponentInChildren<Slider>();

            if (slider)
            {
                float SFXVolOrig = SFXVol;
                if (SFXVolWholeNumbers)
                {
                    slider.minValue = (int)SFXVolMin;
                    slider.maxValue = (int)SFXVolMax;
                    slider.value = (int)SFXVolOrig;
                }
                else
                {
                    slider.minValue = SFXVolMin;
                    slider.maxValue = SFXVolMax;
                    slider.value = SFXVolOrig;
                }

                SFXVol = SFXVolOrig;
            }

            slider = btnFunctions.CharacterVoicesSliderParent.GetComponentInChildren<Slider>();

            if (slider)
            {
                float CharacterVoicesVolOrig = CharacterVoicesVol;
                if (CharacterVoicesVolWholeNumbers)
                {
                    slider.minValue = (int)CharacterVoicesVolMin;
                    slider.maxValue = (int)CharacterVoicesVolMax;
                    slider.value = (int)CharacterVoicesVolOrig;
                }
                else
                {
                    slider.minValue = CharacterVoicesVolMin;
                    slider.maxValue = CharacterVoicesVolMax;
                    slider.value = CharacterVoicesVolOrig;
                }

                CharacterVoicesVol = CharacterVoicesVolOrig;
            }
        }

    }
    
}