using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] bool playButtonSwitchesScenes = false;
    [SerializeField] string sceneToLoad = "Ethans Scene";

    public void PlayGame()
    {
        if (playButtonSwitchesScenes)
            SceneManager.LoadSceneAsync(sceneToLoad);
        else
        {
            // do something else
        }
    }
}
