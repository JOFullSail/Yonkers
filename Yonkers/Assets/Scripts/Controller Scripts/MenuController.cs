using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] GameObject FirstSelection;

    private bool mouseInControl = true;

    private void OnEnable()
    {
        // Default to keyboard controls
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(FirstSelection);

        mouseInControl = false;
    }

    private void Update()
    {
        // Give control to mouse
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            if (!mouseInControl)
            {
                mouseInControl = true;
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        // Give control to keyboard
        if (Input.GetKeyDown(KeyCode.UpArrow) ||
            Input.GetKeyDown(KeyCode.DownArrow) ||
            Input.GetKeyDown(KeyCode.LeftArrow) ||
            Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.Tab))
        {
            if (mouseInControl)
            {
                mouseInControl = false;

                if (EventSystem.current.currentSelectedGameObject == null)
                    EventSystem.current.SetSelectedGameObject(FirstSelection);
                else
                    EventSystem.current.SetSelectedGameObject(
                        EventSystem.current.currentSelectedGameObject
                    );
            }
        }
    }
}

