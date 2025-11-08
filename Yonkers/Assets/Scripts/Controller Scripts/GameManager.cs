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
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuDead;
    [SerializeField] GameObject menuLevelComplete;
    [SerializeField] GameObject mainMenu;

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

    public char finalGrade;

    [Header("Gun Database")]
    public GunDatabase gunDatabase;

    [Header("Level List - Ordered")]
    [SerializeField]List<string> levelOrder = new List<string>();

    private const string SaveKey = "PlayerSaveData";
    private const string ProgressKey = "MetaProgressionData";

    private Dictionary<string, int> levelScores = new Dictionary<string, int>();
    private Dictionary<string, int> levelGrades = new Dictionary<string, int>();

    private HashSet<string> unlockedLevels = new HashSet<string>();

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

    public PlayerState persistentPlayerState = new PlayerState();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            levelOrder.Clear();
            for (int i = 1; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                levelOrder.Add(name);
            }

            LoadProgression();

            player = GameObject.FindWithTag("Player");
            playerScript = player.GetComponent<PlayerController>();
            playerSpawn = GameObject.FindWithTag("PlayerSpawn");
            playerSpawnOrig = playerSpawn;
            goalObject = GameObject.FindWithTag("Goal");
            MainCamera = GameObject.FindWithTag("MainCamera");
            CameraScript = MainCamera.GetComponent<CameraController>();
            timeScaleOrig = Time.timeScale;
            GetUI();

            bool loaded = LoadGame();

            if (!loaded && playerScript != null)
            {
                // No save found — start with full health
                playerScript.CurrentHealth = playerScript.OriginalHealth;
            }

            if (playerScript != null)
                playerScript.updatePlayerUI();
        }
        else
        {
            Destroy(gameObject);
            return;
        }

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

    private void Start()
    {
        if (enableMainMenu)
        {
            stateMainMenuOpen();
            menuActive = mainMenu;
            menuActive.SetActive(true);
        }
    }

    void Update()
    {
        if (menuActive == mainMenu && Input.GetKeyDown(KeyCode.Space))
            stateUnpause();

        if (Input.GetButtonDown("Cancel"))
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

        // TESTING
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F5)) SaveProgression();
        if (Input.GetKeyDown(KeyCode.F9)) LoadProgression();
        if (Input.GetKeyDown(KeyCode.F10)) ResetProgression();
#endif
    }

    public void stateLevelComplete()
    {
        statePause();
        menuActive = menuLevelComplete;
        menuActive.SetActive(true);
    }
    public void stateMainMenuOpen()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
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

        if(gameGoalCount <= 0) 
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

            // If the wrong scene is open, load the correct one
            if (SceneManager.GetActiveScene().name != data.currentScene)
            {
                SceneManager.LoadScene(data.currentScene);
            }

            // Restore player
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
        menuLevelComplete = FindInactive("Level Complete Menu");

        playerHPBar = FindInactive("Player HP Fill")?.GetComponent<Image>();
        playerHPLabel = FindInactive("Player HP Label")?.GetComponent<TMP_Text>();
        ammoCurrent = FindInactive("Ammo Current")?.GetComponent<TMP_Text>();
        ammoMax = FindInactive("Ammo Max")?.GetComponent<TMP_Text>();

        checkpointLabel = FindInactive("Checkpoint Label");
        playerDamageScreen = FindInactive("Player Damage Screen")?.GetComponent<Image>();
        playerHealScreen = FindInactive("Player Heal Screen")?.GetComponent<Image>();

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

        if(playerScript != null)
            playerScript.updatePlayerUI();

        Debug.Log($"Scene '{scene.name}' initialized.");
        isReloadingScene = false;
    }

    /// <summary>
    /// Record the current level's score.
    /// </summary>
    public void RecordLevelScore(int score, char grade)
    {
        string levelName = SceneManager.GetActiveScene().name;
        levelScores[levelName] = score;
        levelGrades[levelName] = grade;

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
        Debug.Log("Final Grade is " + finalGrade);
        stateWin();
    }
}
