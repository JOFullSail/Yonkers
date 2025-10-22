using System.Collections;
using UnityEngine;

public class checkpoint : MonoBehaviour
{
    [SerializeField] GameObject MainFlagObject;
    Renderer flagrender1;
    Renderer flagrender2;
    Renderer flagrender3;

    Color colorOrig1;
    Color colorOrig2;
    Color colorOrig3;
    private void Start()
    {
        GameObject flag = MainFlagObject.transform.Find("Flag").gameObject;
        flagrender1 = flag.transform.Find("Flag Wrap").GetComponent<Renderer>();
        flagrender2 = flag.transform.Find("Flag1").GetComponent<Renderer>();
        flagrender3 = flag.transform.Find("Flag2").GetComponent<Renderer>();
        colorOrig1 = flagrender1.material.color;
        colorOrig2 = flagrender2.material.color;
        colorOrig3 = flagrender3.material.color;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;
        if (true)
        {
            GameManager.instance.playerSpawnOrig.transform.position = transform.position;
            StartCoroutine(feedback());
        }
    }

    IEnumerator feedback()
    {
        flagrender1.material.color = Color.red;
        flagrender2.material.color = Color.red;
        flagrender3.material.color = Color.red;
        //GameManager.instance.CheckPointPopup.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        //GameManager.instance.CheckPointPopup.SetActive(false);
        flagrender1.material.color = colorOrig1;
        flagrender2.material.color = colorOrig2;
        flagrender3.material.color = colorOrig3;
    }
}
