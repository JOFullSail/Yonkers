using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonFunctions : MonoBehaviour
{
    public void resume()
    {
        GameManager.instance.stateUnpause();
    }

    public void respawn()
    {
        if (LevelManager.instance.pointsLostOnRespawn > 0)
            LevelManager.instance.CurrentScore -= LevelManager.instance.pointsLostOnRespawn;

        GameManager.instance.stateUnpause();
        GameManager.instance.playerScript.respawnPlayer(true, true);
    }
    public void restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        GameManager.instance.stateUnpause();
    }
    public void play()
    {
        GameManager.instance.stateUnpause();
    }
    public void quit()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit();
    #endif 
    }

}
