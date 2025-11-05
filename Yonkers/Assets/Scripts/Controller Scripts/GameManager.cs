using System;
using System.Text;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuDead;
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
    public TMP_Text ammoCurrent, ammoMax;

    public bool isPaused;

    float timeScaleOrig;
    public GameObject playerSpawnOrig;

    int gameGoalCount;

    [Header("Gun Database")]
    public GunDatabase gunDatabase;

    private const string SaveKey = "PlayerSaveData";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
        timeScaleOrig = Time.timeScale;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerController>();
        playerSpawn = GameObject.FindWithTag("PlayerSpawn");
        playerSpawnOrig = playerSpawn;
        goalObject = GameObject.FindWithTag("Goal");
        MainCamera = GameObject.FindWithTag("MainCamera");
        CameraScript = MainCamera.GetComponent<CameraController>();

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

    // Update is called once per frame
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
    }

    public void stateMainMenuOpen()
    {
        isPaused = true;
        Time.timeScale = 0;
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

    public void SaveGame(string checkpointName = "", Vector3 checkpointPos = default)
    {
        if (playerScript = null)
            return;

        SaveData data = new SaveData();
        data.HP = playerScript.CurrentHealth;
        Vector3 pos = playerScript.transform.position;
        data.posX = pos.x;
        data.posY = pos.y;
        data.posZ = pos.z;
        data.selectedGun = playerScript.GunListIndex;
        data.currentScene = SceneManager.GetActiveScene().name;

        if (!string.IsNullOrEmpty(checkpointName))
        {
            data.lastCheckpointName = checkpointName;
            data.checkpointX = checkpointPos.x;
            data.checkpointY = checkpointPos.y;
            data.checkpointZ = checkpointPos.z;
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
            playerScript.transform.position = new Vector3(data.posX, data.posY, data.posZ);

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
        LoadGame();
    }

    public void RespawnFromCheckpoint()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            Debug.LogWarning("No checkpoint data found. Restarting scene...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        try
        {
            string encoded = PlayerPrefs.GetString(SaveKey);
            string json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            // Restore player
            playerScript.CurrentHealth = data.HP;
            playerScript.transform.position = new Vector3(data.checkpointX, data.checkpointY, data.checkpointZ);

            // Restore guns
            playerScript.GunList.Clear();
            foreach (GunStatsData g in data.guns)
            {
                GunStats gun = Instantiate(gunDatabase.GetGunByName(g.gunName));
                gun.ammoCurrent = g.ammoCurrent;
                gun.ammoReserves = g.ammoReserves;
                playerScript.GunList.Add(gun);
            }

            playerScript.GunListIndex = data.selectedGun;
            playerScript.changeGun();
            playerScript.updatePlayerUI();

            // Reset UI
            stateUnpause();
            menuActive = null;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            Debug.Log($"Player respawned at checkpoint '{data.lastCheckpointName}'");
        }
        catch (Exception e)
        {
            Debug.LogWarning("Respawn failed: " + e.Message);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}

[Serializable]
public class SaveData
{
    public int HP;
    public float posX, posY, posZ;
    public int selectedGun;
    public string currentScene;
    public string lastCheckpointName;
    public float checkpointX, checkpointY, checkpointZ;
    public List<GunStatsData> guns = new List<GunStatsData>();
}

[Serializable]
public class GunStatsData
{
    public string gunName;
    public int ammoCurrent;
    public int ammoReserves;
}
