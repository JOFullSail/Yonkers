using UnityEngine;

public class Test_LevelTransition : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            GameManager.instance.LoadNextLevel("Jorg Scene");
    }
}
