using System;
using System.Text;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject submenuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuDead;
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject menuSettings;
    [SerializeField] GameObject menuLevelSelect;
    [SerializeField] GameObject CreditsScreen; 
    [SerializeField] GameObject submenuGameplaySettings;
    [SerializeField] GameObject submenuAudioSettings;
    [SerializeField] GameObject submenuLockedlevel2;
    [SerializeField] GameObject submenuLockedlevel3;
    [SerializeField] GameObject submenuLockedlevel4;
    [SerializeField] GameObject submenuLockedlevel5;
    [SerializeField] GameObject submenuUnlockedlevel2button;
    [SerializeField] GameObject submenuUnlockedlevel2stats;
    [SerializeField] GameObject submenuUnlockedlevel3button;
    [SerializeField] GameObject submenuUnlockedlevel3stats;
    [SerializeField] GameObject submenuUnlockedlevel4button;
    [SerializeField] GameObject submenuUnlockedlevel4stats;
    [SerializeField] GameObject submenuUnlockedlevel5button;
    [SerializeField] GameObject submenuUnlockedlevel5stats;
    [SerializeField] GameObject PlayerHPDisplay;
    [SerializeField] GameObject PlayerAmmoDisplay;
    [SerializeField] GameObject PlayerReticleDisplay; //add more displays if you add more to player UI! ALSO ADD IT TO GETUI() OR IT WON'T BE FOUND!!!!

    [SerializeField] bool enableMainMenu = false;

    public GameObject playerSpawn;

    public GameObject player;
    public PlayerController playerScript;
    public GameObject goalObject;
    public GameObject MainCamera;
    public CameraController CameraScript;
    public GameObject blindScreen;
    public GameObject hypnoScreen;
    public GameObject webScreen;

    public Image playerHPBar;
    public TMP_Text playerHPLabel;
    public GameObject checkpointLabel;
    public Image playerDamageScreen;
    public Image playerHealScreen;
    public TMP_Text ammoCurrent, ammoMax;

    public bool isPaused;
    private bool isReloadingScene = false;
    private bool needUIReload = false;

    float timeScaleOrig;
    public GameObject playerSpawnOrig;

    int gameGoalCount;

    public Scene currScene;

    [Header("Gun Database")]
    public GunDatabase gunDatabase;

    private const string SaveKey = "PlayerSaveData";

    [Serializable]
    public class SaveData
    {
        public int HP;

        public int selectedGun;
        public string currentScene;
        public string lastCheckpointName;
        public bool Level2_lock;
        public bool Level3_lock;
        public bool Level4_lock;
        public bool Level5_lock;
        public List<GunStatsData> guns = new List<GunStatsData>();
    }

    [Serializable]
    public class GunStatsData
    {
        public string gunName;
        public int ammoCurrent;
        public int ammoReserves;
    }

    [Serializable]
    public class PlayerState
    {
        public int HP;
        public string lastScene;
        public List<GunStatsData> guns = new List<GunStatsData>();
    }

    public static bool OpenLevelSelect;
    public static bool OpenMainMenu;
    public bool Level2lock = true;
    public bool Level3lock = true;
    public bool Level4lock = true;
    public bool Level5lock = true;

    // This object lives in memory between scene transitions
    public PlayerState persistentPlayerState = new PlayerState();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            needUIReload = true;
            DontDestroyOnLoad(gameObject);
            gameObject.name = "Game ManagaerA";
            currScene = SceneManager.GetActiveScene();
            instance.currScene = SceneManager.GetActiveScene();
            if (currScene.name != "Main Menu Scene First Open")
            {
                player = GameObject.FindWithTag("Player");
                playerScript = player.GetComponent<PlayerController>();
                playerSpawn = GameObject.FindWithTag("PlayerSpawn");
                playerSpawnOrig = playerSpawn;
                goalObject = GameObject.FindWithTag("Goal");
            }
            MainCamera = GameObject.FindWithTag("MainCamera");
            CameraScript = MainCamera.GetComponent<CameraController>();
            timeScaleOrig = Time.timeScale;
            instance.GetUI();
            menuActive = mainMenu;
            menuActive.SetActive(true);
        }
        else
        {
            currScene = SceneManager.GetActiveScene();
            instance.currScene = SceneManager.GetActiveScene();
            if (menuPause == null && menuDead == null && menuLevelSelect == null)
            {
                instance.GetUI();
            }
            MainCamera = GameObject.FindWithTag("MainCamera");
            if (currScene.name != "Main Menu Scene")
            {
                player = GameObject.FindWithTag("Player");
                playerScript = player.GetComponent<PlayerController>();
                playerSpawn = GameObject.FindWithTag("PlayerSpawn");
                playerSpawnOrig = playerSpawn;
                goalObject = GameObject.FindWithTag("Goal");
                CameraScript = MainCamera.GetComponent<CameraController>();
            }
            timeScaleOrig = Time.timeScale;
        }

