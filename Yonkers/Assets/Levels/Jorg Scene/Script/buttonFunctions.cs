using UnityEngine;
using UnityEngine.SceneManagement;

public class buttonFunctions : MonoBehaviour
{
    public void OnResume()
    {
        gameManager.instance.stateUnpause();
    }

    public void OnRestart()
    {
        // Better to reset the variables (position, number of enemies, player position, etc)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        gameManager.instance.stateUnpause();
    }

    public void OnQuit()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;

    #else
        Application.Quit();

    #endif
    }
}
