using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEditor;
using System.Linq;
using UnityEngine.UI;

public class TheCartoonKing : MonoBehaviour, IDamage
{
    //The King Model:
    [Header("Model")]
    [SerializeField] Renderer model;
    //The nav mesh agent:
    [Header("NavMesh Agent")]
    [SerializeField] NavMeshAgent agent;
    //The POV of the enemy:
    [Header("POV of King")]
    [SerializeField] Transform POV;
    [SerializeField] Animator animator;
    //All the projectiles and game objects used for damaging or knocking back the player:
    [Header("Projectiles and Weapons")]
    [SerializeField] GameObject smallProjectile;
    [SerializeField] GameObject rocketProjectile;
    [SerializeField] GameObject sniperProjectile;
    [SerializeField] GameObject VLaser;
    [SerializeField] GameObject HLaser;
    //General Stats:
    [Header("General Stats")]
    [SerializeField] int Health = 200;
    [SerializeField] float baseMoveSpeed = 1.0f;
    [SerializeField] float turningSpeed = 4;
    //Combat Stats:
    [Header("Combat Stats")]
    //Default Stats:
    [Header("Default Stats")]
    [SerializeField] float strafeDist = 35;
    [SerializeField] float pelletRate;
    [SerializeField] float styleSwitchTime;
    [SerializeField] float attackRate;
    //Dash Stats:
    [Header("Dash Stats")]
    [SerializeField] float meleeReach;
    [SerializeField] int meleeDamage;
    [SerializeField] float punchSpeed;
    [SerializeField] float punchForce;
    [SerializeField] float punchMax;

    [Header("Rocket Stats")]
    [SerializeField] float rocketRate;
    [SerializeField] float rocketMax;

    [Header("Sniper Stats")]
    [SerializeField] float sniperRate;
    [SerializeField] float sniperMax;

    [Header("Laser Stats")]
    [SerializeField] float laserRate;
    [SerializeField] float laserMax;

    [Header("Audio")]
    [SerializeField] AudioSource sfxAudioSource;
    [SerializeField] AudioSource voiceAudioSource;
    [SerializeField] AudioClip[] bulletVoicelines;
    [SerializeField] AudioClip[] rocketVoicelines;
    [SerializeField] AudioClip[] sniperVoicelines;
    [SerializeField] AudioClip[] laserVoicelines;
    [SerializeField] AudioClip[] dashWindupSounds;
    [SerializeField] AudioClip[] dashAttackSounds;
    [SerializeField] AudioClip[] punchImpactSounds;
    [SerializeField] AudioClip[] hurtSounds;
    [SerializeField] AudioClip deathSound;
    [SerializeField] AudioClip entryMonologue;
    [SerializeField] AudioClip battleStartSound;

    [Header("Misc")]
    [Tooltip("How long the game will wait in seconds after the boss dies before triggering the Win State")]
    [SerializeField] int afterDeathTimer = 10;
    [SerializeField] Collider collisionBox;
    [SerializeField] Texture2D neutralFace;
    [SerializeField] Texture2D angryFace;
    [SerializeField] Texture2D hurtFace;
    [SerializeField] Texture2D deadFace;
    [SerializeField] Transform shootPos;
    public GameObject damageNumberPopup;
    [SerializeField] GameObject healthBar;
    private int maxHP;
    private GameObject healthBarInstance;
    private Image healthFill;
    [SerializeField] Vector3 healthBarOffset = new Vector3(0, 2f, 0);
    [SerializeField] Vector3 damageNumberOffset = new Vector3(0, 2.5f, 0);

    int HP;
    int HPP2;
    bool isDead = false;
    bool isEntryMono;
    float entryMonoTimer;
    float entryMonoLength;
    bool canBeDamaged = true;
    bool doRocketFX = true;
    bool doDashFX = true;
    bool doDashWindupFX = true;
    bool doLaserFX = true;
    //DICE BOOLEANS:
    bool strafediceRolled = false;

    //STYLE BOOLEANS:
    bool defaultState;
    bool dashState;
    bool rocketState;
    bool sniperState;
    bool laserState;

    //MOVEMENT BOOLEANS:
    bool strafeMode;
    bool canMove;

