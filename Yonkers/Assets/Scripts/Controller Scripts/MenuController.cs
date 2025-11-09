using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject[] Selection;
    private int index = 0;
    private void Update()
    {
        if (Input.GetButton("arrow_Up"))
        {
           
        }
    }
    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(Selection[index]);
    }
    IEnumerator menuslectionDelay()
    {
        yield return new WaitForSeconds(0.5f);
    }
}
