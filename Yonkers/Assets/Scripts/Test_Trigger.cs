using UnityEditor;
using UnityEngine;

public class Test_Trigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            EventController.RaiseGameComplete();
    }
}
