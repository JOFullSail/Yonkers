using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        
        if(SceneManager.GetActiveScene().name == "Level 1 - Jorg Plains")//make more for later levels
        {
            GameManager.instance.Level2lock = false;
            GameManager.instance.SaveGame();
        }
        if (other.CompareTag("Player"))
            GameManager.instance.stateWin();
    }
}
