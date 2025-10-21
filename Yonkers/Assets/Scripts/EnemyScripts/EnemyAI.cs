using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour, IDamage, IPushback
{
    [SerializeField] Renderer model;

    public Animator animator;

    [SerializeField] NavMeshAgent agent;

    [SerializeField] int HP = 2;
    [Tooltip("Field of view the enemy will have to detect the player.")]
    [SerializeField] int FOV = 90;
    [Tooltip("Speed the enemy travels in.\n\n- Will get overridden by the Enrage Speed Increase.")]
    [SerializeField] float baseMoveSpeed = 5f;

    [Tooltip("Allows the enemy to move and follow the player.")]
    [SerializeField] bool canMove = true;

    [Tooltip("Allows the enemy to rotate when shooting at the player.\n\n- The enemy will still rotate when the enemy moves around.")]
    [SerializeField] bool canRotate = true;

    [Tooltip("Allows the enemy to roam around the area while they aren't detecting the player.\n\n" +
        "- Will be disabled if Can Move is OFF or if the Roam Distance is set to 0.")]
    [SerializeField] bool canRoam;

    //[SerializeField] bool canFly;

    //[SerializeField] bool canDodge;

    [Tooltip("Allows the enemy to continue roaming around while shooting at the player.\n\n- Stopping distance will be reset to 0.")]
    [SerializeField] bool alwaysRoaming;

    [Tooltip("Randomizes the amount of time the enemy waits until its next roam cycle.\n\n" +
        "- Don't forget to set the Random Min Roam Time and Random Max Roam Time fields!")]
    [SerializeField] bool randomizeRoamTime;

    [Tooltip("Allows the enemy to always face the player when detected.\n\n- Paired with Always Roaming.")]
    [SerializeField] bool facingWhileRoaming;

    [Tooltip("Allows the enemy to approach the player when shot as the player is detected by the enemy.\n\n" +
        "- Paired with Always Roaming.")]
    [SerializeField] bool approachWhenShot = true;

    [Tooltip("Allows the enemy to take damage while the enemy's attacks are delayed by Attack Delay or Initial Attack Delay.\n\n" +
        "- This should only be used for bosses.")]
    [SerializeField] bool damageWhenDelayed;

    [Tooltip("Allows the enemy to take damage while the player remains undetected by the enemy.\n\n" +
        "- Should be turned OFF for bosses.")]
    [SerializeField] bool damageWhenUndetected = true;

    [Tooltip("Makes the enemy move faster when shot.")]
    [SerializeField] bool enragesWhenDamaged;

    [SerializeField] bool explodesOnDeath;

    [Tooltip("Rotational speed of the enemy when facing the player if detected.")]
    [SerializeField] int faceTargetSpeed = 5;
    [Tooltip("Distance between the enemy and the player the enemy will attempt not to cross.")]
    [SerializeField] float stoppingDistance = 10f;

    [Tooltip("Radius of the starting position of the enemy to the given value where the enemy will roam around in.")]
    [SerializeField] float roamDist;
    [Tooltip("Amount of time (in seconds) the enemy waits until its next roam cycle.")]
    [SerializeField] float roamPauseTime = 2f;
    [Tooltip("Minimum inclusive range of seconds the enemy will randomly choose to pause their roam cycle.\n\n" +
        "- Randomize Pause Time must be ON to utilize this field.")]
    [SerializeField] float randomMinRoamTime = 1f;
    [Tooltip("Maximum inclusive range of seconds the enemy will randomly choose to pause their roam cycle.\n\n" +
        "- Randomize Pause Time must be ON to utilize this field.")]
    [SerializeField] float randomMaxRoamTime = 2f;
    [Tooltip("Amount of time (in seconds) the enemy will wait until it starts attacking the player for the first time\n" +
        "(this is mostly for bosses since bossfights have an intro cutscene before fighting; " +
        "you can't just show up and get attacked immediately).\n\n- This will only happen once.")]
    [SerializeField] protected float initialAttackDelay = 1;
    [Tooltip("Amount of time (in seconds) the enemy will wait until it starts attacking the player.\n\n" +
        "- This will happen every time the the enemy detects the player.\n" +
        "- Recommended to set it to a number greater or equal to 1. " +
        "It will prevent the enemy from firing immediately to the direction it faced earlier.")]
    [SerializeField] protected float attackDelay = 1f;

    [Tooltip("Speed increase that will be added to the enemy speed once it's enraged.")]
    [SerializeField] int enrageSpeedIncrease;

    [SerializeField] GameObject explosionPrefab;
    [SerializeField] int explosionDamage;

    Color colorOrig;

    [Tooltip("Will be true if an enemy has not met the player yet," +
        " turning false once canSeePlayer() returns true.")]
    protected bool firstTimeMet = true;
    [Tooltip("Will be true if the enemy spots the player.")]
    protected bool playerDetected = false;
    [Tooltip("Will be true if the player enters the enemy's sphere collider.")]
    protected bool playerInRange = false;
    protected bool alreadyEnraged = false;

    Vector3 playerDir;
    Vector3 startPos;

    float roamTimer = 0f;
    float angleToPlayer;
    protected float attackDelayTimer;
    protected float stoppingDistanceOrig;
    protected float originalMoveSpeed;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (canMove)
        {
            originalMoveSpeed = baseMoveSpeed;
            agent.speed = baseMoveSpeed;
        }
        else
        {
            originalMoveSpeed = baseMoveSpeed = 0.0f;
            agent.speed = 0.0f;
        }

        if (roamDist == 0 || !canMove)
        {
            canRoam = false;
            roamDist = 0.0f;
        }

        if (faceTargetSpeed == 0)
        {
            canRotate = false;
        }

        if (!canRotate)
        {
            facingWhileRoaming = false;
        }

        if (!canRoam)
        {
            alwaysRoaming = false;
            randomizeRoamTime = false;
        }

        if (!alwaysRoaming)
        {
            facingWhileRoaming = false;
            agent.stoppingDistance = stoppingDistanceOrig = stoppingDistance;
        }
        else agent.stoppingDistance = stoppingDistanceOrig = stoppingDistance = 0.0f;

        if (initialAttackDelay == 0.0f)
        {
            initialAttackDelay = attackDelay;
        }

        if (!randomizeRoamTime)
        {
            randomMinRoamTime = randomMaxRoamTime = 0.0f;
        }

        colorOrig = model.material.color;
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        enemyRoutine();
    }

    protected void enemyRoutine() // roamRoutine();
    {

        animator.SetFloat("Speed", agent.velocity.normalized.magnitude);

        timers();

        if (playerInRange && !canSeePlayer() && canRoam)
        {
            checkRoam();
        }
        else if (!playerInRange && canRoam)
        {
            checkRoam();
        }
        // For alwaysRoaming
        else if (alwaysRoaming && (firstTimeMet && initialAttackDelay <= 0.0f ||
            !firstTimeMet && attackDelayTimer <= 0.0f))
        {
            checkRoam();
        }
    }

    void timers()
    {
        if (playerDetected && initialAttackDelay <= 0.0f)
        {
            firstTimeMet = false;
            attackDelayTimer -= Time.deltaTime;
        }
        else if (playerDetected)
        {
            initialAttackDelay -= Time.deltaTime;
        }

        if (agent.remainingDistance < 0.01f)
        {
            roamTimer += Time.deltaTime;
        }
    }
    void checkRoam()
    {
        if (roamTimer >= roamPauseTime) //? No && remaining distance check?
        {
            roam();
        }
    }

    void roam()
    {
        roamTimer = 0.0f;
        agent.stoppingDistance = 0.0f;

        if (randomizeRoamTime)
        {
            roamPauseTime = Random.Range(randomMinRoamTime, randomMaxRoamTime);
        }


        Vector3 ranPos = Random.insideUnitSphere * roamDist;
        ranPos += startPos;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(ranPos, out hit, roamDist, 1))
        {
            agent.SetDestination(hit.position);
        }
    }

    public void takeDamage(int amount)
    {
        bool damageTaken = checkDamage(amount);

        if (canMove && approachWhenShot) agent.SetDestination(GameManager.instance.player.transform.position);

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
        else if (damageTaken)
        {
            StartCoroutine(flashRed());
        }

        if (enragesWhenDamaged && !alreadyEnraged)
        {
            incrementMoveSpeed(enrageSpeedIncrease);

            alreadyEnraged = true;
        }
    }

    // For damageWhenDelayed and damageWhenUndetected (took me ten hours to figure out :D)
    bool checkDamage(int amount)
    {
        if (!damageWhenDelayed && playerDetected)
        {
            if (firstTimeMet && initialAttackDelay <= 0.0f || !firstTimeMet && attackDelayTimer <= 0.0f)
            {
                HP -= amount;
                return true;
            }
            else return false;
        }

        if (!damageWhenUndetected && !playerDetected)
        {
            return false;
        }

        HP -= amount;
        return true;
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
        {
            agent.ResetPath();
            playerDetected = false;
            playerInRange = false;
            attackDelayTimer = attackDelay;
        }
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

            if (angleToPlayer <= (FOV) && hit.collider.CompareTag("Player") && playerInRange)
            {
                if (canMove && !alwaysRoaming && initialAttackDelay <= 0.0f)
                {
                    agent.SetDestination(GameManager.instance.player.transform.position);
                    agent.stoppingDistance = stoppingDistanceOrig;
                }

                if (alwaysRoaming && facingWhileRoaming || agent.remainingDistance <= stoppingDistanceOrig)
                    faceTarget();

                playerDetected = true;
                return playerDetected;
            }
        }

        attackDelayTimer = attackDelay;
        agent.stoppingDistance = 0;
        playerDetected = false;
        return playerDetected;
    }

    void faceTarget()
    {
        if (canRotate)
        {
            Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, 0.0f, playerDir.z));
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
        }
    }

    protected void setNewMoveSpeed(int newSpeed)
    {
        if (canMove)
        {
            baseMoveSpeed = newSpeed;
            agent.speed = newSpeed;
        }
        else
            Debug.Log("Tried to set new move speed for " + gameObject + ", but it is set as not being allowed to move.");
    }

    protected void incrementMoveSpeed(int speedModifier)
    {
        if (canMove)
            agent.speed += speedModifier;
        else
            Debug.Log("Tried to increment move speed for " + gameObject + ", but it is set as not being allowed to move.");
    }

    public void applyPushback(Vector3 direction)
    {
        // TODO
    }
}
