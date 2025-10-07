using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;

    [SerializeField] NavMeshAgent agent;

    [SerializeField] int HP;

    [SerializeField] int faceTargetSpeed;
    [SerializeField] int FOV;
    [SerializeField] int roamDist;
    [SerializeField] int roamPauseTime;

    [SerializeField] float baseMoveSpeed;

    [SerializeField] bool canMove;

    [SerializeField] bool canFly;

    [SerializeField] bool canDodge;

    [SerializeField] bool enragesWhenDamaged;

    [SerializeField] float enrageSpeedIncrease;

    [SerializeField] bool explodesOnDeath;
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] int explosionDamage;

    Color colorOrig;

    protected bool playerInRange = false;
    protected bool alreadyEnraged;

    Vector3 playerDir;
    Vector3 startPos;

    float roamTimer;
    float angleToPlayer;
    protected float stoppingDistanceOrig;
    protected float originalMoveSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalMoveSpeed = baseMoveSpeed;
        agent.speed = baseMoveSpeed;

        alreadyEnraged = false;

        colorOrig = model.material.color;

        stoppingDistanceOrig = agent.stoppingDistance;
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        roamRoutine();
    }

    protected void roamRoutine()
    {
        if (agent.remainingDistance < 0.01f)
        {
            roamTimer += Time.deltaTime;
        }

        if (playerInRange && !canSeePlayer())
        {
            checkRoam();
        }
        else if (!playerInRange)
        {
            checkRoam();
        }
    }
    void checkRoam()
    {
        if (roamTimer >= roamPauseTime && agent.remainingDistance < 0.01f)
        {
            roam();
        }
    }

    void roam()
    {
        roamTimer = 0;
        agent.stoppingDistance = 0;

        Vector3 ranPos = Random.insideUnitSphere * roamDist;
        ranPos += startPos;

        NavMeshHit hit;
        NavMesh.SamplePosition(ranPos, out hit, roamDist, 1);
        agent.SetDestination(hit.position);
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        agent.SetDestination(GameManager.instance.player.transform.position);

        if (HP <= 0)
        {
            if (explodesOnDeath && explosionPrefab != null)
            {
                GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Explosion expl = explosion.GetComponent<Explosion>();
                if (expl != null)
                    expl.TriggerExplosion(transform.position, explosionDamage);
            }

            Destroy(gameObject);

        }
        else
        {
            StartCoroutine(flashRed());
        }

        if(enragesWhenDamaged && !alreadyEnraged)
        {
            incrementmovespeed(enrageSpeedIncrease);

            alreadyEnraged = true;
        }
            
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }
    protected bool canSeePlayer()
    {
        playerDir = GameManager.instance.player.transform.position - transform.position;
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);
        Debug.DrawRay(transform.position, playerDir);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, playerDir, out hit))
        {
            Debug.Log(hit.collider.name);

            if (angleToPlayer <= (FOV) && hit.collider.CompareTag("Player") && playerInRange)
            {
                agent.SetDestination(GameManager.instance.player.transform.position);

                if (agent.remainingDistance <= stoppingDistanceOrig)
                    faceTarget();

                agent.stoppingDistance = stoppingDistanceOrig;
                return true;
            }
        }

        agent.stoppingDistance = 0;
        return false;
    }

    void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, 0, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }

    protected void setnewmovespeed(float newSpeed)
    {
        baseMoveSpeed = newSpeed;
        agent.speed = newSpeed;
    }

    protected void incrementmovespeed(float speedModifier)
    {
        agent.speed += speedModifier;
    }
}
