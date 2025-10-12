using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour, IDamage, IPushback
{
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreShottingLayer;

    [Header("General")]
    [SerializeField] int HP; // 10 (Could be changed)

    [Header("Jumping")]
    [SerializeField] int jumpSpeed; // 12
    [SerializeField] int jumpMaxCount; // 2
    [SerializeField] float jumpGracePeriod; // 0.175


    [Header("Shooting")]
    [SerializeField] int shootDmg; // 1 (Could be changed)
    [SerializeField] int shootDist; // 15 (Could be changed)
    [SerializeField] float shootRate; // 0.5 (Could be changed)

    [Header("Climbing")]
    [SerializeField] float climbSpeed; // 10.25
    [SerializeField] int climbingDist; // 0
    [SerializeField] float climbWallDetection; // 1.25
    [SerializeField] float climbMaxAngle; // 90

    [Header("RagDoll")]
    [SerializeField] float ragdollPerSpeed = 0.06f;  // seconds of control lockout per 1 m/s moved during ragdoll
    [SerializeField] float minRagdollTime = 0.15f;

    [Header("Movement")]
    [SerializeField] float MinSpeed; //3 //what your speed starts at from 0
    [SerializeField] float MaxSpeed; // 12

    [Header("Accel")]
    [SerializeField] float MinAccel;
    [SerializeField] float MaxAccel;
    [SerializeField] float speedAccelRATE;//how fast the Aceel ramps up
    [SerializeField] float MinDeaccel;
    [SerializeField] float MaxDeaccel;
    [SerializeField] float speedDeaccelRATE; //how fast the Deaceel ramps up

    [Header("Dash")]
    [SerializeField] float DashSpeed; //
    [SerializeField] float DashLength; //how long dash last
    [SerializeField] float Dashcd;

    [Header("Forces")]
    [SerializeField] int gravity; // 35 // down force


    [Header("Bools")]
    [SerializeField] bool dashcarryover; //Best false //used to see if you want your dash speed to carry over into your current speed
    [SerializeField] bool Knockback_only; //used to see if the player momentum has any input on the knockback. True = when knocked backed player momentum isn't considered.

    //"INTERNAL FILEDS //Used in the background for various things

    //Vectors for player
    Vector3 moveDirec; //inputted direction from player
    Vector3 playerVel; // used for jump and also holds pushback y
    Vector3 pushBack; //force applied to player
    Vector3 MomentumDir; //last input direction is used
    Vector3 Knockback; //used to hold pushback's x and z

    //RaycastHit
    RaycastHit hit;

    //Ints
    int jumpCount;
    int hpOrig;
    //Floats
    float knockbacktimer; //uesd to know when to start losing knockback
    float jumpTimer;
    float shootTimer;
    public float ragdollTimeLeft;
    float currDashSpeed;
    float currspeedAccel;
    float currspeedDeaccel;
    float currentSpeed;
    float Dashcdtimer;//used to track dash cd.
    float Dashtimer; //used to track how long dash will go.
    float speedDeaccelOrig;
    float speedAccelOrig;
    float Speed0 = 0;
    //Bools
    bool is_dashing;
    bool isClimbing;
    bool isJumping;
    public bool isInRagdoll = false;
    public bool knockbacked = false;

    // - UNUSED -
    //[SerializeField] Collider slopecheck; // no use yet
    // bool _onSlope; //use will be added later
    //Debug.DrawRay(GameManager.instance.player.transform.position, camdown * shootDist, Color.blue); //ray that looks at the floor
    //if (controller.isGrounded && _onSlope == true){}// possible slope code
    //bool onslope() //more possible slope code
    //{bool check = false;RaycastHit slopehit;if (Physics.Raycast(player.transform.position, camdown * shootDist, out slopehit, slopecheck.GetComponent<>))
    // {check = true;}return check;}   
    //float AirTime; // might have a future use
    // void AirCheck() {if (controller.isGrounded){ AirTime = 0;}} //possible airtime code

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hpOrig = HP;
        currentSpeed = Speed0;
        currspeedAccel = MinAccel;
        currspeedDeaccel = MinDeaccel;
        speedDeaccelOrig = currspeedDeaccel;
        speedAccelOrig = currspeedAccel;
        is_dashing = false;
    }

    // Update is called once per frame
    void Update()
    {
        Timers();
        Shoot();
        PlayerMovement();
    }

    void PlayerMovement()
    {

        climb();
        dash();
        DashEnd();
        Movement();
        jump();
        KnockbackMovement();
        controller.Move(playerVel * Time.deltaTime); //used here to apply gravity correctly
        Gravity();
    }

    void climb()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * climbWallDetection, Color.blue);
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, climbWallDetection, ~ignoreShottingLayer) && is_dashing == false)
        {
            int wallAngle = (int)Vector3.Angle(hit.normal, Vector3.up);
            if (hit.collider.CompareTag("CanClimb") && controller.slopeLimit <= wallAngle && wallAngle <= climbMaxAngle && !controller.isGrounded && Input.GetButton("Jump"))
            {
                playerVel.y = climbSpeed;
                isClimbing = true;
            }
        }
        else isClimbing = false;
    }
    void jump()
    {
        if (!isClimbing)
        {
            if (Input.GetButtonDown("Jump") && jumpCount < jumpMaxCount)
            {
                if (jumpTimer >= jumpGracePeriod && jumpCount == 0)
                {
                    ++jumpCount;
                }
                isJumping = true;
            }
        }
    }

    void ShootApplyDamage()
    {
        shootTimer = 0;

        // ~ignoreLayer will ignore the player later to prevent the player shooting themselves.
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist, ~ignoreShottingLayer))
        {
            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null)
            {
                dmg.takeDamage(shootDmg);
            }
            Debug.Log(hit.collider.name); // logs info to the debug status bar.
        }
    }

    public void takeDamage(int amount)
    {
        HP -= amount;

        if (HP <= 0)
        {
            //GameManager.instance.youDied();  RE ADD THIS!!!!!
        }
    }

    // getters
    public float RagdollPerSpeed() { return ragdollPerSpeed; }
    public float MinRagdollTime() { return minRagdollTime; }
    void Timers()
    {
        shootTimer += Time.deltaTime;
        Dashcdtimer += Time.deltaTime;
        RagdollTimer();
        knockbacktimer += Time.deltaTime;
        //jump timer for wwhen in the air.
        if (!controller.isGrounded) jumpTimer += Time.deltaTime;
        else { jumpTimer = 0; }
    }
    void Shoot()
    {
        if (Input.GetButton("Fire1") && shootTimer >= shootRate)
            ShootApplyDamage();
    }
    void Movement()
    {
        if (is_dashing == false && isInRagdoll == false && isClimbing == false)
        {
            Movementincrementation();
            Movelike();
        }
    }
    void Movementincrementation()
    {
        if (currentSpeed >= MaxSpeed && (Input.GetButton("Horizontal") == true || Input.GetButton("Vertical") == true)) // Uncomment this block of code to prevent the player from stopping immediately after reaching max speed.
        {
            if (currentSpeed > MaxSpeed && Input.GetButton("Shift") == false)
            {
                if (currentSpeed > MaxSpeed + 1)
                {
                    currentSpeed -= currspeedDeaccel * Time.deltaTime;
                    Ramp();
                }
            }
            moveDirec = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
            MomentumDir = moveDirec;
        }
        else if ((Input.GetButton("Horizontal") == true || Input.GetButton("Vertical") == true) && currentSpeed < MaxSpeed)
        {
            if (currentSpeed < MinSpeed)
            {
                currentSpeed = MinSpeed;
            }
            moveDirec = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
            MomentumDir = moveDirec;
            currentSpeed += currspeedAccel * Time.deltaTime;
            Ramp();
        }
        else if ((Input.GetButton("Horizontal") == false && Input.GetButton("Vertical") == false) && currentSpeed > Speed0)
        {
            currentSpeed -= currspeedDeaccel * Time.deltaTime;
            Ramp();
            if (currentSpeed < 0)
            {
                currentSpeed = 0;
            }
        }
    }
    void Movelike()//main way the player moves
    {
        if ((Input.GetButton("Horizontal") == true || Input.GetButton("Vertical") == true) && (currentSpeed < MaxSpeed || currentSpeed >= MaxSpeed))
        {
            controller.Move((moveDirec + Knockback) * currentSpeed * Time.deltaTime);
        }
        else if ((Input.GetButton("Horizontal") == false && Input.GetButton("Vertical") == false))
        {
            controller.Move((MomentumDir + Knockback) * currentSpeed * Time.deltaTime);
        }

    }
    void Ramp() //used to make Accel and Deacell higher over time.
    {
        if ((Input.GetButton("Horizontal") == false && Input.GetButton("Vertical") == false) && currentSpeed < MaxSpeed)
        {
            currspeedAccel = speedAccelOrig;
            if (currentSpeed == Speed0)
            {
                currspeedDeaccel = speedDeaccelOrig;
            }
            else if (currspeedDeaccel < MaxDeaccel)
            {
                currspeedDeaccel += speedDeaccelRATE * Time.deltaTime;

            }
        }
        else if ((Input.GetButton("Horizontal") == true || Input.GetButton("Vertical") == true) && currentSpeed > Speed0)
        {
            currspeedDeaccel = speedDeaccelOrig;
            if (currentSpeed >= MaxSpeed)
            {
                currspeedAccel = MaxAccel;
            }
            else if (currspeedAccel < MaxAccel)
            {
                currspeedAccel += speedAccelRATE * Time.deltaTime;
            }
        }
    }
    void dash() //dash in a direction but has bools for how you want to specifically dash.
    {

        if (Input.GetButton("Shift") && (Input.GetButton("Horizontal") == true || Input.GetButton("Vertical") == true) && isClimbing == false && isInRagdoll == false)
        {
            is_dashing = true;
            if (Dashcdtimer >= Dashcd)
            {
                Dashcdtimer = 0;
                Dashtimer = 0;
                currDashSpeed = DashSpeed;
                StartCoroutine(dashwait(MomentumDir * currDashSpeed * Time.deltaTime));
            }
        }
    }
    void DashMomentum() //makes the dash add to the player's speed
    {
        if (dashcarryover)
        {
            if (currentSpeed < MaxSpeed)
            {
                if (currentSpeed + currDashSpeed > MaxSpeed)
                {
                    currentSpeed = MaxSpeed;
                }
                else
                {
                    currentSpeed += currDashSpeed;
                }
            }
        }
        currDashSpeed = 0;
        is_dashing = false;
    }
    void DashEnd()// ends dash
    {
        if (Dashtimer >= DashLength || knockbacked)
        {
            StopCoroutine(dashwait(MomentumDir * currDashSpeed * Time.deltaTime));
            DashMomentum();
        }
    }
    IEnumerator dashwait(Vector3 move) //used to make dash function
    {
        Dashtimer = 0;
        while (Dashtimer < DashLength)
        {
            Dashtimer += Time.deltaTime;
            controller.Move(move);
            yield return null;
        }
    }

    void Gravity()// made gravity a method for easier use 
    {
        if (is_dashing == true)
        {
            Debug.Log("Dashing or Jumping");
            playerVel.y = 0;
        }
        else if (isJumping == true)
        {
            jumpCount++;
            playerVel.y = jumpSpeed;
            isJumping = false;
        }
        else if (controller.isGrounded)
        {
            Debug.Log("On Floor");
            playerVel.y = -(0.001f);
            jumpCount = 0;
        }
        else if (is_dashing == false && isClimbing == false)
        {
            Debug.Log("Not Floor");
            playerVel.y -= gravity * Time.deltaTime;
        }
        controller.Move(playerVel * Time.deltaTime);
    }
    public void applyPushback(Vector3 direction)
    {
        pushBack = direction;
    }
    void KnockbackMovement()
    {
        //appying forces when hit
        if (isInRagdoll && knockbacked)
        {
            MovementResetFULL();
            playerVel.y = pushBack.y;
            Knockback.z = pushBack.z;
            Knockback.x = pushBack.x;
            knockbacktimer = 0;
            knockbacked = false;
            pushBack = Vector3.zero;
        }
        else if (knockbacked)
        {
            playerVel.y = pushBack.y;
            Knockback.z = pushBack.z;
            Knockback.x = pushBack.x;
            knockbacktimer = 0;
            knockbacked = false;
            pushBack = Vector3.zero;
        }
        //how to move the player based on your bool and if you're in ragdoll right now
        if (Knockback_only && isInRagdoll)
        {
            controller.Move((Knockback) * Time.deltaTime); // knock back only
        }
        else if (isInRagdoll)
        {
            controller.Move((MomentumDir + Knockback) * Time.deltaTime); // knock back is added to the last known input
        }
        //mkaing knockback decrease
        if (Knockback.z > 0 && knockbacktimer > 0.001f)
        {
            Knockback.z -= gravity * Time.deltaTime;
            currentSpeed = 1;
        }
        else if (Knockback.z <= 0)
        {
            Knockback.z = 0;
        }
        if (Knockback.x > 0 && knockbacktimer > 0.001f)
        {
            Knockback.x -= gravity * Time.deltaTime;
            currentSpeed = 1;
        }
        else if (Knockback.x <= 0)
        {
            Knockback.x = 0;
        }
    }
    void RagdollTimer()
    {
        if (controller.isGrounded && ragdollTimeLeft > 0f)
            ragdollTimeLeft = Mathf.Max(0f, ragdollTimeLeft - Time.deltaTime * 2f); // Recover twice as fast from ragdoll if you are on the ground
        else
            ragdollTimeLeft = Mathf.Max(0f, ragdollTimeLeft - Time.deltaTime);
        if (ragdollTimeLeft <= 0f)
        {
            isInRagdoll = false;
        }
        else
        {
            isInRagdoll = true;
        }
    }
    void MovementResetFULL() //used to reset player Movement values
    {
        currentSpeed = Speed0;
        currspeedAccel = MinAccel;
        currspeedDeaccel = MinDeaccel;
        speedDeaccelOrig = currspeedDeaccel;
        speedAccelOrig = currspeedAccel;
    }

}

//Gold's pile of possible features
//add a new layer for dashing, this layer ignores enemy projectiles and would effectly make you invincible based on what it ignores


