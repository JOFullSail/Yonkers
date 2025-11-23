
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using static UnityEngine.Analytics.IAnalytic;

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
    [SerializeField] GameObject submenuLockedlevel6;
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
    [SerializeField] GameObject PlayerClimbStaminaDisplay;
    [SerializeField] GameObject LoadingScreen;
    [SerializeField] ButtonFunctions buttonFunctions;
    [SerializeField] Sprite gradeiconS;
    [SerializeField] Sprite gradeiconA;
    [SerializeField] Sprite gradeiconB;
    [SerializeField] Sprite gradeiconC;
    [SerializeField] Sprite gradeiconD;
    [SerializeField] Sprite gradeiconUn;//ungraded

    [SerializeField] GameObject inLVLScorenGrade;
    public TMP_ColorGradient hasAmmoGradient;
    public TMP_ColorGradient NoAmmoGradient;
    public TMP_ColorGradient SrankGradient;
    public TMP_ColorGradient ArankGradient;
    public TMP_ColorGradient BrankGradient;
    public TMP_ColorGradient CrankGradient;
    public TMP_ColorGradient DrankGradient;
    public TMP_ColorGradient UnrankedGradient;

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip levelMusic;
    public AudioMixer audMix;
    public TMP_Text FOVcurrentNumber;
    public TMP_Text MouseSenscurrentNumber;
    public TMP_Text BrightnesscurrentNumber;
    public TMP_Text MastervolcurrentNumber;
    public TMP_Text MusicvolcurrentNumber;
    public TMP_Text SFXvolcurrentNumber;
    public TMP_Text CVoicescurrentNumber;
    public GameObject lvlManagerObj;
    public GameObject playerSpawn;
    public GameObject player;
    public PlayerController playerScript;
    public GameObject goalObject;
    public GameObject MainCamera;
    public CameraController CameraScript;
    public Camera cam;
    public GameObject blindScreen;
    public GameObject hypnoScreen;
    public GameObject webScreen;
    public GameObject ExitButton;
    public Image playerBrightnessOverlay;
    public Image playerHPBar;
    public TMP_Text playerHPLabel;
    public TMP_Text ScoreLC; //for when you comeplete levels
    public TMP_Text LevelnameLC; //for when you comeplete levels
    public Image GradeLC;//for when you comeplete levels
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
    public Image OverallGrade;
    public GameObject checkpointLabel;
    public Image playerDamageScreen;
    public Image playerHealScreen;
    public TMP_Text ammoCurrent, ammoReserves;
    public Image playerClimbStamina;
    public TMP_Text LoadingScreendotdotdot; // the "..." of teh loading screen!
    public Image inLvLgrade;
    public TMP_Text inLvlgradetext;
    public TMP_Text inLvlScoreNumber;
    public LevelManager currentlevelManager;
    public TMP_Text WinGradetxt;
    public Image WinGradeImg;
    public Button[] respawnButtons;

    public bool isPaused;
    private bool isReloadingScene = false;
    private bool needUIReload = false;

    float timeScaleOrig;
    public GameObject playerSpawnOrig;

    int gameGoalCount;
    public Scene currScene;
    int loadingdottick;
    char currentLvLgrade;
    public char finalGrade = 'U';


    [Header("Gun Database")]
    public GunDatabase gunDatabase;

    [Header("Level List - Ordered")]
    [SerializeField] List<string> levelOrder = new List<string>();

    private const string SaveKey = "PlayerSaveData";
    private const string ProgressKey = "MetaProgressionData";
    private const string InvKey = "InventoryData";

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

        public float posX, posY, posZ;
        public float rotX, rotY, rotZ;

        public float currentScore;
    }

    [Serializable]
    public class InventoryData
    {
        public List<GunStatsData> guns = new List<GunStatsData>();
    }

    [Serializable]
    public class ProgressData
    {
        public List<string> unlockedLevels = new List<string>();
        public List<string> levelNames = new List<string>();
        public List<int> levelScores = new List<int>();
        public List<int> levelGrades = new List<int>(); // ASCII Codes
        public char SavedFinal = 'n';
    }

    [Serializable]
    public class GunStatsData
    {
        public string gunName;
        public int ammoCurrent;
        public int ammoReserves;
        public int Maxammo;
        public int MaxammoReserves;
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
            GetUI();
            levelOrder.Clear();
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string name = System.IO.Path.GetFileNameWithoutExtension(path);

                if (name.StartsWith("Level"))
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
                cam = MainCamera.GetComponent<Camera>();
            }

            timeScaleOrig = Time.timeScale;
            menuActive = mainMenu;
            colorOrig = playerDamageScreen.color;
            menuActive.SetActive(true);
            if (playerScript != null)
                playerScript.updatePlayerUI();


            PlayMenuMusic();
        }
    }
    void Update()
    {
#if UNITY_WEBGL
        if (Input.GetButtonDown("Pause") && SceneManager.GetActiveScene().name != "Main Menu Scene" && SceneManager.GetActiveScene().name != "Main Menu Scene First Open" && menuActive != menuDead)
        {
            if (menuActive == null)
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(true);

            }
            else if ((menuActive == menuPause || menuActive != null) && menuActive != LoadingScreen && menuActive != menuSettings)
            {
                stateUnpause();
            }
        }
        ExitButton.SetActive(false);