    //DETECTED BOOLEANS:
    bool dashLocalfound;
    bool punched;
    bool shotSniper;
    bool HLaserB;
    bool VLaserB;

    //DICE TIMERS:
    int strafediceRoll;
    int diceRoll;

    //ATTACK TIMERS:
    float switchTimer;
    float stanceTimer;
    float shootTimer;
    float chargeTimer;
    float dashTimer;

    //PLAYER VECTORS:
    Vector3 playerDir;
    Vector3 playerTarget;

    //MELEE VECTORS:
    Vector3 punchPosition;

    //COLOR:
    Color kingColor;

    //GENERAL STATS:
    float originalSpeed;
    float distance;

    //THE STATS THE ENEMY STARTS WITH:
    void Start()
    {
        foreach (Button button in GameManager.instance.respawnButtons)
        {
            button.enabled = false;
        }
        if (healthBar != null)
        {
            healthBarInstance = Instantiate(healthBar, transform);
            healthBarInstance.transform.localPosition = healthBarOffset;
            healthFill = healthBarInstance.transform.Find("Background/Fill").GetComponent<Image>();

            healthBarInstance.SetActive(false);
        }
        //I will figure this out later, but I assume I will make the boss wait for the player to get ready
        defaultState = true;
        SetNeutral();
        HLaserB = true;
        originalSpeed = baseMoveSpeed;
        kingColor = model.material.color;
        HP = Health;
        maxHP = HP;
        HPP2 = HP / 2;
        agent.speed = originalSpeed;

        if (entryMonologue != null)
        {
            voiceAudioSource.PlayOneShot(entryMonologue);
            entryMonoLength = entryMonologue.length;
            isEntryMono = true;
            entryMonoTimer = 0;
            canBeDamaged = false;
            model.material.color = Color.darkSlateGray;
        }
    }

