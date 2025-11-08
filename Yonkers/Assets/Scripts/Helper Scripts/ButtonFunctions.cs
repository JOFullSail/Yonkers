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
    public void QuitGame() //Exit on main menu uses this!!!
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
    //new game goes to level select and refreshes player data
    public void NewGametoLevelSelect() 
    {
        //GameManager.instance.statet

    }
    //to settings menu
    public void toSettings()
    {
        GameManager.instance.statetoSettings();

    }
    //Open Gameplay section in settings menu
    public void SettingsGameplay()
    {


    }
    //Open Sound section in settings menu
    public void SettingsSound()
    {


    }
    //back to main menu
    public void toMainmenu() //exit during a level uses this! also "back" while in main menu scene!
    {
        GameManager.instance.LoadNextLevel("Main Menu Scene");
    }
    //to level select menu
    public void toLevelselect()
    {


    }
    //Continue does to level select but uses player's current save data
    public void ContinuetoLevelSelect()
    {


    }
    //to credits section
    public void toCredits()
    {

    
    }
}
