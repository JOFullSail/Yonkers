using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class LevelScoreDisplay : MonoBehaviour
{
    [SerializeField] GameObject displayAspect;
    [SerializeField] GameObject OnTop; //used for buttond 4 and 5 to display the right one;
    [SerializeField] GameObject RealParent;// used to reassign parnet back to rght right one for level 4 and 5;
    Vector3 OGpostion;
    bool isDisplaying = false;
    bool getOGpostion = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Update()
    {
        if ((EventSystem.current.IsPointerOverGameObject() == true && EventSystem.current.currentSelectedGameObject == gameObject && isDisplaying == false))
        {
            if (getOGpostion == true)
            {
                OGpostion = displayAspect.transform.localPosition;
                getOGpostion = false;
            }

            if (gameObject.name == "Level 1 Button")
            {
                displayAspect.transform.parent.SetParent(OnTop.transform, false);
                gameObject.transform.parent.SetParent(OnTop.transform, false);
                displayAspect.transform.localPosition = new Vector3(245, -5, 0);
            }
            else if (gameObject.name == "Level 2 Button")
            {
                displayAspect.transform.parent.SetParent(OnTop.transform, false);
                gameObject.transform.parent.SetParent(OnTop.transform, false);
                displayAspect.transform.localPosition = new Vector3(245, -5, 0);
            }
            else if (gameObject.name == "Level 3 Button")
            {
                displayAspect.transform.parent.SetParent(OnTop.transform, false);
                gameObject.transform.parent.SetParent(OnTop.transform, false);
                displayAspect.transform.localPosition = new Vector3(245, -5, 0);
            }
            else if (gameObject.name == "Level 4 Button")
            {
                displayAspect.transform.parent.SetParent(OnTop.transform, false);
                gameObject.transform.parent.SetParent(OnTop.transform, false);
                displayAspect.transform.localPosition = new Vector3(-245, -5, 0);
            }
            else if (gameObject.name == "Level 5 Button")
            {
                displayAspect.transform.parent.SetParent(OnTop.transform, false);
                gameObject.transform.parent.SetParent(OnTop.transform, false);
                displayAspect.transform.localPosition = new Vector3(-245, -5, 0);
            }
            else if (gameObject.name == "Level 6 Button")
            {
                displayAspect.transform.parent.SetParent(OnTop.transform, false);
                gameObject.transform.parent.SetParent(OnTop.transform, false);
                displayAspect.transform.localPosition = new Vector3(-245, -5, 0); 
            }
            isDisplaying = true;
        }
        else if ((EventSystem.current.currentSelectedGameObject != gameObject && isDisplaying == true))
        {
            displayAspect.transform.parent.SetParent(RealParent.transform, false);
            gameObject.transform.parent.SetParent(RealParent.transform, false);
            displayAspect.transform.localPosition = OGpostion;
            isDisplaying = false;
            getOGpostion = true;
        }
    }
}