    //UPDATE:
    //Where all the styles and 
    void Update()
    {
        if (isEntryMono)
        {
            entryMonoTimer += Time.deltaTime;
            if (entryMonoTimer > entryMonoLength)
            {
                isEntryMono = false;
                canBeDamaged = true;
                model.material.color = kingColor;
                if (battleStartSound != null)
                    sfxAudioSource.PlayOneShot(battleStartSound);
            }
        }

        if (!isDead && !isEntryMono)
        {
            playerDir = GameManager.instance.player.transform.position - transform.position;
            playerTarget = GameManager.instance.player.transform.position;

            //STYLE SWITCH TIMER:
            //Once the timer is more or equal to the switch time you set.
            if (switchTimer >= styleSwitchTime)
            {
                defaultState = false;
                diceRollCheck();
                switchTimer = 0;
            }

            //DEFAULT STYLE:
            //The Cartoon King will begin to walk toward the player. He will always shoot rapidly toward the player. If he gets too close, he will begin strafing around you.
            if (defaultState == true)
            {
                faceTarget();
                movementCheck();
                switchTimer += Time.deltaTime;
                shootTimer += Time.deltaTime;

                if (shootTimer >= pelletRate)
                    shootPellets();

                if (canMove == true)
                {
                    agent.SetDestination(playerTarget);
                }

                if (strafeMode == true)
                {
                    if (strafediceRolled == false)
                    {
                        strafediceRoll = Random.Range(1, 3);
                        strafediceRolled = true;
                    }

                    if (strafediceRoll == 1)
                    {
                        strafeRight();
                    }
                    else if (strafediceRoll == 2)
                    {
                        strafeLeft();
                    }

                }
            }

            //DASH STYLE NOTES:
            if (dashState == true)
            {
                agent.updateRotation = false;
                agent.isStopped = true;

                if (!dashLocalfound && chargeTimer == 0)
                    animator.SetTrigger("Dash");

                model.material.color = Color.blue;
                agent.SetDestination(transform.position);
                chargeTimer += Time.deltaTime;

                if (doDashWindupFX)
                {
                    if (dashWindupSounds.Count() > 0)
                        voiceAudioSource.PlayOneShot(dashWindupSounds[Random.Range(0, dashWindupSounds.Length)]);

                    doDashWindupFX = false;
                }

                // when charge complete, lock in a dash target
                if (chargeTimer >= punchMax && dashLocalfound == false)
                {
                    // try to find a valid dash target; if we fail, cancel dash and return to default
                    if (savePlayerposition())
                    {
                        dashLocalfound = true;
                    }
                    else
                    {
                        dashTimer = 0;
                        chargeTimer = 0;
                        model.material.color = kingColor;
                        punched = false;
                        defaultState = true;
                        SetNeutral();
                        doDashFX = true;
                        doDashWindupFX = true;
                        dashLocalfound = false;
                        dashState = false;
                        agent.updateRotation = true;
                        agent.isStopped = false;
                        return;
                    }
                }

                if (dashLocalfound == true)
                {
                    if (dashAttackSounds.Count() > 0 && doDashFX)
                    {
                        animator.SetTrigger("ExecuteDash");
                        sfxAudioSource.PlayOneShot(dashAttackSounds[Random.Range(0, dashAttackSounds.Length)]);
                        doDashFX = false;
                    }

                    transform.position = Vector3.Lerp(transform.position, punchPosition, Time.deltaTime * punchSpeed);
                    punch(punchForce, (GameManager.instance.player.transform.position - transform.position));
                    dashTimer += Time.deltaTime;
                }

                // dash end conditions
                if (Vector3.Distance(transform.position, punchPosition) <= 0.15f || punched == true || dashTimer >= punchSpeed)
                {
                    dashTimer = 0;
                    chargeTimer = 0;
                    model.material.color = kingColor;
                    punched = false;
                    defaultState = true;
                    SetNeutral();
                    doDashFX = true;
                    doDashWindupFX = true;
                    dashLocalfound = false;
                    dashState = false;
                    agent.updateRotation = true;
                    agent.isStopped = false;
                }

            }

            //ROCKET STANCE NOTES:
            //The King will stand still and shoot rockets at you
            if (rocketState)
            {
                if (rocketVoicelines.Count() > 0 && doRocketFX)
                {
                    animator.SetTrigger("Fire Rockets");
                    voiceAudioSource.PlayOneShot(rocketVoicelines[Random.Range(0, rocketVoicelines.Length)]);
                    doRocketFX = false;
                }

                faceTarget();
                model.material.color = Color.orangeRed;
                agent.SetDestination(transform.position);
                chargeTimer += Time.deltaTime;

                if (chargeTimer >= attackRate)
                {
                    shootTimer += Time.deltaTime;
                    stanceTimer += Time.deltaTime;
                    if (shootTimer >= rocketRate)
                    {
                        shootRockets();
                    }

                    if (stanceTimer >= rocketMax)
                    {
                        chargeTimer = 0;
                        stanceTimer = 0;
                        shootTimer = 0;
                        model.material.color = kingColor;
                        rocketState = false;
                        doRocketFX = true;
                        defaultState = true;
                        SetNeutral();
                    }
                }
            }

            //SNIPER STYLE NOTE:
            //The King will stand still and take a shot. Be quick or you'll die immediatly.
            if (sniperState)
            {
                faceTarget();
                model.material.color = Color.purple;
                agent.SetDestination(transform.position);
                chargeTimer += Time.deltaTime;

                if (chargeTimer >= sniperMax)
                {

                    if (shotSniper == false)
                        shootSniper();

                    if (shotSniper == true)
                    {
                        chargeTimer = 0;
                        shootTimer = 0;
                        model.material.color = kingColor;
                        sniperState = false;
                        shotSniper = false;
                        defaultState = true;
                        SetNeutral();
                    }
                }
            }

            //LASER STATE:
            if (laserState)
            {
                if (laserVoicelines.Count() > 0 && doLaserFX)
                {
                    voiceAudioSource.PlayOneShot(laserVoicelines[Random.Range(0, laserVoicelines.Length)]);
                    doLaserFX = false;
                }

                faceTarget();
                model.material.color = Color.black;
                agent.SetDestination(transform.position);
                chargeTimer += Time.deltaTime;

                if (chargeTimer >= attackRate)
                {
                    shootTimer += Time.deltaTime;
                    stanceTimer += Time.deltaTime;
                    if (shootTimer >= laserRate)
                        shootLaser();

                    if (stanceTimer >= laserMax)
                    {
                        chargeTimer = 0;
                        stanceTimer = 0;
                        shootTimer = 0;
                        model.material.color = kingColor;
                        doLaserFX = true;
                        laserState = false;
                        defaultState = true;
                        SetNeutral();
                    }
                }
            }
        }
    }

