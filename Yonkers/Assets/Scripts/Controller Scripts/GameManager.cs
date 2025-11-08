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
    //[SerializeField] GameObject CreditsScreen; //not made yet
    [SerializeField] GameObject submenuGameplaySettings;
    [SerializeField] GameObject submenuAudioSettings;

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

    // This object lives in memory between scene transitions
    public PlayerState persistentPlayerState = new PlayerState();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            currScene = SceneManager.GetActiveScene();
            if (currScene.name != "Main Menu Scene")
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

            GetUI();
        }
        else
        {
            Destroy(gameObject);
            return;
        }

#if UNITY_EDITOR
        if (currScene.name != "Main Menu Scene")
        {
            if (playerScript.DebugSpawnAtCamera)
            {
                if (playerSpawn != null)
                {
                    Transform cameraTransform = SceneView.lastActiveSceneView.camera.transform;
                    playerSpawn.transform.position = cameraTransform.position;
                    playerSpawn.transform.rotation = new Quaternion(0, cameraTransform.rotation.y, 0, 1);
                    playerSpawnOrig = playerSpawn;
                }
                else player.transform.position = SceneView.lastActiveSceneView.camera.transform.position;
            }
        }
#endif
    }

    private void Start()
    {
        if (enableMainMenu)
        {
            stateMainMenuOpen();
            menuActive = mainMenu;
            menuActive.SetActive(true);
        }
        currScene = SceneManager.GetActiveScene();
        if(currScene.name != "Main Menu Scene")
        {
            InLevelUpdate();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (menuActive == mainMenu && Input.GetKeyDown(KeyCode.Space))
            stateUnpause();

        if (Input.GetButtonDown("Cancel") && currScene.name != "Main Menu Scene")
        {
            if (menuActive == null)
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(true);
            }
            else if (menuActive == menuPause)
            {
                stateUnpause();
            }
        }
        if (currScene.name != "Main Menu Scene")
        {
            currScene = SceneManager.GetActiveScene();
        }
    }

    public void stateMainMenuOpen()
    {
        statePause();
    }

    public void statePause()
    {
        isPaused = !isPaused;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void stateUnpause()
    {
        isPaused = !isPaused;
        Time.timeScale = timeScaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;
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
    }

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
        SceneManager.sceneLoaded -= OnSceneLoaded;
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
    private void GetUI()
    {
        if (menuPause != null && menuWin != null && menuDead != null && playerHPBar != null)
            return;

        GameObject uiRoot = GameObject.Find("UI");
        if (uiRoot == null)
        {
            Debug.LogWarning("No UI object found in scene.");
            return;
        }

        menuPause = FindInactive("Pause Menu");
        menuWin = FindInactive("Win Menu");
        menuDead = FindInactive("Lose Menu");
        mainMenu = FindInactive("Main Menu");
        menuSettings = FindInactive("Settings Menu");
        menuLevelSelect = FindInactive("Level Select Menu");
        submenuGameplaySettings = FindInactive("Gameplay Menu");
        submenuAudioSettings = FindInactive("Audio Menu");
        if (currScene.name != "Main Menu Scene")
        {
            playerHPBar = FindInactive("Player HP Fill")?.GetComponent<Image>();
            playerHPLabel = FindInactive("Player HP Label")?.GetComponent<TMP_Text>();
            ammoCurrent = FindInactive("Ammo Current")?.GetComponent<TMP_Text>();
            ammoMax = FindInactive("Ammo Max")?.GetComponent<TMP_Text>();
            checkpointLabel = FindInactive("Checkpoint Label");
            playerDamageScreen = FindInactive("Player Damage Screen")?.GetComponent<Image>();
            playerHealScreen = FindInactive("Player Heal Screen")?.GetComponent<Image>();
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
        menuActive.SetActive(false);
        menuActive = null;
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

    public void InLevelUpdate()
    {
        if (GameObject.FindWithTag("Player") != null)
        {
          player = GameObject.FindWithTag("Player");
            playerScript = player.GetComponent<PlayerController>(); 
            playerSpawn = GameObject.FindWithTag("PlayerSpawn");
            playerSpawnOrig = playerSpawn;
            goalObject = GameObject.FindWithTag("Goal");
            GetUI();
    #if UNITY_EDITOR
            if (playerScript.DebugSpawnAtCamera)
            {
                if (playerSpawn != null)
                {
                    Transform cameraTransform = SceneView.lastActiveSceneView.camera.transform;
                    playerSpawn.transform.position = cameraTransform.position;
                    playerSpawn.transform.rotation = new Quaternion(0, cameraTransform.rotation.y, 0, 1);
                    playerSpawnOrig = playerSpawn;
                }
                else player.transform.position = SceneView.lastActiveSceneView.camera.transform.position;
            }
        
    #endif
        }
    }
}
