using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public AudioClip levelMusic;
    [Range(0, 1)] public float levelMusicVol;

    [Header("Grade Scaling")]
    [SerializeField] int levelMaxScore = 600;
    [SerializeField] int gradeSMinScore = 500;
    [SerializeField] int gradeAMinScore = 400;
    [SerializeField] int gradeBMinScore = 300;
    [SerializeField] int gradeCMinScore = 200;
    [SerializeField] float pointslostrate = 20;
    [SerializeField] bool sRankNeedsAllEnemiesDead;

    [Header("Score Modifiers")]
    public int pointsLostOnRespawn = 0;

    public float currentScore;
    private int numberOfEnemies;

    public enum Grade
    {
        D = 'D',
        C = 'C',
        B = 'B',
        A = 'A',
        S = 'S'
    }

    private Grade levelGrade;
    private int levelScore;

    public int EnemyCount
    {
        get { return numberOfEnemies; }
        set { numberOfEnemies = value; }
    }

    public float CurrentScore
    {
        get { return currentScore; }
        set { currentScore = value; }
    }

    private void Awake()
    {
        instance = this;
        currentScore = levelMaxScore;
    }
    private void Start()
    {

    }

    private void Update()
    {
        if (currentScore > 0)
        {
            currentScore -= pointslostrate * Time.deltaTime;
        }

        if (currentScore > levelMaxScore)
            currentScore = levelMaxScore;
    }

    private void OnEnable()
    {
        EventController.OnLevelComplete += HandleLevelComplete;
    }

    private void OnDisable()
    {
        EventController.OnLevelComplete -= HandleLevelComplete;
    }

    private void HandleLevelComplete()
    {
        levelScore = (int)currentScore;

        if (levelScore >= gradeSMinScore)
        {
            levelGrade = Grade.S;
            if (sRankNeedsAllEnemiesDead && numberOfEnemies > 0)
                levelGrade = Grade.A;
        }

        else if (levelScore >= gradeAMinScore)
            levelGrade = Grade.A;
        else if (levelScore >= gradeBMinScore)
            levelGrade = Grade.B;
        else if (levelScore >= gradeCMinScore)
            levelGrade = Grade.C;
        else
            levelGrade = Grade.D;

        GameManager.instance.RecordLevelScore(levelScore, (char)levelGrade);

        GameManager.instance.UnlockNextLevel();

        GameManager.instance.SaveProgression();

        GameManager.instance.SaveInventory();

        GameManager.instance.ResetSave();

        GameManager.instance.stateLevelComplete();
    }
    public char currentGrade()
    {
        levelScore = (int)currentScore;

        if (levelScore >= gradeSMinScore)
        {
            levelGrade = Grade.S;
            if (sRankNeedsAllEnemiesDead && numberOfEnemies > 0)
                levelGrade = Grade.A;
        }

        else if (levelScore >= gradeAMinScore)
            levelGrade = Grade.A;
        else if (levelScore >= gradeBMinScore)
            levelGrade = Grade.B;
        else if (levelScore >= gradeCMinScore)
        {
            levelGrade = Grade.C;
        }
        else
        {
            levelGrade = Grade.D;
        }
        return (char)levelGrade;
    }
}