//#if UNITY_EDITOR
//        if (currScene.name != "Main Menu Scene First Open" || currScene.name != "Main Menu Scene") 
//        {
//            if (playerScript.DebugSpawnAtCamera)
//            {
//                if (playerSpawn != null)
//                {
//                    Transform cameraTransform = SceneView.lastActiveSceneView.camera.transform;
//                    playerSpawn.transform.position = cameraTransform.position;
//                    playerSpawn.transform.rotation = new Quaternion(0, cameraTransform.rotation.y, 0, 1);
//                    playerSpawnOrig = playerSpawn;
//                }
//                else player.transform.position = SceneView.lastActiveSceneView.camera.transform.position;
//            }
//        }
//#endif
    }

    // Update is called once per frame
    void Update()
    {


        if (Input.GetButtonDown("Cancel") && currScene.name != "Main Menu Scene")
        {
            if (menuActive == null)
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(true);
            }
            else if (menuActive == menuPause || menuActive != null)
            {
                stateUnpause();
            }
        }
    }

    public void stateMainMenuOpen()
    {
        statePause();
    }

    public void statePause()
    {
        isPaused = true;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void stateUnpause()
    {
        isPaused = false;
        Time.timeScale = timeScaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;
    }

    public void UnpausetoMenu()
    {
        isPaused = false;
        Time.timeScale = timeScaleOrig;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void updateGameGoal(int amount)
    {
        gameGoalCount += amount;

        if (gameGoalCount <= 0)
        {
            menuActive = menuWin;
            menuActive.SetActive(true);
            statePause();
        }
    }

    public void stateWin()
    {
        statePause();
        menuActive = menuWin;
        menuActive.SetActive(true);
    }

    public void youDied()
    {
        statePause();
        menuActive = menuDead;
        menuActive.SetActive(true);
    }

    public void SaveGame(string checkpointName = "")
    {
        if (playerScript == null)
            return;

        SaveData data = new SaveData();
        data.HP = playerScript.CurrentHealth;
        data.selectedGun = playerScript.GunListIndex;
        data.currentScene = SceneManager.GetActiveScene().name;

        if (!string.IsNullOrEmpty(checkpointName))
        {
            data.lastCheckpointName = checkpointName;
        }

        foreach (GunStats gun in playerScript.GunList)
        {
            GunStatsData g = new GunStatsData
            {
                gunName = gun.name,
                ammoCurrent = gun.ammoCurrent,
                ammoReserves = gun.ammoReserves
            };
            data.guns.Add(g);
        }

        string json = JsonUtility.ToJson(data);
        string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        PlayerPrefs.SetString(SaveKey, encoded);
        PlayerPrefs.Save();

        Debug.Log($"Game saved (Checkpoint: {data.lastCheckpointName})");
    }

    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            Debug.Log("No save data found.");
            return;
        }

        try
        {
            string encoded = PlayerPrefs.GetString(SaveKey);
            string json = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            // If the wrong scene is open, load the correct one
            if (SceneManager.GetActiveScene().name != data.currentScene)
            {
                SceneManager.LoadScene(data.currentScene);
                return;
            }
            // Restore player
            playerScript.CurrentHealth = data.HP;
            if (!string.IsNullOrEmpty(data.lastCheckpointName))
            {
                GameObject checkpoint = GameObject.Find(data.lastCheckpointName).gameObject;
                playerSpawn.transform.position = checkpoint.transform.position;
                playerSpawn.transform.rotation = checkpoint.transform.rotation;
            }
            player.transform.position = playerSpawn.transform.position;
            player.transform.rotation = playerSpawn.transform.rotation;

            // Restore gun list
            playerScript.GunList.Clear();
            foreach (GunStatsData g in data.guns)
            {
                GunStats gun = Instantiate(gunDatabase.GetGunByName(g.gunName));
                if (gun == null)
                {
                    Debug.LogWarning($"Gun '{g.gunName}' not found in database!");
                    continue;
                }

                gun.ammoCurrent = g.ammoCurrent;
                gun.ammoReserves = g.ammoReserves;
                playerScript.GunList.Add(gun);
            }

            playerScript.GunListIndex = data.selectedGun;
            playerScript.changeGun();
            playerScript.updatePlayerUI();

            Debug.Log($"Loaded checkpoint '{data.lastCheckpointName}'");
        }
        catch (Exception e)
        {
            Debug.LogWarning("Load failed: " + e.Message);
        }
    } //for loading last known player data
    

    public void ResetSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
        Debug.Log("Save data cleared.");
    }

    public void SavePlayerToMemory()
    {
        if (playerScript == null) return;

        persistentPlayerState.HP = playerScript.CurrentHealth;
        persistentPlayerState.guns.Clear();

        foreach (GunStats gun in playerScript.GunList)
        {
            persistentPlayerState.guns.Add(new GunStatsData
            {
                gunName = gun.name,
                ammoCurrent = gun.ammoCurrent,
                ammoReserves = gun.ammoReserves
            });
        }

        Debug.Log("Player state saved to memory for scene transition.");
    }

    public void LoadPlayerFromMemory()
    {
        if (playerScript == null)
        {
            Debug.Log("No memory data to load.");
            return;
        }

        playerScript.CurrentHealth = persistentPlayerState.HP;
        playerScript.GunList.Clear();

        foreach (GunStatsData g in persistentPlayerState.guns)
        {
            GunStats gun = Instantiate(gunDatabase.GetGunByName(g.gunName));
            gun.ammoCurrent = g.ammoCurrent;
            gun.ammoReserves = g.ammoReserves;
            playerScript.GunList.Add(gun);
        }

        playerScript.GunListIndex = 0;
        if (playerScript.GunList.Count > 0)
            playerScript.changeGun();
        playerScript.updatePlayerUI();

        Debug.Log("Player state restored from memory after scene load.");
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isReloadingScene)
            return;

        isReloadingScene = true;

        Debug.Log($"OnSceneLoaded: {scene.name}");

        // Clear old references
        player = null;
        playerScript = null;
        playerSpawn = null;
        playerSpawnOrig = null;
        goalObject = null;
        MainCamera = null;
        CameraScript = null;
        GetUI();

        // Wait a short delay before relinking
        StartCoroutine(ReinitializeAfterLoad(scene));
    }

    public void LoadNextLevel(string nextScene)
    {
        SavePlayerToMemory();  // Save to memory first
        SceneManager.LoadScene(nextScene);  // Then load the new scene
    }

    public void RespawnFromCheckpoint()
    {
        playerScript.CurrentHealth = playerScript.OriginalHealth;
        playerScript.transform.position = playerSpawn.transform.position;
        playerScript.transform.rotation = playerSpawn.transform.rotation;

        stateUnpause();
        menuActive = null;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        playerScript.updatePlayerUI();

        Debug.Log("Respawned at last checkpoint.");
    }

    // For finding inactive elements
    GameObject FindInactive(string name)
    {
        foreach (Transform t in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (t.hideFlags == HideFlags.None && t.name == name)
                return t.gameObject;
        }
        return null;
    }
    public void GetUI()
    {
        if (needUIReload == true)
        {
            menuActive = null;
            menuPause = null;
            menuWin = null;
            menuDead = null;
            mainMenu = null;
            menuSettings = null;
            menuLevelSelect = null;
            submenuGameplaySettings = null;
            submenuAudioSettings = null;
            playerHPBar = null;
            playerHPLabel = null;
            ammoCurrent = null;
            ammoMax = null;
            checkpointLabel = null;
            playerDamageScreen = null;
            playerHealScreen = null;
            needUIReload = false;
        }
        else
        {
            return;
        }
        GameObject uiRoot = GameObject.Find("UI");
        if (uiRoot == null)
        {
            Debug.LogWarning("No UI object found in scene.");
            return;
        }
        uiRoot.name = "UIA";
        DontDestroyOnLoad(uiRoot);
        menuWin = FindInactive("Win Menu");
        menuDead = FindInactive("Restart Menu");
        mainMenu = FindInactive("Main Menu");
        menuPause = FindInactive("Pause Menu");
        menuSettings = FindInactive("Settings Menu");
        menuLevelSelect = FindInactive("Level Select Menu");
        CreditsScreen = FindInactive("Credits");
        submenuGameplaySettings = FindInactive("Gameplay Menu");
        submenuAudioSettings = FindInactive("Audio Menu");
        submenuLockedlevel2 = FindInactive("Locked 2");
        submenuLockedlevel3 = FindInactive("Locked 3");
        submenuLockedlevel4 = FindInactive("Locked 4");
        submenuLockedlevel5 = FindInactive("Locked 5");
        submenuUnlockedlevel2button = FindInactive("Level 2 Button");
        submenuUnlockedlevel2stats = FindInactive("Level 2 Button Back Ground");
        submenuUnlockedlevel3button = FindInactive("Level 3 Button");
        submenuUnlockedlevel3stats = FindInactive("Level 3 Button Back Ground");
        submenuUnlockedlevel4button = FindInactive("Level 4 Button");
        submenuUnlockedlevel4stats = FindInactive("Level 4 Button Back Ground");
        submenuUnlockedlevel5button = FindInactive("Level 5 Button");
        submenuUnlockedlevel5stats = FindInactive("Level 5 Button Back Ground");
        currScene = SceneManager.GetActiveScene();
        playerHPBar = FindInactive("Player HP Fill").GetComponent<Image>();
        playerHPLabel = FindInactive("Player HP Label").GetComponent<TMP_Text>();
        ammoCurrent = FindInactive("Ammo Current").GetComponent<TMP_Text>();
        ammoMax = FindInactive("Ammo Max").GetComponent<TMP_Text>();
        checkpointLabel = FindInactive("Checkpoint Label");
        playerDamageScreen = FindInactive("Player Damage Screen")?.GetComponent<Image>();
        playerHealScreen = FindInactive("Player Heal Screen")?.GetComponent<Image>();
        PlayerHPDisplay = FindInactive("Player HP");
        PlayerAmmoDisplay = FindInactive("Ammo");
        PlayerReticleDisplay = FindInactive("Reticle");
        levelLocks();
        if (OpenLevelSelect == true && mainMenu.name != null && menuLevelSelect != null && currScene.name == "Main Menu Scene")
        {
            menuActive = mainMenu;
            menuActive.SetActive(true);
            menuActive.SetActive(false);
            menuActive = null;
            menuActive = menuLevelSelect;
            menuActive.SetActive(true);
            OpenLevelSelect = false;
        }
        else if (OpenMainMenu == true && mainMenu.name != null && currScene.name == "Main Menu Scene")
        {
            menuActive = mainMenu;
            menuActive.SetActive(true);
            OpenMainMenu = false;
        }

        Debug.Log("UI linked.");
    }


    private IEnumerator ReinitializeAfterLoad(Scene scene)
    {
        // Wait
        yield return new WaitForEndOfFrame();

        // Reacquire key objects
        player = GameObject.FindWithTag("Player");
        if (player != null)
            playerScript = player.GetComponent<PlayerController>();

        playerSpawn = GameObject.FindWithTag("PlayerSpawn");
        playerSpawnOrig = playerSpawn;
        goalObject = GameObject.FindWithTag("Goal");
        MainCamera = GameObject.FindWithTag("MainCamera");
        if (MainCamera != null)
            CameraScript = MainCamera.GetComponent<CameraController>();

        // Refresh UI
        GetUI();


        // Load player data only if the scene changed
        if (SceneManager.GetActiveScene().name != persistentPlayerState.lastScene)
            LoadPlayerFromMemory();

        persistentPlayerState.lastScene = SceneManager.GetActiveScene().name;

        if (playerScript != null)
            playerScript.updatePlayerUI();

        Debug.Log($"Scene '{scene.name}' initialized.");
        isReloadingScene = false;
    }
    public void menuChange(GameObject menuchoice)
    {
        if (menuActive != null)
        {
            menuActive.SetActive(false);
            menuActive = null;
        }
        menuActive = menuchoice;
        menuActive.SetActive(true);
    }
    public void submenuChange(GameObject submenuchoice)
    {
        submenuActive.SetActive(false);
        submenuActive = null;
        submenuActive = submenuchoice;
        submenuActive.SetActive(true);
    }
    public void emptyActive()
    {
        menuActive.SetActive(false);
        menuActive = null;
    }
    public void menuTolevel()
    {
        menuChange(mainMenu);
        menuChange(menuLevelSelect);
    }

    public void menuToCredits()
    {
        menuChange(mainMenu);
        menuChange(CreditsScreen);
    }
    public void statetoSettings()
    {
        menuChange(menuSettings);
        submenuActive = submenuGameplaySettings;
        submenuActive.SetActive(true);
    }
    public void backtoPausemenu()
    {
        statePause();
        menuChange(menuPause);

    }
    public void backtoMainmenu()
    {
        if (currScene.name != "Main Menu Scene")
        {
            GetUI();
        }
        menuChange(mainMenu);
    }
    public void settoMainmenu()
    {
        menuActive = mainMenu;
        menuActive.SetActive(true);
    }
    public void statetoLevelSelect()
    {

        menuChange(menuLevelSelect);
    }

    public void openGameplaySubmenu()
    {
        submenuChange(submenuGameplaySettings);
    }
    public void openAudioSubmenu()
    {
        submenuChange(submenuAudioSettings);
    }
    public void levelLocks() //used to keep track of locked and unlocked levels 
    {
        if (Level2lock == true)
        {
            submenuLockedlevel2.SetActive(true);
            submenuUnlockedlevel2button.SetActive(false);
            submenuUnlockedlevel2stats.SetActive(false);
        }
        else
        {
            submenuLockedlevel2.SetActive(false);
            submenuUnlockedlevel2button.SetActive(true);
            submenuUnlockedlevel2stats.SetActive(true);
        }
        if (Level3lock == true)
        {
            submenuLockedlevel3.SetActive(true);
            submenuUnlockedlevel3button.SetActive(false);
            submenuUnlockedlevel3stats.SetActive(false);
        }
        else
        {
            submenuLockedlevel3.SetActive(false);
            submenuUnlockedlevel3button.SetActive(true);
            submenuUnlockedlevel3stats.SetActive(true);
        }
        if (Level4lock == true)
        {
            submenuLockedlevel4.SetActive(true);
            submenuUnlockedlevel4button.SetActive(false);
            submenuUnlockedlevel4stats.SetActive(false);
        }
        else
        {
            submenuLockedlevel4.SetActive(false);
            submenuUnlockedlevel4button.SetActive(true);
            submenuUnlockedlevel4stats.SetActive(true);
        }
        if (Level5lock == true)
        {
            submenuLockedlevel5.SetActive(true);
            submenuUnlockedlevel5button.SetActive(false);
            submenuUnlockedlevel5stats.SetActive(false);
        }
        else
        {
            submenuLockedlevel5.SetActive(false);
            submenuUnlockedlevel5button.SetActive(true);
            submenuUnlockedlevel5stats.SetActive(true);
        }
    }
    public void clearActive()
    {
        if (menuActive.activeInHierarchy == true)
        {
            menuActive.SetActive(false);
        }
        menuActive = null;
    }

    public void enablePlayerUI()
    {
        PlayerHPDisplay.SetActive(true);
        PlayerAmmoDisplay.SetActive(true);
        PlayerReticleDisplay.SetActive(true);
    }
    public void disablePlayerUI()
    {
        PlayerHPDisplay.SetActive(false);
        PlayerAmmoDisplay.SetActive(false);
        PlayerReticleDisplay.SetActive(false);
    }
}
