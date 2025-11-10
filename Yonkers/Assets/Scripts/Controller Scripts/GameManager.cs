using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuDead;
    [SerializeField] GameObject menuLevelComplete;
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject submenuActive;
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
    [SerializeField] GameObject PlayerReticleDisplay;
    [SerializeField] GameObject PlayerDashCoolDownDisplay;

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip levelMusic;

    public GameObject playerSpawn;

    public GameObject player;
    public PlayerController playerScript;
    public GameObject goalObject;
    public GameObject MainCamera;
    public CameraController CameraScript;
    public GameObject blindScreen;
    public GameObject hypnoScreen;
    public GameObject webScreen;

    public Image playerBrightnessOverlay;
    public Image playerHPBar;
    public TMP_Text playerHPLabel;
    public TMP_Text ScoreLC; //for when you comeplete levels
    public TMP_Text GradeLC;//for when you comeplete levels
    public TMP_Text ScoreLVL1;
    public TMP_Text ScoreLVL2;
    public TMP_Text ScoreLVL3;
    public TMP_Text ScoreLVL4;
    public TMP_Text ScoreLVL5;
    public Image GradeLVL1;
    public Image GradeLVL2;
    public Image GradeLVL3;
    public Image GradeLVL4;
    public Image GradeLVL5;
    public GameObject checkpointLabel;
    public Image playerDamageScreen;
    public Image playerHealScreen;
    public TMP_Text ammoCurrent, ammoMax, ammoReserves;
    public Image playerDashCooldown;

    public bool isPaused;
    private bool isReloadingScene = false;
    private bool needUIReload = false;

    float timeScaleOrig;
    public GameObject playerSpawnOrig;

    int gameGoalCount;
    public Scene currScene;

    public char finalGrade;

    [Header("Gun Database")]
    public GunDatabase gunDatabase;

    [Header("Level List - Ordered")]
    [SerializeField] List<string> levelOrder = new List<string>();

    private const string SaveKey = "PlayerSaveData";
    private const string ProgressKey = "MetaProgressionData";

    private Dictionary<string, int> levelScores = new Dictionary<string, int>();
    private Dictionary<string, int> levelGrades = new Dictionary<string, int>();

    private HashSet<string> unlockedLevels = new HashSet<string>();

    Color colorOrig;

    [Serializable]
    public class PlayerSaveData
    {
        public int HP;
        public int selectedGun;
        public string currentScene;
        public string lastCheckpointName;
        public List<GunStatsData> guns = new List<GunStatsData>();
    }

    [Serializable]
    public class ProgressData
    {
        public List<string> unlockedLevels = new List<string>();
        public List<string> levelNames = new List<string>();
        public List<int> levelScores = new List<int>();
        public List<int> levelGrades = new List<int>(); // ASCII Codes
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

            levelOrder.Clear();
            for (int i = 2; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                levelOrder.Add(name);
            }

            LoadProgression();

            if (currScene.name != "Main Menu Scene First Open")
            {
                player = GameObject.FindWithTag("Player");
                playerScript = player.GetComponent<PlayerController>();
                playerSpawn = GameObject.FindWithTag("PlayerSpawn");
                playerSpawnOrig = playerSpawn;
                goalObject = GameObject.FindWithTag("Goal");
                MainCamera = GameObject.FindWithTag("MainCamera");
                CameraScript = MainCamera.GetComponent<CameraController>();
            }

            timeScaleOrig = Time.timeScale;
            instance.GetUI();
            menuActive = mainMenu;
            colorOrig = playerDamageScreen.color;
            menuActive.SetActive(true);

            if (playerScript != null)
                playerScript.updatePlayerUI();


            PlayMenuMusic();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
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

    public void stateLevelComplete()
    {
        statePause();
        menuActive = menuLevelComplete;
        
        menuActive.SetActive(true);
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
        if (menuActive != null)
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

    /// <summary>
    /// Save the player's current state along with the last checkpoint they hit.
    /// </summary>
    public void SaveGame(string checkpointName = "")
    {
        if (playerScript == null)
            return;

        PlayerSaveData data = new PlayerSaveData();
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

    /// <summary>
    /// Load the player's last saved state and place them at the last checkpoint they hit.
    /// </summary>
    public bool LoadGame()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            Debug.Log("No save data found.");
            return false;
        }

        try
        {
            string encoded = PlayerPrefs.GetString(SaveKey);
            string json = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);
            if(data.lastCheckpointName != null && data.lastCheckpointName != "")
            {
                GameObject spawn = GameObject.Find(data.lastCheckpointName);
                if (spawn != null)
                {
                    Checkpoint checkpoint = spawn.GetComponent<Checkpoint>();
                    if (checkpoint != null)
                    {
                        Transform spawnPos = checkpoint.SpawnPos;
                        if (spawnPos != null)
                        {
                            playerSpawn.transform.position = spawnPos.position;
                            playerSpawn.transform.rotation = spawnPos.rotation;
                        }
                    }
                }
            }
            // Restore player
            
            if(player != null)
            {
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
            }
            

            Debug.Log($"Loaded checkpoint '{data.lastCheckpointName}'");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("Load failed: " + e.Message);
            return false;
        }
    }

    /// <summary>
    /// Erase the player's saved state
    /// </summary>
    public void ResetSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
        Debug.Log("Save data cleared.");
    }

    /// <summary>
    /// Save level progression and scores
    /// </summary>
    public void SaveProgression()
    {
        ProgressData progress = new ProgressData();
        progress.unlockedLevels = unlockedLevels.ToList();


        foreach (var kvp in levelScores)
        {
            progress.levelNames.Add(kvp.Key);
            progress.levelScores.Add(kvp.Value);
            progress.levelGrades.Add(levelGrades.TryGetValue(kvp.Key, out int grade) ? grade : 'D');
        }

        string jsonP = JsonUtility.ToJson(progress);
        string encodedP = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonP));

        PlayerPrefs.SetString(ProgressKey, encodedP);
        PlayerPrefs.Save();

        Debug.Log("Progress saved.");
    }

    /// <summary>
    /// Load level progression and scores. This should be done on game start.
    /// </summary>
    public void LoadProgression()
    {
        // first-time playthrough
        if (!PlayerPrefs.HasKey(ProgressKey))
        {
            unlockedLevels.Clear();
            if (levelOrder.Count > 0)
                unlockedLevels.Add(levelOrder[0]); 
            return;
        }

        string encoded = PlayerPrefs.GetString(ProgressKey);
        string json = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
        ProgressData data = JsonUtility.FromJson<ProgressData>(json);

        unlockedLevels = new HashSet<string>(data.unlockedLevels);
        levelScores.Clear();
        levelGrades.Clear();

        for (int i = 0; i < data.levelNames.Count; i++)
        {
            string name = data.levelNames[i];
            levelScores[name] = data.levelScores[i];
            levelGrades[name] = data.levelGrades[i];
        }

        Debug.Log("Progress loaded.");
    }

    /// <summary>
    /// Reset level progression and scores.
    /// </summary>
    public void ResetProgression()
    {
        PlayerPrefs.DeleteKey(ProgressKey);
        PlayerPrefs.Save();

        unlockedLevels.Clear();
        levelScores.Clear();
        levelGrades.Clear();

        if (levelOrder.Count > 0)
            unlockedLevels.Add(levelOrder[0]);

        Debug.Log("Progress reset.");
    }

    /// <summary>
    /// Save player state between scene transitions.
    /// </summary>
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

    /// <summary>
    /// Load player state after scene load
    /// </summary>
    public void LoadPlayerFromMemory()
    {
        if (playerScript == null)
        {
            Debug.Log("No memory data to load.");
            return;
        }

        playerScript.CurrentHealth = persistentPlayerState.HP;
        if (playerScript.CurrentHealth <= 0)
            playerScript.CurrentHealth = playerScript.OriginalHealth;
        playerScript.GunList.Clear();
        
        foreach (GunStatsData g in persistentPlayerState.guns)
        {
            if (g != null && gunDatabase.GetGunByName(g.gunName) != null)
            {
                GunStats gun = Instantiate(gunDatabase.GetGunByName(g.gunName));
                gun.ammoCurrent = g.ammoCurrent;
                gun.ammoReserves = g.ammoReserves;
                playerScript.GunList.Add(gun);
            }
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

        EventController.OnGameComplete += HandleGameWon;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        EventController.OnGameComplete -= HandleGameWon;
    }

    /// <summary>
    /// Subscribes to sceneLoaded event. This should never be called manually.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isReloadingScene)
            return;

        isReloadingScene = true;

        Debug.Log($"OnSceneLoaded: {scene.name}");

        if (scene.name.Contains("Menu"))
            PlayMenuMusic();
        else if (scene.name.Contains("Level") || scene.name.Contains("Scene"))
        {
            PlayLevelMusic();
        }


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

    /// <summary>
    /// Save player and load given level.
    /// </summary>
    public void LoadNextLevel(string nextScene)
    {
        SavePlayerToMemory();  // Save to memory first
        SceneManager.LoadScene(nextScene);  // Then load the new scene
    }

    /// <summary>
    /// Heal player and place them where the last checkpoint they hit was.
    /// </summary>
    public void RespawnFromCheckpoint(bool heal)
    {
        if (heal)
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

    /// <summary>
    /// Helper for finding inactive objects
    /// </summary>
    GameObject FindInactive(string name)
    {
        foreach (Transform t in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (t.hideFlags == HideFlags.None && t.name == name)
                return t.gameObject;
        }
        return null;
    }

    /// <summary>
    /// Get UI in current scene
    /// </summary>
    private void GetUI()
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
            menuLevelComplete = null;
            submenuGameplaySettings = null;
            submenuAudioSettings = null;
            playerHPBar = null;
            playerHPLabel = null;
            playerDashCooldown = null;
            ammoCurrent = null;
            ammoMax = null;
            checkpointLabel = null;
            playerDamageScreen = null;
            playerHealScreen = null;
            needUIReload = false;
            playerBrightnessOverlay = null;
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

        menuLevelComplete = FindInactive("Level Complete Menu");
        playerBrightnessOverlay = FindInactive("Brightness Overlay")?.GetComponent<Image>();
        playerDashCooldown = FindInactive("Player Dash Cooldown Fill")?.GetComponent<Image>();

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
        ScoreLC = FindInactive("ScoreNumber").GetComponent<TMP_Text>();
        GradeLC = FindInactive("Grade Letter").GetComponent<TMP_Text>();
        ScoreLVL1 = FindInactive("Score Text 1").GetComponent<TMP_Text>();
        ScoreLVL2 = FindInactive("Score Text 2").GetComponent<TMP_Text>();
        ScoreLVL3 = FindInactive("Score Text 3").GetComponent<TMP_Text>();
        ScoreLVL4 = FindInactive("Score Text 4").GetComponent<TMP_Text>();
        ScoreLVL5 = FindInactive("Score Text 5").GetComponent<TMP_Text>();
        GradeLVL1 = FindInactive("Grade Image 1").GetComponent<Image>(); 
        GradeLVL2 = FindInactive("Grade Image 2").GetComponent<Image>();
        GradeLVL3 = FindInactive("Grade Image 3").GetComponent<Image>();
        GradeLVL4 = FindInactive("Grade Image 4").GetComponent<Image>();
        GradeLVL5 = FindInactive("Grade Image 5").GetComponent<Image>();
        ammoCurrent = FindInactive("Ammo Current").GetComponent<TMP_Text>();
        ammoMax = FindInactive("Ammo Max").GetComponent<TMP_Text>();
        ammoReserves = FindInactive("Ammo Reserves").GetComponent<TMP_Text>();
        checkpointLabel = FindInactive("Checkpoint Label");
        playerDamageScreen = FindInactive("Player Damage Screen")?.GetComponent<Image>();
        playerHealScreen = FindInactive("Player Heal Screen")?.GetComponent<Image>();
        PlayerHPDisplay = FindInactive("Player HP");
        PlayerAmmoDisplay = FindInactive("Ammo");
        PlayerReticleDisplay = FindInactive("Reticle");
        PlayerDashCoolDownDisplay = FindInactive("Player Dash Cooldown");

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

    /// <summary>
    /// Refresh references on scene load.
    /// </summary>
    private IEnumerator ReinitializeAfterLoad(Scene scene)
    {
        // Wait
        yield return new WaitForEndOfFrame();

        // Reacquire key objects
        player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerScript = player.GetComponent<PlayerController>();
        }

        playerSpawn = GameObject.FindWithTag("PlayerSpawn");
        playerSpawnOrig = playerSpawn;
        goalObject = GameObject.FindWithTag("Goal");
        MainCamera = GameObject.FindWithTag("MainCamera");
        if (MainCamera != null)
            CameraScript = MainCamera.GetComponent<CameraController>();

        // Refresh UI
        GetUI();
        playerDamageScreen.color = new Color(colorOrig.r, colorOrig.g, colorOrig.b, 0f);

        // Load player data only if the scene changed
        if (SceneManager.GetActiveScene().name != persistentPlayerState.lastScene)
            LoadPlayerFromMemory();

        persistentPlayerState.lastScene = SceneManager.GetActiveScene().name;

        if (playerScript != null)
            playerScript.updatePlayerUI();

        Debug.Log($"Scene '{scene.name}' initialized.");
        isReloadingScene = false;

        isPaused = false;

        if (scene.name.Contains("Level") || scene.name.Contains("Scene"))
        {
            if (PlayerPrefs.HasKey(SaveKey))
                LoadGame();
        }
        Time.timeScale = timeScaleOrig;
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
        PlayerDashCoolDownDisplay.SetActive(true);
        playerDashCooldown.gameObject.SetActive(true);
    }
    public void disablePlayerUI()
    {
        PlayerHPDisplay.SetActive(false);
        PlayerAmmoDisplay.SetActive(false);
        PlayerReticleDisplay.SetActive(false);
        PlayerDashCoolDownDisplay.SetActive(false);
        playerDashCooldown.gameObject.SetActive(false);
    }

    /// <summary>
    /// Record the current level's score.
    /// </summary>
    public void RecordLevelScore(int score, char grade)
    {
        string levelName = SceneManager.GetActiveScene().name;

        levelScores[levelName] = score;
        levelGrades[levelName] = grade;
        ScoreLC.text = score.ToString();
        GradeLC.text = grade.ToString();

        Debug.Log((char)levelGrades[levelName] + " rank recorded for " + levelName);
    }

    /// <summary>
    /// Calculate Final Grade
    /// </summary>
    public char GetFinalGrade()
    {
        int avg = (int)levelGrades.Values.Average();

        if (avg >= 'S') return 'S';
        else if (avg >= 'A') return 'A';
        else if (avg >= 'B') return 'B';
        else if (avg >= 'C') return 'C';
        else return 'D';
    }

    /// <summary>
    /// Get a given level's score.
    /// </summary>
    public int GetLevelScore(string levelName)
    {
        return levelScores.TryGetValue(levelName, out int score) ? score : 0;
    }

    /// <summary>
    /// Get a given level's letter grade.
    /// </summary>
    public char GetLevelGrade(string levelName)
    {
        return levelGrades.TryGetValue(levelName, out int score) ? (char)score : ' ';
    }

    /// <summary>
    /// Unlock the next level in sequence.
    /// </summary>
    public void UnlockNextLevel()
    {
        string currentLevel = SceneManager.GetActiveScene().name;

        int currentIndex = levelOrder.IndexOf(currentLevel);
        if (currentIndex != -1 && currentIndex + 1 < levelOrder.Count)
        {
            string nextLevel = levelOrder[currentIndex + 1];
            unlockedLevels.Add(nextLevel);
            Debug.Log($"Unlocked {nextLevel}");
        }
    }

    /// <summary>
    /// Check if a given level is unlocked.
    /// </summary>
    public bool IsLevelUnlocked(string levelName)
    {
        return unlockedLevels.Contains(levelName);
    }

    /// <summary>
    /// What happens when you win the game.
    /// </summary>
    private void HandleGameWon()
    {
        // TODO - Play cutscene
        finalGrade = GetFinalGrade();
        GradeLC.text = finalGrade.ToString();
        Debug.Log("Final Grade is " + finalGrade);
        stateWin();
    }

    public void PlayMenuMusic()
    {
        if (musicSource == null || menuMusic == null) return;
        if (musicSource.clip == menuMusic && musicSource.isPlaying) return;

        musicSource.clip = menuMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayLevelMusic()
    {
        if (musicSource == null || levelMusic == null) return;
        if (musicSource.clip == levelMusic && musicSource.isPlaying) return;

        musicSource.clip = levelMusic;
        musicSource.loop = true;
        musicSource.Play();
    }
    public void levelLocks() //used to keep track of locked and unlocked levels 
    {
        int index = 2;
        int maxIndex = levelOrder.Count() - 1;
        if (!IsLevelUnlocked(levelOrder[index]))
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
        if (index < maxIndex)
        {
            index++;
        }
        else
        {
            return;
        }

        if (!IsLevelUnlocked(levelOrder[index]))
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
        if (index < maxIndex)
        {
            index++;
        }
        else
        {
            return;
        }
        if (!IsLevelUnlocked(levelOrder[index]))
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
        if (index < maxIndex)
        {
            index++;
        }
        else
        {
            return;
        }
        if (!IsLevelUnlocked(levelOrder[index]))
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
}
