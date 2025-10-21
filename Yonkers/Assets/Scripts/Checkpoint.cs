using System.Collections;
using UnityEngine;

public class checkpoint : MonoBehaviour
{
    [SerializeField] Renderer model;

    Color colorOrig;
    private void Start()
    {
        colorOrig = model.material.color;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (true)
        {
            //GameManager.instance.playerSpawnPos.transform.position = transform.position;
            //StartCoroutine(feedback());
        }
    }

    //IEnumerator feedback()
    //{
    //    model.material.color = Color.red;
    //    GameManager.instance.CheckPointPopup.SetActive(true);
    //    yield return new WaitForSeconds(0.5f);
    //    GameManager.instance.CheckPointPopup.SetActive(false);
    //    model.material.color = colorOrig;
    //}
}
