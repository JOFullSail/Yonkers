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
        GameManager.instance.statetoLevelSelect();

    }
    //to settings menu
    public void toSettings()
    {
        GameManager.instance.statetoSettings();

    }
    //Open Gameplay section in settings menu
    public void SettingsGameplay()
    {
        GameManager.instance.openGameplaySubmenu();

    }
    //Open Sound section in settings menu
    public void SettingsSound()
    {
        GameManager.instance.openAudioSubmenu();

    }
    //back to main menu
    public void toMainmenu() //exit during a level uses this! also "back" while in main menu scene!
    {
        GameManager.instance.LoadNextLevel("Main Menu Scene");
    }
    public void Backfrom() //exit during a level uses this! also "back" while in main menu scene!
    {
        if(GameManager.instance.currScene.name != "Main Menu Scene")
        {
            
            GameManager.instance.backtoPausemenu();
        }
        else
        {
            GameManager.instance.backtoMainmenu();
        }
    }
    //to level select menu
    public void backtoLevelselect()
    {
        toMainmenu();
        GameManager.instance.statetoLevelSelect();

    }
    //Continue goes to level select but uses player's current save data
    public void ContinuetoLevelSelect()
    {
        GameManager.instance.statetoLevelSelect();
    }
    //to credits section not added yet
    //public void toCredits()
    //{

    //}
    public void gotoMainmenu()
    {

        GameManager.instance.stateUnpause();
        GameManager.instance.settoMainmenu();
        GameManager.instance.LoadNextLevel("Main Menu Scene");
        
    }
    public void gotoLevel1()//head to level 1
    {
        GameManager.instance.stateUnpause();
        GameManager.instance.LoadNextLevel("Level 1 - Jorg Plains");
        
    }
    public void gotoLevel2()//head to level 2
    {
        GameManager.instance.stateUnpause();
        GameManager.instance.LoadNextLevel("Level 2- Gold's Springway");
    }
    public void gotoLevel3()//head to level 3
    {
        //GameManager.instance.LoadNextLevel(levelName);
    }
    public void gotoLevel4()//head to level 4
    {
        //GameManager.instance.LoadNextLevel(levelName);
    }
    public void gotoLevel5()//head to level 5
    {
        //GameManager.instance.LoadNextLevel(levelName);
    }
    public void gotoShowcase()//head to Showcase
    {
        //GameManager.instance.LoadNextLevel(levelName);
    }
}
