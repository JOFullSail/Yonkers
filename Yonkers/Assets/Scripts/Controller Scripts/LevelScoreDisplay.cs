using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelScoreDisplay : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, // Interfaces to implement if this class needs OnPointerEnter/Exit callbacks (for mouse hovering in menus)
    ISelectHandler, IDeselectHandler // Interfaces to implement if this class needs OnSelect/Deselect callbacks (for keyboard input in menus)
{
    [SerializeField] GameObject displayAspect;
    [SerializeField] GameObject OnTop;
    [SerializeField] GameObject RealParent;

    Vector3 OGposition;
    bool getOGposition = true;

    void ShowBox()
    {
        if (getOGposition)
        {
            OGposition = displayAspect.transform.localPosition;
            getOGposition = false;
        }

        displayAspect.transform.parent.SetParent(OnTop.transform, false);
        transform.parent.SetParent(OnTop.transform, false);

        if (name.Contains("1") || name.Contains("2") || name.Contains("3"))
            displayAspect.transform.localPosition = new Vector3(245, -5, 0);
        else
            displayAspect.transform.localPosition = new Vector3(-245, -5, 0);
    }

    void HideBox()
    {
        displayAspect.transform.parent.SetParent(RealParent.transform, false);
        transform.parent.SetParent(RealParent.transform, false);
        displayAspect.transform.localPosition = OGposition;

        getOGposition = true;
    }

    // MOUSE HOVER
    public void OnPointerEnter(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(null); // Clear previous selection

        ShowBox();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideBox();
    }

    // KEYBOARD
    public void OnSelect(BaseEventData eventData)
    {
        ShowBox();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        HideBox();
    }
}
