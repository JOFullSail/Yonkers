using UnityEngine;

public class RoomScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public GameObject door1;
    [SerializeField] GameObject[] Enemy;
    public GameObject InvisWalls;
    [SerializeField] int MaxEnemy;
    [SerializeField] float spawnRate;
    [SerializeField] Transform[] spawnPos;
    int currenemyCount;
    int enemyCounttotal;
    float spawnTimer;
    bool startspawning;
    bool hasentered;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startspawning = false;
        hasentered = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (startspawning)
        {
            spawnTimer += Time.deltaTime;
            if (enemyCounttotal < MaxEnemy && spawnTimer >= spawnRate)
            {
                spawn();
            }
        }

    }
    void spawn()
    {
        int SpawnarrayPos = Random.Range(0, spawnPos.Length); 
        int EnemyarrayPos = Random.Range(0, Enemy.Length);
        GameObject EnemyClone = Instantiate(Enemy[EnemyarrayPos], spawnPos[SpawnarrayPos].position, spawnPos[SpawnarrayPos].rotation);
        EnemyClone.GetComponent<EnemyAI>().room = this;


        spawnTimer = 0;
        enemyCounttotal++;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasentered)
        {
            startspawning = true;
            hasentered = true;
            RoomState(true);
        }
    }
    void RoomState(bool state)
    {
        if(door1!= null)
        {
        door1.SetActive(state);
        }
        if(InvisWalls != null)
        {
        InvisWalls.SetActive(state);
        }
    }
    public void UpdateEnemyCount(int amount)
    {
        currenemyCount += amount;
        if (currenemyCount <= 0)
        {
            RoomState(false);
            startspawning = false;
        }

    }
}
