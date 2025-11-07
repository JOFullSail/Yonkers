using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

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
            Transform cameraTransform = SceneView.lastActiveSceneView.camera.transform;
            if (playerSpawn != null)
            {
                playerSpawn.transform.position = cameraTransform.position;
                playerSpawn.transform.rotation = new Quaternion(0, cameraTransform.rotation.y, 0, cameraTransform.rotation.w);
                playerSpawnOrig = playerSpawn;
            }
            else
            {
                player.transform.position = cameraTransform.position;
                player.transform.rotation = new Quaternion(0, cameraTransform.rotation.y, 0, cameraTransform.rotation.w);
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
}
