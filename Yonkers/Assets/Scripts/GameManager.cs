using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuDead;
    [SerializeField] GameObject MainMenu;

    [SerializeField] GameObject goalObject;

    public GameObject player;
    public PlayerController playerScript;

    public bool isPaused;

    float timeScaleOrig;

    private int gameGoalCount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
        timeScaleOrig = Time.timeScale;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerController>();
    }

    private void Start()
    {
        statePause();
        menuActive = MainMenu;
        menuActive.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
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
