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
        }

    }
    
}