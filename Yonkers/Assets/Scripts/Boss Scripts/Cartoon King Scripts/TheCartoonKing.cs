
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEditor;

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
    //All the projectiles and game objects used for damaging or knocking back the player:
    [Header("Projectiles and Weapons")]
    [SerializeField] GameObject smallProjectile;
    [SerializeField] GameObject rocketProjectile;
    [SerializeField] GameObject sniperProjectile;
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


    int HP;
    int HPP2;
    //DICE BOOLEANS:
    bool strafediceRolled = false;

    //STYLE BOOLEANS:
    bool defaultState;
    bool dashState;
    bool rocketState;
    bool sniperState;

    //MOVEMENT BOOLEANS:
    bool strafeMode;
    bool canMove;

    //DETECTED BOOLEANS:
    bool dashLocalfound;
    bool punched;
    bool shotSniper;

    //DICE TIMERS:
    int strafediceRoll;
    int diceRoll;


    //ATTACK TIMERS:
    float switchTimer;
    float stanceTimer;
    float shootTimer;
    float chargeTimer;

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
        //I will figure this out later, but I assume I will make the boss wait for the player to get ready
        defaultState = true;
        originalSpeed = baseMoveSpeed;
        kingColor = model.material.color;
        HP = Health;
        HPP2 = HP / 2;
        agent.speed = originalSpeed;
    }

    //UPDATE:
    //Where all the styles and 
    void Update()
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

            if(shootTimer >= pelletRate)
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
            
            model.material.color = Color.blue;
            agent.SetDestination(transform.position);
            chargeTimer += Time.deltaTime;

            if (chargeTimer >= punchMax && dashLocalfound == false)
            {
                
                savePlayerposition();
                dashLocalfound = true;

            }
            if (dashLocalfound == true)
            {
                agent.SetDestination(punchPosition);
                transform.position =
                Vector3.Lerp(transform.position, punchPosition, Time.deltaTime * punchSpeed);
                punch(punchForce, (GameManager.instance.player.transform.position - transform.position));
            }

            if (transform.position == Vector3.Lerp(transform.position, punchPosition, Time.deltaTime * punchSpeed) || punched == true)
            {
                chargeTimer = 0;
                model.material.color = kingColor;
                punched = false;
                defaultState = true;
                dashLocalfound = false;
                dashState = false;

            }

        }

        //ROCKET STANCE NOTES:
        //The King will stand still and shoot rockets at you
        if (rocketState)
        {
            faceTarget();
            model.material.color = Color.orangeRed;
            agent.SetDestination(transform.position);
            chargeTimer += Time.deltaTime;


            if (chargeTimer >= attackRate)
            {
                shootTimer += Time.deltaTime;
                stanceTimer += Time.deltaTime;
                if(shootTimer >= rocketRate)
                shootRockets();

                if (stanceTimer >= rocketMax)
                {
                    chargeTimer = 0;
                    stanceTimer = 0;
                    shootTimer = 0;
                    model.material.color = kingColor;
                    rocketState = false;
                    defaultState = true;
                }
            }
        }

        //SNIPER STYLE NOTE:
        //The King will stand still and take a shot. Be quick or you'll die immediatly.
        if(sniperState)
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
                }
            }
        }

    }

    


    //DICE ROLL FUCNTION NOTES:
    void diceRollCheck()
    {
        if (HP <= HPP2)
        {
            diceRoll = Random.Range(1, 4);
        }
        else
        {
            diceRoll = Random.Range(1, 3);
        }


        if (diceRoll == 1)
        {
            dashState = true;
        }

        if(diceRoll == 2)
        {
            rocketState = true;
        }

        if(diceRoll == 3)
        {
            sniperState = true;
        }
    }

    
    
    //MOVEMENT CHECK FUNCTIONS NOTES:
    //void movementCheck() sees if the king is too close to the player
    void movementCheck() //checks if the current state calls for certain movement
    {
        //If the distance between player and enemy is close, strafe. Otherwise, chase.
        distance = Vector3.Distance(GameManager.instance.player.transform.position, transform.position);
        if ( distance <= strafeDist)
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
    //void strafeRight() function allows the king to circle around the player to right. It will also move closer and closer to the player.
    void strafeRight()
    {
        Vector3 strafeRightDir = Vector3.Cross(playerDir.normalized, Vector3.up);
        agent.SetDestination(GameManager.instance.player.transform.position + strafeRightDir * 30);
    }

    //STRAFE LEFT FUNCTION NOTES:
    //void strafeLeft() function allows the king to circle around the player to left. It will also move closer and closer to the player.
    void strafeLeft()
    {
        Vector3 strafeLeftDir = Vector3.Cross(playerDir.normalized, Vector3.up);
        agent.SetDestination(GameManager.instance.player.transform.position - strafeLeftDir * 30);
    }

    //SHOOT PELLETS FUNCTION NOTES:
    //void shootPellets() instantiates the pellet
    void shootPellets()
    {
        shootTimer = 0;
        Vector3 shootposition = new Vector3(POV.position.x, GameManager.instance.player.transform.position.y, POV.position.z);
        Instantiate(smallProjectile, shootposition, transform.rotation);
    }

    void shootRockets()
    {
        shootTimer = 0;
        Vector3 shootposition = new Vector3(POV.position.x, GameManager.instance.player.transform.position.y, POV.position.z);
        Instantiate(rocketProjectile, shootposition, transform.rotation);
    }

    void shootSniper()
    {
        shootTimer = 0;
        Vector3 shootposition = new Vector3(POV.position.x, GameManager.instance.player.transform.position.y, POV.position.z);
        Instantiate(sniperProjectile, shootposition, transform.rotation);
        shotSniper = true;
    }

    //PUNCH FUNCTION NOTES:
    //void punch(float Force, Vector3 dir) takes the direction and force to knock back the player. If the enemy gets to close during his punch phase, he will damage and knockback the player.
    void punch(float Force, Vector3 dir)
    {

        dir = dir.normalized;
        Vector3 totalPunch = dir * Force;

        if (Vector3.Distance(GameManager.instance.player.transform.position, transform.position) <= meleeReach)
        {
            Debug.Log("Ouch!!!");
            GameManager.instance.playerScript.Knockbacked = true;
            GameManager.instance.playerScript.applyPushback(totalPunch);
            GameManager.instance.playerScript.takeDamage(meleeDamage);
            punched = true;
        }
    }
    //SAVE PLAYER POSITION FUNCTION NOTES:
    //void savePlayerposition() make the enemy scan the players position and saves it in punchPosition while creating a punch direction. This function is mainly used for
    //scanning the player's position once to help the enemy dash.
    void savePlayerposition()
    {
        punchPosition = new Vector3(GameManager.instance.player.transform.position.x, transform.position.y, GameManager.instance.player.transform.position.z);
        Debug.Log("Player Dectected");

    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        StartCoroutine(flashRed());

        if(HP <= 0)
        {
            EventController.RaiseGameComplete();
            Destroy(gameObject);
        }
            
        
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = kingColor;
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

