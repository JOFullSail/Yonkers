using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour, IDamage, IPushback, IPickup
{
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreShooting;
    [SerializeField] LayerMask ignoreClimbing;

    [Header("General")]
    [SerializeField] int HP = 10;
    //[SerializeField] int speed = 12; // Left here since the use of the movement speed have not been decided.

    [Header("Jumping")]
    [SerializeField] int jumpSpeed = 12;
    [SerializeField] int jumpMaxCount = 2;
    [Tooltip("Amount of time (in seconds) the player has to trigger the first jump after falling off a platform. \n" +
        "If the player jumps after the grace period, the player will only trigger the second jump.")]
    [SerializeField] float jumpGracePeriod = 0.175f;


    [Header("Shooting")] // Values could be changed
    [SerializeField] List<GunStats> gunList = new List<GunStats>();
    [SerializeField] GameObject gunModel;
    [SerializeField] int shootDmg = 1;
    [SerializeField] int shootDist = 20;
    [SerializeField] float shootRate = 0.5f;
    [SerializeField] bool weaponIsHitscan;
    [SerializeField] GameObject projectile;

    [Header("Climbing")]
    //[Tooltip("Makes climbing easier for the player.\n\n- Players will be able to continue climbing even while looking away from the wall.\n" +
    //    "- The player will automatically ledge grab when they reach the top of a wall.")]
    //// Make playerVel.y = 0 once they get to the ledge. Attempt to build system that makes the player jump over a wall and land on the surface above automatically.
    //[SerializeField] bool climbAccessability;
    [SerializeField] float climbSpeed = 10.25f;
    //[Tooltip("Amount of time the player is allowed to climb a wall.\n\n- Will be overrided once the player reaches the top of a wall.")]
    //[SerializeField] float climbingTime = 0.5f; not used yet :) // might have to be higher for taller walls.
    [Tooltip("Distance between the player and the wall required for the player to climb a wall.")]
    [SerializeField] float climbWallDetection = 1.25f;
    [Tooltip("Max angle of a wall the player can climb.")]
    [SerializeField] float climbMaxAngle = 90f;

    [Header("Speed")]
    [SerializeField] float minSpeed = 3f; // What your speed starts at from 0.
    [SerializeField] float maxSpeed = 12f;

    [Header("Acceleration")]
    [SerializeField] float minAccel;
    [SerializeField] float maxAccel;
    [SerializeField] float speedAccelRate; // How fast the Aceel ramps up.
    [SerializeField] float minDeaccel;
    [SerializeField] float maxDeaccel;
    [SerializeField] float speedDeaccelRate; // How fast the Deaceel ramps up.

    [Header("Dash")]
    [SerializeField] bool dashCarryOver; // Best false //used to see if you want your dash speed to carry over into your current speed.
    [SerializeField] float dashSpeed;
    [SerializeField] float dashLength; //How long dash lasts.
    [SerializeField] float dashCooldown;

    [Header("Forces")]
    [SerializeField] int gravity = 35;

    [Header("Ragdoll")]
    [SerializeField] bool knockbackOnly; // Used to see if the player momentum has any input on the knockback. True = when knocked backed player momentum isn't considered.
    [SerializeField] float ragdollPerSpeed = 0.06f;  // Seconds of control lockout per 1 m/s moved during ragdoll.
    [SerializeField] float minRagdollTime = 0.15f;

    [Header("Debug")]
    [Tooltip("Gives the player the ability to climb literally anything.")]
    [SerializeField] bool debugClimbAnything;
    [Tooltip("Gives the player the ability to climb at any given speed set to Debug Climb Speed.\n\n" +
        "- Gravity will not pull you down as fast with high values.")]
    [SerializeField] bool debugFastClimb;
    [Tooltip("Gives the player the ability to sprint when pressing and holding the Left Shift key. It was left unused as a design choice.\n\n" +
        "- Will replace the player's dash.\n\n- Useful for skipping levels when debugging.\n\n- Hi TetraBitGaming!")]
    [SerializeField] bool debugSprint;
    [Tooltip("Sets the player's climb speed.\n\n- Gravity will not pull you down as fast with high values.")]
    [SerializeField] float debugClimbSpeed = 50f;
    [Tooltip("Multiplier for the player's unused sprint speed.\n\n - Hi TetraBitGaming!")]
    [SerializeField] int debugSprintModifier = 10;

    RaycastHit hit;


    int gunListIdx;
    int jumpCount;
    int hpOrig;

    bool isDashing;
    bool isClimbing;
    bool isJumping;
    bool isInRagdoll;
    bool knockbacked;

    float knockbackTimer;      // Used to know when to start losing knockback.
    float jumpTimer;
    float shootTimer;
    float ragdollTimeLeft;
    float currentDashSpeed;
    float currentSpeedAccel;
    float currentSpeedDeaccel;
    float currentSpeed;
    float dashCooldownTimer;   // Used to track dash cooldown.
    float dashTimer;           // Used to track how long dash will go.
    float speedDeaccelOrig;
    float speedAccelOrig;
    float speedZero = 0.0f;

    // Vectors for the player
    Vector3 moveDirec;   // Input direction from the player.
    Vector3 playerVel;   // Used for jump and also holds pushback.y.
    Vector3 pushBack;    // Force applied to the player.
    Vector3 momentumDir; // Last input direction used.
    Vector3 knockback;   // Used to hold pushBack.x and z.
    
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

    public float RagdollTimeLeft
    {
        get { return ragdollTimeLeft; }
        set { ragdollTimeLeft = value; }
    }

    public bool IsInRagdoll
    {
        get { return isInRagdoll; }
        set {  isInRagdoll = value; }
    }

    public bool Knockbacked
    {
        get { return knockbacked; }
        set {  knockbacked = value; }
    }

    public float RagdollPerSpeed
    {
        get { return ragdollPerSpeed; }
    }

    public float MinRagdollTime
    {
        get { return minRagdollTime; }
    }

    public bool IsDashing
    {
        get { return isDashing; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (debugFastClimb) climbSpeed = debugClimbSpeed;

        hpOrig = HP;
        currentSpeed = speedZero;
        currentSpeedAccel = minAccel;
        currentSpeedDeaccel = minDeaccel;
        speedDeaccelOrig = currentSpeedDeaccel;
        speedAccelOrig = currentSpeedAccel;
        isDashing = false;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.red);
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * climbWallDetection, Color.blue);

        if(!GameManager.instance.isPaused)
        {
            timers();
            shoot();
            playerMovement();
        }
    }

    void playerMovement()
    {

        climb();

        if (debugSprint) sprint();
        else dash();

        dashEnd();
        movement();
        jump();
        knockbackMovement();
        controller.Move(playerVel * Time.deltaTime); // Used here to apply gravity correctly
        Gravity();
    }

    // FOR DEBUG PURPOSES ONLY
    void sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            currentSpeed *= debugSprintModifier;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            currentSpeed /= debugSprintModifier;
        }
    }

    void climb()
    {
        int noClimbLayers;
        string noClimbTag = "NoClimb";
        if (debugClimbAnything)
        {
            noClimbLayers = 0;
            noClimbTag = "Player";
        }
        else noClimbLayers = ignoreClimbing.value;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, climbWallDetection, ~noClimbLayers))
        {
            int wallAngle = (int)Vector3.Angle(hit.normal, Vector3.up);

            if (!hit.collider.CompareTag(noClimbTag) &&
                controller.slopeLimit <= wallAngle &&
                wallAngle <= climbMaxAngle &&
                !controller.isGrounded &&
                (playerVel.y < 0 || isClimbing))
            {
                playerVel.y = climbSpeed;
                isClimbing = true;
            }
            else isClimbing = false;
        }
        else isClimbing = false;
    }

    void jump()
    {
        if (!isClimbing && Input.GetButtonDown("Jump") && jumpCount < jumpMaxCount)
        {
            if (jumpTimer >= jumpGracePeriod && jumpCount == 0)
            {
                ++jumpCount;
            }

            isJumping = true;
        }
    }

    void shootApplyDamage()
    {
        shootTimer = 0;
        --gunList[gunListIdx].ammoCurrent;

        // ~ignoreLayer will ignore the player later to prevent the player shooting themselves.
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist, ~ignoreShooting) && weaponIsHitscan)
        {
            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null)
            {
                dmg.takeDamage(shootDmg);
            }
        }
        else if(projectile != null)
            Instantiate(projectile, Camera.main.transform.position, Camera.main.transform.rotation);
    }

    public void takeDamage(int amount)
    {
        HP -= amount;

        if (HP <= 0)
        {
            GameManager.instance.youDied();
        }
    }

    void timers()
    {
        if (!controller.isGrounded) jumpTimer += Time.deltaTime;
        else jumpTimer = 0;

        shootTimer += Time.deltaTime;
        dashCooldownTimer += Time.deltaTime;
        ragdollTimer();
        knockbackTimer += Time.deltaTime;
    }

    void shoot()
    {
        if (Input.GetButton("Fire1") && gunList.Count > 0 && gunList[gunListIdx].ammoCurrent > 0 && shootTimer >= shootRate)
        {
            shootApplyDamage();

            // For special guns
            if (gunList[gunListIdx].ammoCurrent <= 0 && gunList[gunListIdx].ammoReserves <= 0 && gunList[gunListIdx].isSpecial)
            {
                gunList.RemoveAt(gunListIdx);
                gunListIdx = 0;
                if (gunList.Count > 0)
                {
                    changeGun();
                }
                else
                {
                    gunModel.GetComponent<MeshFilter>().sharedMesh = null;
                    gunModel.GetComponent<MeshRenderer>().sharedMaterial = null;
                }

            }
        }


        reload();
        if (gunList.Count > 0) switchGun();
    }

    void reload()
    {
        if (Input.GetButtonDown("Reload") && gunList.Count > 0 && gunList[gunListIdx].ammoReserves > 0)
        {
            int ammoToLoad = gunList[gunListIdx].ammoMax <= gunList[gunListIdx].ammoReserves ? gunList[gunListIdx].ammoMax : gunList[gunListIdx].ammoReserves;
            gunList[gunListIdx].ammoReserves -= (gunList[gunListIdx].ammoMax - gunList[gunListIdx].ammoCurrent);
            gunList[gunListIdx].ammoCurrent = ammoToLoad;
        }
    }

    public void GetGunStats(GunStats gun)
    {
        gunList.Add(gun);
        gunListIdx = gunList.Count - 1;

        changeGun();
    }

    void changeGun()
    {
        shootDmg = gunList[gunListIdx].shootDamage;
        shootDist = gunList[gunListIdx].shootDist;
        shootRate = gunList[gunListIdx].shootRate;
        weaponIsHitscan = gunList[gunListIdx].isHitscan;
        projectile = gunList[gunListIdx].projectile;

        if (gunModel != null)
        {
            gunModel.transform.localScale = gunList[gunListIdx].scaleWhenHeld;
            gunModel.transform.localRotation = Quaternion.Euler(gunList[gunListIdx].rotationWhenHeld);
            gunModel.transform.localPosition = gunList[gunListIdx].positionWhenHeld;

            gunModel.GetComponent<MeshFilter>().sharedMesh = gunList[gunListIdx].gunModel.GetComponent<MeshFilter>().sharedMesh;
            gunModel.GetComponent<MeshRenderer>().sharedMaterial = gunList[gunListIdx].gunModel.GetComponent<MeshRenderer>().sharedMaterial;
        }
    }

    void switchGun()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (gunListIdx < gunList.Count - 1)
                ++gunListIdx;
            else
                gunListIdx = 0;
            changeGun();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (gunListIdx > 0)
                --gunListIdx;
            else
                gunListIdx = gunList.Count - 1;
            changeGun();
        }
    }

    void movement()
    {
        if (isDashing == false && isInRagdoll == false)
        {
            movementIncrementation();
            moveLike();
        }
    }

    void movementIncrementation()
    {
        // Uncomment this block of code to prevent the player from stopping immediately after reaching max speed.
        if (currentSpeed >= maxSpeed && (Input.GetButton("Horizontal") == true || Input.GetButton("Vertical") == true)) 
        {
            if (currentSpeed > maxSpeed && Input.GetButton("Shift") == false)
            {
                if (currentSpeed > maxSpeed + 1)
                {
                    currentSpeed -= currentSpeedDeaccel * Time.deltaTime;
                    ramp();
                }
            }
            moveDirec = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
            momentumDir = moveDirec;
        }
        else if ((Input.GetButton("Horizontal") == true || Input.GetButton("Vertical") == true) && currentSpeed < maxSpeed)
        {
            if (currentSpeed < minSpeed)
            {
                currentSpeed = minSpeed;
            }
            moveDirec = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
            momentumDir = moveDirec;
            currentSpeed += currentSpeedAccel * Time.deltaTime;
            ramp();
        }
        else if ((Input.GetButton("Horizontal") == false && Input.GetButton("Vertical") == false) && currentSpeed > speedZero)
        {
            currentSpeed -= currentSpeedDeaccel * Time.deltaTime;
            ramp();
            if (currentSpeed < 0)
            {
                currentSpeed = 0;
            }
        }
    }

    void moveLike()// Main way the player moves
    {
        if ((Input.GetButton("Horizontal") == true || Input.GetButton("Vertical") == true) && (currentSpeed < maxSpeed || currentSpeed >= maxSpeed))
        {
            controller.Move((moveDirec + knockback) * currentSpeed * Time.deltaTime);
        }
        else if ((Input.GetButton("Horizontal") == false && Input.GetButton("Vertical") == false))
        {
            controller.Move((momentumDir + knockback) * currentSpeed * Time.deltaTime);
        }

    }

    void ramp() // Used to make Accel and Deacell higher over time.
    {
        if ((Input.GetButton("Horizontal") == false && Input.GetButton("Vertical") == false) && currentSpeed < maxSpeed)
        {
            currentSpeedAccel = speedAccelOrig;
            if (currentSpeed == speedZero)
            {
                currentSpeedDeaccel = speedDeaccelOrig;
            }
            else if (currentSpeedDeaccel < maxDeaccel)
            {
                currentSpeedDeaccel += speedDeaccelRate * Time.deltaTime;

            }
        }
        else if ((Input.GetButton("Horizontal") == true || Input.GetButton("Vertical") == true) && currentSpeed > speedZero)
        {
            currentSpeedDeaccel = speedDeaccelOrig;
            if (currentSpeed >= maxSpeed)
            {
                currentSpeedAccel = maxAccel;
            }
            else if (currentSpeedAccel < maxAccel)
            {
                currentSpeedAccel += speedAccelRate * Time.deltaTime;
            }
        }
    }

    void dash() // Dash in a direction but has bools for how you want to specifically dash.
    {

        if (Input.GetButton("Shift") && (Input.GetButton("Horizontal") == true || Input.GetButton("Vertical") == true) && isClimbing == false && isInRagdoll == false)
        {
            isDashing = true;
            if (dashCooldownTimer >= dashCooldown)
            {
                dashCooldownTimer = 0.0f;
                dashTimer = 0.0f;
                currentDashSpeed = dashSpeed;
                StartCoroutine(dashWait(momentumDir * currentDashSpeed * Time.deltaTime));
            }
        }
    }

    void dashMomentum() // Makes the dash add to the player's speed
    {
        if (dashCarryOver)
        {
            if (currentSpeed < maxSpeed)
            {
                if (currentSpeed + currentDashSpeed > maxSpeed)
                {
                    currentSpeed = maxSpeed;
                }
                else
                {
                    currentSpeed += currentDashSpeed;
                }
            }
        }
        currentDashSpeed = 0;
        isDashing = false;
    }

    void dashEnd()// Ends dash
    {
        if (dashTimer >= dashLength || knockbacked)
        {
            StopCoroutine(dashWait(momentumDir * currentDashSpeed * Time.deltaTime));
            dashMomentum();
        }
    }

    IEnumerator dashWait(Vector3 move) // Used to make dash function
    {
        dashTimer = 0.0f;
        while (dashTimer < dashLength)
        {
            dashTimer += Time.deltaTime;
            controller.Move(move);
            yield return null;
        }
    }

    void Gravity()// Made gravity a method for easier use 
    {
        if (isDashing == true)
        {
            Debug.Log("Dashing or Jumping");
            playerVel.y = 0.0f;
        }
        else if (isJumping == true)
        {
            ++jumpCount;
            playerVel.y = jumpSpeed;
            isJumping = false;
        }
        else if (controller.isGrounded)
        {
            Debug.Log("On Floor");
            playerVel.y = -(0.001f);
            jumpCount = 0;
        }
        else if (isDashing == false && isClimbing == false)
        {
            Debug.Log("Not Floor");
            playerVel.y -= gravity * Time.deltaTime;
        }
    }

    public void applyPushback(Vector3 direction)
    {
        pushBack += direction;
    }

    void knockbackMovement()
    {  
        // Appying forces when hit
        if (isInRagdoll && knockbacked)
        {
            movementResetFull();
            playerVel.y = pushBack.y;
            knockback.z = pushBack.z;
            knockback.x = pushBack.x;
            knockbackTimer = 0;
            knockbacked = false;
            pushBack = Vector3.zero;
        }
        else if (knockbacked)
        {
            playerVel.y = pushBack.y;
            knockback.z = pushBack.z;
            knockback.x = pushBack.x;
            knockbackTimer = 0;
            knockbacked = false;
            pushBack = Vector3.zero;
        }
        // How to move the player based on your bool and if you're in ragdoll right now
        if (knockbackOnly && isInRagdoll)
        {
            controller.Move((knockback) * Time.deltaTime); // knock back only
        }
        else if (isInRagdoll)
        {
            controller.Move((momentumDir + knockback) * Time.deltaTime); // knock back is added to the last known input
        }
        // Making knockback decrease
        if (Mathf.Abs(knockback.z) > 0.001f && knockbackTimer > 0.001f)
        {
            knockback.z -= (knockback.z > 0) ? (gravity * Time.deltaTime) : -(gravity * Time.deltaTime);
            currentSpeed = 1; // Removed to prevent the player from being slowed down after a pushback.
        }
        if ((Mathf.Abs(knockback.x)) > 0.001f && knockbackTimer > 0.001f)
        {
            knockback.x -= (knockback.x > 0) ? (gravity * Time.deltaTime) : -(gravity * Time.deltaTime);
            currentSpeed = 1; // Removed to prevent the player from being slowed down after a pushback.
        }
    }

    void ragdollTimer()
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

    void movementResetFull() // Used to reset player Movement values
    {
        currentSpeed = speedZero;
        currentSpeedAccel = minAccel;
        currentSpeedDeaccel = minDeaccel;
        speedDeaccelOrig = currentSpeedDeaccel;
        speedAccelOrig = currentSpeedAccel;
    }

    public void clearKnockback()
    {
        knockbacked = false;
        knockback = Vector3.zero;
    }
}

//Gold's pile of possible features
//add a new layer for dashing, this layer ignores enemy projectiles and would effectly make you invincible based on what it ignores