    //DICE ROLL FUCNTION NOTES:
    void diceRollCheck()
    {
        if (HP <= HPP2)
        {
            diceRoll = Random.Range(1, 5);
        }
        else
        {
            diceRoll = Random.Range(1, 3);
        }

        if (diceRoll == 1)
        {
            dashState = true;
            SetAngry();
        }

        if (diceRoll == 2)
        {
            rocketState = true;
            SetAngry();
        }

        if (diceRoll == 3)
        {
            sniperState = true;
        }

        if (diceRoll == 4)
        {
            laserState = true;
        }
    }

    //MOVEMENT CHECK FUNCTIONS NOTES:
    //void movementCheck() sees if the king is too close to the player
    void movementCheck() //checks if the current state calls for certain movement
    {
        //If the distance between player and enemy is close, strafe. Otherwise, chase.
        distance = Vector3.Distance(GameManager.instance.player.transform.position, transform.position);
        if (distance <= strafeDist)
        {
            strafeMode = true;
            canMove = false;
        }
        else
        {
            strafediceRolled = false;
            strafeMode = false;
            canMove = true;
        }

    }

    //FACE TARGET FUNCTIONS NOTES:
    //void faceTarget() makes the enemy always face the player.
    void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, 0.0f, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * turningSpeed);
    }

    //STRAFE RIGHT FUNCTION NOTES:
    void strafeRight()
    {
        Vector3 strafeRightDir = Vector3.Cross(playerDir.normalized, Vector3.up);
        agent.SetDestination(GameManager.instance.player.transform.position + strafeRightDir * 30);
    }

    //STRAFE LEFT FUNCTION NOTES:
    void strafeLeft()
    {
        Vector3 strafeLeftDir = Vector3.Cross(playerDir.normalized, Vector3.up);
        agent.SetDestination(GameManager.instance.player.transform.position - strafeLeftDir * 30);
    }

    //SHOOT PELLETS FUNCTION NOTES:
    void shootPellets()
    {
        shootTimer = 0;
        Instantiate(smallProjectile, shootPos.position, transform.rotation);
    }

    void shootRockets()
    {
        shootTimer = 0;
        Instantiate(rocketProjectile, shootPos.position, transform.rotation);
    }

    void shootSniper()
    {
        shootTimer = 0;
        if (sniperVoicelines.Count() > 0)
            voiceAudioSource.PlayOneShot(sniperVoicelines[Random.Range(0, sniperVoicelines.Length)]);
        Instantiate(sniperProjectile, shootPos.position, transform.rotation);
        shotSniper = true;
    }

    void shootLaser()
    {
        shootTimer = 0;

        if (HLaserB == true)
        {
            Instantiate(HLaser, shootPos.position, transform.rotation);
            VLaserB = true;
            HLaserB = false;
            return;
        }
        if (VLaserB == true)
        {
            Instantiate(VLaser, shootPos.position, transform.rotation);
            HLaserB = true;
            VLaserB = false;
            return;
        }
    }

    //PUNCH FUNCTION NOTES:
    void punch(float Force, Vector3 dir)
    {
        dir = dir.normalized;
        Vector3 totalPunch = dir * Force;

        if (Vector3.Distance(GameManager.instance.player.transform.position, transform.position) <= meleeReach)
        {
            if (punchImpactSounds.Count() > 0)
                sfxAudioSource.PlayOneShot(punchImpactSounds[Random.Range(0, punchImpactSounds.Length)]);

            GameManager.instance.playerScript.Knockbacked = true;
            GameManager.instance.playerScript.applyPushback(totalPunch);
            GameManager.instance.playerScript.takeDamage(meleeDamage);
            punched = true;
        }
    }

    // returns true if a valid dash target was found
    bool savePlayerposition()
    {
        // base desired dash target = player's XZ, King's Y
        Vector3 desired = new Vector3(
            GameManager.instance.player.transform.position.x,
            transform.position.y,
            GameManager.instance.player.transform.position.z);

        NavMeshHit hit;

        // Try to snap to nearest valid navmesh point around the player
        if (NavMesh.SamplePosition(desired, out hit, 3f, NavMesh.AllAreas))
        {
            punchPosition = hit.position;
        }
        else
        {
            // Try a midpoint between King and player if player point is invalid
            Vector3 midpoint = transform.position +
                (desired - transform.position).normalized * 3f;

            if (NavMesh.SamplePosition(midpoint, out hit, 3f, NavMesh.AllAreas))
                punchPosition = hit.position;
            else
                return false; // no valid dash target found
        }

        // Slight inward nudge to keep off the exact navmesh border
        Vector3 inward = punchPosition - transform.position;
        inward.y = 0;

        if (inward.sqrMagnitude > 0.0001f)
            punchPosition -= inward.normalized * 0.25f;

        // If target is basically our current position, treat as invalid
        if (Vector3.Distance(punchPosition, transform.position) < 0.1f)
            return false;

        return true;
    }

    public void takeDamage(int amount)
    {
        if (!canBeDamaged) return;

        Vector3 spawnPos = transform.position + damageNumberOffset;
        GameObject dmg = Instantiate(damageNumberPopup, spawnPos, Quaternion.identity);
        dmg.GetComponent<DamageNumber>().Initialize(amount);

        if (!healthBarInstance.activeSelf)
            healthBarInstance.SetActive(true);
        HP -= amount;
        float pct = (float)HP / maxHP;
        healthFill.fillAmount = pct;
        animator.SetTrigger("Hurt");
        StartCoroutine(flashRed());
        StartCoroutine(GetHurt());

        if (HP > 0 && hurtSounds.Count() > 0)
            voiceAudioSource.PlayOneShot(hurtSounds[Random.Range(0, hurtSounds.Length)]);

        if (HP <= 0)
        {
            isDead = true;
            defaultState = false;
            if (deathSound != null)
            {
                voiceAudioSource.Stop();
                sfxAudioSource.Stop();
                voiceAudioSource.PlayOneShot(deathSound);
            }

            if (collisionBox != null)
            {
                collisionBox.enabled = false;
            }
            agent.isStopped = true;
            agent.enabled = false;
            if (healthBarInstance != null)
                Destroy(healthBarInstance);
            SetDead();
            animator.SetBool("isDead", true);
            StartCoroutine(GameEndCountdownTimer(afterDeathTimer));
        }
    }

    IEnumerator GameEndCountdownTimer(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        EventController.RaiseGameComplete();
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = kingColor;
    }

    public void SetNeutral() => model.material.SetTexture("_BaseMap", neutralFace);
    public void SetAngry() => model.material.SetTexture("_BaseMap", angryFace);
    public void SetDead() => model.material.SetTexture("_BaseMap", deadFace);

    IEnumerator GetHurt()
    {
        model.material.SetTexture("_BaseMap", hurtFace);
        yield return new WaitForSeconds(0.458f);
        if (defaultState)
            SetNeutral();
        else if (isDead)
            SetDead();
        else
            SetAngry();
    }
}


//CODING JOURNAL:
//I will create a series of bools that give the enemy a variety of states and a check similiar to FNAF. Once the dice has been rolled, it will choose to atttack that way.
//Afterwards, it will wait for a while. While it is waiting, it will go back shooting projectiles and strafing.
//Few seconds later, he will roll his dice again.
//First, try to apply this to dash and rockets. The object will flash or animate to a certain pose before attack.
//This has to be distinct, and it has to work.
//It should not be perfect, perfect is the enemy of good.
//Research, then come back.

//UPDATE (5:03 PM, 11/6/2025):
//Things are going well. The strafe dice and the attacks I have implemented work as expected.
//Now I am working on style switching.
//See if I can organize this even further and get a better grasp at making boss battles.

//UPDATE (9:50 AM, 11/8/2025):
//A dash has been implemented. It took a lot of time because of bools not working as expected.
//Not sure if restarting  or redoing my code fixed it, but its fixed.
//Now on to rockets, lazers, and ground pound. I assume ground pound will be the hardest.
//Afterwards, work on your level's format, then worry about enemy placement in the beta.
//Make sure everything is up to code.