#else
        if (Input.GetButtonDown("Cancel") && SceneManager.GetActiveScene().name != "Main Menu Scene" && SceneManager.GetActiveScene().name != "Main Menu Scene First Open"  && menuActive != menuDead)
        {
            if (menuActive == null)
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(true);

            }
            else if ((menuActive == menuPause || menuActive != null) && menuActive != LoadingScreen && menuActive != menuSettings)
            {
                stateUnpause();
            }
        }
#endif
        if (playerScript != null)
        {
            if (playerScript.GunList.Count > 0 && SceneManager.GetActiveScene().name != "Main Menu Scene" && GameManager.instance.menuActive == null)
            {
                PlayerAmmoDisplay.SetActive(true);
            }
            else
            {
                PlayerAmmoDisplay.SetActive(false);
            }
        }
        else if (GameManager.instance.menuActive != null)
        {

            PlayerAmmoDisplay.SetActive(false);
        }
        if (currentlevelManager != null)
        {
            currentLvLgrade = currentlevelManager.currentGrade();
            inLvLgrade.sprite = GradeImageGetter(currentLvLgrade);
            inLvlgradetext.colorGradientPreset = GradientGetter(currentLvLgrade);
            inLvlScoreNumber.colorGradientPreset = GradientGetter(currentLvLgrade);
            inLvlScoreNumber.text = currentlevelManager.currentScore.ToString("F0");
        }
        if (menuActive == null)
        {
            enablePlayerUI();
        }
        else
        {
            disablePlayerUI();
        }

        if (menuActive != null && (menuActive == menuSettings || menuActive == menuLevelSelect || menuActive == CreditsScreen) && (Input.GetButton("Cancel") || Input.GetButton("Pause")))
            buttonFunctions.Backfrom();
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
    public void SaveGame()
    {
        if (playerScript == null)
            return;

        PlayerSaveData data = new PlayerSaveData();
        data.HP = playerScript.OriginalHealth;
        data.selectedGun = playerScript.GunListIndex;
        data.currentScene = SceneManager.GetActiveScene().name;

        data.posX = playerSpawn.transform.position.x;
        data.posY = playerSpawn.transform.position.y;
        data.posZ = playerSpawn.transform.position.z;

        data.rotX = playerSpawn.transform.rotation.eulerAngles.x;
        data.rotY = playerSpawn.transform.rotation.eulerAngles.y;
        data.rotZ = playerSpawn.transform.rotation.eulerAngles.z;

        data.currentScore = currentlevelManager.CurrentScore;

        SaveInventory();

        string json = JsonUtility.ToJson(data);
        string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        PlayerPrefs.SetString(SaveKey, encoded);
        PlayerPrefs.Save();

        //Debug.Log($"Game saved");
    }

    public void SaveInventory()
    {
        if (playerScript == null)
            return;

        InventoryData data = new InventoryData();
        foreach (GunStats gun in playerScript.GunList)
        {
            GunStatsData g = new GunStatsData
            {
                gunName = gun.name.Replace("(Clone)", "").Trim(),
                ammoCurrent = gun.ammoCurrent,
                ammoReserves = gun.ammoReserves,
                Maxammo = gun.ammoMax,
                MaxammoReserves = gun.maxAmmoReserves
            };
            data.guns.Add(g);
        }

        string json = JsonUtility.ToJson(data);
        string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        PlayerPrefs.SetString(InvKey, encoded);
        PlayerPrefs.Save();
    }

    public void LoadInventory()
    {
        if (!PlayerPrefs.HasKey(InvKey))
            return;

        string encoded = PlayerPrefs.GetString(InvKey);
        string json = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
        InventoryData data = JsonUtility.FromJson<InventoryData>(json);

        playerScript.GunList.Clear();
        if (data.guns != null)
        {
            foreach (GunStatsData g in data.guns)
            {
                GunStats gun = Instantiate(gunDatabase.GetGunByName(g.gunName));

                if (gun == null)
                    continue;

                gun.ammoCurrent = g.ammoCurrent;
                gun.ammoReserves = g.ammoReserves;

                playerScript.GunList.Add(gun);
            }
        }
    }
    /// <summary>
    /// Load the player's last saved state and place them at the last checkpoint they hit.
    /// </summary>
    public bool LoadGame()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            //Debug.Log("No save data found.");
            return false;
        }

        try
        {
            string encoded = PlayerPrefs.GetString(SaveKey);
            string json = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);
            // Restore player

            if (player != null)
            {
                playerScript.CurrentHealth = data.HP;
                player.transform.position = new Vector3(data.posX, data.posY, data.posZ);
                player.transform.rotation = Quaternion.Euler(data.rotX, data.rotY, data.rotZ);

                playerSpawn.transform.position = player.transform.position;
                playerSpawn.transform.rotation = player.transform.rotation;

                currentlevelManager.CurrentScore = data.currentScore;

                // Restore gun list
                LoadInventory();

                playerScript.GunListIndex = data.selectedGun;
                playerScript.changeGun();
            }
            playerScript.updatePlayerUI();

            //Debug.Log($"Loaded in!");
            return true;
        }
        catch (Exception)
        {
            //Debug.LogWarning("Load failed: " + e.Message);
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
        //Debug.Log("Save data cleared.");
    }

    public void ResetInventory()
    {
        PlayerPrefs.DeleteKey(InvKey);
        PlayerPrefs.Save();
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
        progress.SavedFinal = finalGrade;
        string jsonP = JsonUtility.ToJson(progress);
        string encodedP = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonP));

        PlayerPrefs.SetString(ProgressKey, encodedP);
        PlayerPrefs.Save();

        //Debug.Log("Progress saved.");
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
            levelScores.Clear();
            levelGrades.Clear();
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
            int score = data.levelScores[i];
            char grade = (char)data.levelGrades[i];

            levelScores[name] = score;
            levelGrades[name] = grade;

            int uiIndex = levelOrder.IndexOf(name);

            if (uiIndex == 0)
            {
                ScoreLVL1.text = score.ToString();
                GradeLVL1.sprite = GradeImageGetter(grade);
            }
            else if (uiIndex == 1)
            {
                ScoreLVL2.text = score.ToString();
                GradeLVL2.sprite = GradeImageGetter(grade);
            }
            else if (uiIndex == 2)
            {
                ScoreLVL3.text = score.ToString();
                GradeLVL3.sprite = GradeImageGetter(grade);
            }
            else if (uiIndex == 3)
            {
                ScoreLVL4.text = score.ToString();
                GradeLVL4.sprite = GradeImageGetter(grade);
            }
            else if (uiIndex == 4)
            {
                ScoreLVL5.text = score.ToString();
                GradeLVL5.sprite = GradeImageGetter(grade);
            }
            finalGrade = data.SavedFinal;
            if(finalGrade != 'U')
            {
                OverallGrade.sprite = GradeImageGetter(finalGrade);
            }
        }
        finalGrade = data.SavedFinal;
        OverallGrade.sprite = GradeImageGetter(finalGrade);
        //Debug.Log("Progress loaded.");
    }

    public void ResetLevelSelectUI()
    {
        ScoreLVL1.text = "0000000";
        GradeLVL1.sprite = gradeiconUn;

        ScoreLVL2.text = "00000000";
        GradeLVL2.sprite = gradeiconUn;

        ScoreLVL3.text = "00000000";
        GradeLVL3.sprite = gradeiconUn;

        ScoreLVL4.text = "00000000";
        GradeLVL4.sprite = gradeiconUn;

        ScoreLVL5.text = "00000000";
        GradeLVL5.sprite = gradeiconUn;

        OverallGrade.sprite = gradeiconUn;
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

        ResetLevelSelectUI();

        //Debug.Log("Progress reset.");
    }

    /// <summary>
    /// Save player state between scene transitions.
    /// </summary>
    public void SavePlayerToMemory()
    {
        if (playerScript == null) return;

        persistentPlayerState.HP = playerScript.OriginalHealth;
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

        ////Debug.Log("Player state saved to memory for scene transition.");
    }

    /// <summary>
    /// Load player state after scene load
    /// </summary>
    public void LoadPlayerFromMemory()
    {
        if (playerScript == null)
        {
            //Debug.Log("No memory data to load.");
            return;
        }

        playerScript.CurrentHealth = playerScript.OriginalHealth;
        if (playerScript.CurrentHealth <= 0)
            playerScript.CurrentHealth = playerScript.OriginalHealth;
        playerScript.GunList.Clear();

        if (persistentPlayerState.guns.Count() != 0)
        {
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
        }
        lvlManagerObj = FindInactive("LevelManager");
        currentlevelManager = lvlManagerObj.GetComponent<LevelManager>();
        playerScript.updatePlayerUI();

        //Debug.Log("Player state restored from memory after scene load.");
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

        //Debug.Log($"OnSceneLoaded: {scene.name}");

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
        cam = null;

        foreach (Button button in respawnButtons)
            button.enabled = true;
        // Wait a short delay before relinking
        StartCoroutine(ReinitializeAfterLoad(scene));
    }

    /// <summary>
    /// Save player and load given level.
    /// </summary>
    public void LoadNextLevel(string nextScene, bool saveMemory = true)
    {
        if (saveMemory)
            SavePlayerToMemory();

        StartCoroutine(LoadingScreenEnable(nextScene));

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

        playerScript.clearKnockback();
        playerScript.movementResetFull();

        stateUnpause();
        menuActive = null;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        playerScript.updatePlayerUI();

        //Debug.Log("Respawned at last checkpoint.");
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
        if (needUIReload == true || PlayerHPDisplay == null)
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
            playerClimbStamina = null;
            ammoCurrent = null;
            checkpointLabel = null;
            playerDamageScreen = null;
            playerHealScreen = null;
            needUIReload = false;
            playerBrightnessOverlay = null;

        }
        else return;

        GameObject uiRoot = GameObject.Find("UI");
        if (uiRoot == null)
        {
            //Debug.LogWarning("No UI object found in scene.");
            return;
        }
        uiRoot.name = "UIA";
        DontDestroyOnLoad(uiRoot);
        menuLevelComplete = FindInactive("Level Complete Menu");
        playerBrightnessOverlay = FindInactive("Brightness Overlay")?.GetComponent<Image>();
        playerClimbStamina = FindInactive("Player Climb Stamina Fill")?.GetComponent<Image>();
        menuWin = FindInactive("Win Menu");
        menuDead = FindInactive("Restart Menu");
        mainMenu = FindInactive("Main Menu");
        menuPause = FindInactive("Pause Menu");
        menuSettings = FindInactive("Settings Menu");
        menuLevelSelect = FindInactive("Level Select Menu (1)");
        CreditsScreen = FindInactive("Credits (1)");
        submenuGameplaySettings = FindInactive("Gameplay Menu");
        submenuAudioSettings = FindInactive("Audio Menu");
        ExitButton = FindInactive("Exit Game Button");
        submenuLockedlevel2 = FindInactive("Locked 2 (1)");
        submenuLockedlevel3 = FindInactive("Locked 3");
        submenuLockedlevel4 = FindInactive("Locked 4");
        submenuLockedlevel5 = FindInactive("Locked 5");
        FOVcurrentNumber = FindInactive("FOV Current Value").GetComponent<TMP_Text>(); ;
        MouseSenscurrentNumber = FindInactive("Mouse Sens Current Value").GetComponent<TMP_Text>(); ;
        BrightnesscurrentNumber = FindInactive("Brightness Current Value").GetComponent<TMP_Text>(); ;
        MastervolcurrentNumber = FindInactive("MasterVol Current Value").GetComponent<TMP_Text>(); ;
        MusicvolcurrentNumber = FindInactive("MusicVol Current Value").GetComponent<TMP_Text>(); ;
        SFXvolcurrentNumber = FindInactive("SFXVol Current Value").GetComponent<TMP_Text>(); ;
        CVoicescurrentNumber = FindInactive("Character Voices Vol Current Value").GetComponent<TMP_Text>(); ;
        submenuUnlockedlevel2button = FindInactive("Level 2 Button (1)");
        submenuUnlockedlevel2stats = FindInactive("Level 2 Button Back Ground (1)");
        submenuUnlockedlevel3button = FindInactive("Level 3 Button");
        submenuUnlockedlevel3stats = FindInactive("Level 3 Button Back Ground");
        submenuUnlockedlevel4button = FindInactive("Level 4 Button");
        submenuUnlockedlevel4stats = FindInactive("Level 4 Button Back Ground");
        submenuUnlockedlevel5button = FindInactive("Level 5 Button");
        submenuUnlockedlevel5stats = FindInactive("Level 5 Button Back Ground");
        inLVLScorenGrade = FindInactive("In Level Score and Grade");
        currScene = SceneManager.GetActiveScene();
        playerHPBar = FindInactive("Player HP Fill").GetComponent<Image>();
        playerHPLabel = FindInactive("Player HP Label").GetComponent<TMP_Text>();
        LevelnameLC = FindInactive("Level Name LC").GetComponent<TMP_Text>();
        ScoreLC = FindInactive("ScoreNumber").GetComponent<TMP_Text>();
        GradeLC = FindInactive("Grade Image LC").GetComponent<Image>();
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
        OverallGrade = FindInactive("OVALGradeimg").GetComponent<Image>();
        ammoCurrent = FindInactive("Ammo Current").GetComponent<TMP_Text>();
        ammoReserves = FindInactive("Ammo Reserves").GetComponent<TMP_Text>();
        checkpointLabel = FindInactive("Checkpoint Label");
        playerDamageScreen = FindInactive("Player Damage Screen")?.GetComponent<Image>();
        playerHealScreen = FindInactive("Player Heal Screen")?.GetComponent<Image>();
        PlayerHPDisplay = FindInactive("Player HP");
        PlayerAmmoDisplay = FindInactive("Ammo");
        PlayerReticleDisplay = FindInactive("Reticle");
        PlayerClimbStaminaDisplay = FindInactive("Player Climb Stamina");
        LoadingScreen = FindInactive("Loading Screen");
        LoadingScreendotdotdot = FindInactive("Loading Text ...").GetComponent<TMP_Text>();
        inLvLgrade = FindInactive("inlvlGradeImg").GetComponent<Image>();
        inLvlScoreNumber = FindInactive("InlevelScoreNumber").GetComponent<TMP_Text>();
        inLvlgradetext = FindInactive("inlvlGrade:").GetComponent<TMP_Text>();
        WinGradetxt = FindInactive("RealFinalGrade:").GetComponent<TMP_Text>();
        WinGradeImg = FindInactive("FinalGradingImage").GetComponent<Image>();
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
        //Debug.Log("UI linked.");
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
        {
            CameraScript = MainCamera.GetComponent<CameraController>();
            cam = MainCamera.GetComponent<Camera>();
        }
        lvlManagerObj = FindInactive("LevelManager");
        if (lvlManagerObj != null)
            currentlevelManager = lvlManagerObj.GetComponent<LevelManager>();

        // Refresh UI
        GetUI();
        playerDamageScreen.color = new Color(colorOrig.r, colorOrig.g, colorOrig.b, 0f);

        string loadedScene = SceneManager.GetActiveScene().name;

        bool usedCheckpointSave = false;

        // CASE 1: checkpoint load for this scene
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string encoded = PlayerPrefs.GetString(SaveKey);
            string json = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            PlayerSaveData save = JsonUtility.FromJson<PlayerSaveData>(json);

            if (save.currentScene == loadedScene)
            {
                LoadGame();
                usedCheckpointSave = true;
            }
        }

        // CASE 2: normal level transition
        bool cameFromMenu =
        GameManager.OpenMainMenu ||
        GameManager.OpenLevelSelect ||
        scene.name == "Main Menu Scene" ||
        scene.name == "Main Menu Scene First Open";

        if (!usedCheckpointSave && loadedScene != persistentPlayerState.lastScene && !cameFromMenu)
        {
            LoadPlayerFromMemory();
        }

        persistentPlayerState.lastScene = loadedScene;

        isReloadingScene = false;
        isPaused = false;
        Time.timeScale = timeScaleOrig;

        OpenMainMenu = false;
        OpenLevelSelect = false;

        if (!SceneManager.GetActiveScene().name.Contains("Menu"))
        {
            LoadInventory();
            playerScript.changeGun();
            enablePlayerUI();
            stateUnpause();
            playerScript.updatePlayerUI();
            SaveGame();
        }
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
        PlayerReticleDisplay.SetActive(true);
        PlayerClimbStaminaDisplay.SetActive(true);
        playerClimbStamina.gameObject.SetActive(true);
        inLVLScorenGrade.SetActive(true);
    }
    public void disablePlayerUI()
    {
        PlayerHPDisplay.SetActive(false);
        PlayerReticleDisplay.SetActive(false);
        PlayerClimbStaminaDisplay.SetActive(false);
        playerClimbStamina.gameObject.SetActive(false);
        inLVLScorenGrade.SetActive(false);
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
        GradeLC.sprite = GradeImageGetter(grade);
        LevelnameLC.text = levelName;
        UpdateLevelSelect(levelName, score, grade);

        //Debug.Log((char)levelGrades[levelName] + " rank recorded for " + levelName);
    }

    /// <summary>
    /// Calculate Final Grade
    /// </summary>
    public char GetFinalGrade()
    {
        if (levelGrades.Count > 0)
        {
            int avg = (int)levelGrades.Values.Average();

            if (avg >= 'S') return 'S';
            else if (avg >= 'A') return 'A';
            else if (avg >= 'B') return 'B';
            else if (avg >= 'C') return 'C';
            else return 'D';
        }
        return 'F';
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
            //Debug.Log($"Unlocked {nextLevel}");
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
        currentlevelManager.HandleFinalLevelComplete(); 
        finalGrade = GetFinalGrade();
        OverallGrade.sprite = GradeImageGetter(finalGrade);
        WinGradetxt.colorGradientPreset = GradientGetter(finalGrade);
        WinGradeImg.sprite = GradeImageGetter(finalGrade);
        OverallGrade.sprite = GradeImageGetter(finalGrade);
        //Debug.Log("Final Grade is " + finalGrade);
        SaveProgression();
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
        if (musicSource == null) return;

        if (LevelManager.instance != null && LevelManager.instance.levelMusic != null)
            levelMusic = LevelManager.instance.levelMusic;

        if (musicSource.clip == levelMusic && musicSource.isPlaying) return;

        musicSource.clip = levelMusic;
        musicSource.loop = true;
        musicSource.volume = LevelManager.instance.levelMusicVol;
        musicSource.Play();
    }
    public void levelLocks() //used to keep track of locked and unlocked levels 
    {
        int index = 1;
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
        if (index < maxIndex)
        {
            index++;
        }
        else
        {
            return;
        }
    }
    IEnumerator LoadingScreenEnable(string level)
    {
        menuChange(LoadingScreen);
        AsyncOperation Loading = SceneManager.LoadSceneAsync(level);
        loadingdottick = 0;
        while (!Loading.isDone)
        {
            if (loadingdottick == 0)
            {
                LoadingScreendotdotdot.text = "";
            }
            else if (loadingdottick == 1)
            {
                LoadingScreendotdotdot.text = ".";
            }
            else if (loadingdottick == 2)
            {
                LoadingScreendotdotdot.text = "..";
            }
            else if (loadingdottick == 3)
            {
                LoadingScreendotdotdot.text = "...";
            }
            loadingdottick++;
            if (loadingdottick == 4)
            {
                loadingdottick = 0;
            }
            yield return null;
        }
        if (level != "Main Menu Scene")
        {
            clearActive();
        }

    }
    private Sprite GradeImageGetter(char letter)
    {
        Sprite gradetoReturn;
        if (letter == 'S')
        {
            gradetoReturn = gradeiconS;
        }
        else if (letter == 'A')
        {
            gradetoReturn = gradeiconA;
        }
        else if (letter == 'B')
        {
            gradetoReturn = gradeiconB;
        }
        else if (letter == 'C')
        {
            gradetoReturn = gradeiconC;
        }
        else if (letter == 'D')
        {
            gradetoReturn = gradeiconD;
        }
        else
        {
            gradetoReturn = gradeiconUn;
        }
        return gradetoReturn;
    }
    private TMP_ColorGradient GradientGetter(char letter)
    {
        TMP_ColorGradient gradienttoReturn;
        if (letter == 'S')
        {
            gradienttoReturn = SrankGradient;
        }
        else if (letter == 'A')
        {
            gradienttoReturn = ArankGradient;
        }
        else if (letter == 'B')
        {
            gradienttoReturn = BrankGradient;
        }
        else if (letter == 'C')
        {
            gradienttoReturn = CrankGradient;
        }
        else if (letter == 'D')
        {
            gradienttoReturn = DrankGradient;
        }
        else
        {
            gradienttoReturn = UnrankedGradient;
        }
        return gradienttoReturn;
    }
    private void UpdateLevelSelect(string levelname, int score, char grade)
    {
        for (int index = 0; index < levelOrder.Count; index++)
        {
            if (levelname == levelOrder[index])
            {
                if (index == 0)
                {
                    ScoreLVL1.text = score.ToString();
                    GradeLVL1.sprite = GradeImageGetter(grade);
                }
                else if (index == 1)
                {
                    ScoreLVL2.text = score.ToString();
                    GradeLVL2.sprite = GradeImageGetter(grade);
                }
                else if (index == 2)
                {
                    ScoreLVL3.text = score.ToString();
                    GradeLVL3.sprite = GradeImageGetter(grade);
                }
                else if (index == 3)
                {
                    ScoreLVL4.text = score.ToString();
                    GradeLVL4.sprite = GradeImageGetter(grade);
                }
                else if (index == 4)
                {
                    ScoreLVL5.text = score.ToString();
                    GradeLVL5.sprite = GradeImageGetter(grade);
                }
            }
        }
    }
    IEnumerator Loadingdots()
    {
        yield return new WaitForSecondsRealtime(1f);
    }
}
