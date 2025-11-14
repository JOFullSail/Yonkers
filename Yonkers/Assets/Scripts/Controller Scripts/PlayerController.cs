using UnityEngine;
using UnityEditor;
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
    [SerializeField] float freezeDelay = 0.5f; //used to know when the player can be frozen again.
    [SerializeField] float InvincibilityDuration = 0.5f;

    [Header("Jumping")]
    [SerializeField] int jumpSpeed = 12;
    [SerializeField] int jumpMaxCount = 2;
    [Tooltip("Amount of time (in seconds) the player has to trigger the first jump after falling off a platform. \n" +
        "If the player jumps after the grace period, the player will only trigger the second jump.")]
    [SerializeField] float jumpGracePeriod = 0.175f;


    [Header("Shooting")]
    [SerializeField] List<GunStats> gunList = new List<GunStats>();
    [SerializeField] GameObject gunModel;
    [SerializeField] int shootDmg = 1;
    [SerializeField] int shootDist = 20;
    [SerializeField] float shootRate = 0.5f;
    [SerializeField] bool weaponIsHitscan;
    [SerializeField] GameObject projectile;

    [Header("Climbing")]
    [Tooltip("If turned ON, player will only be able to climb on objects with the \"CanClimb\" tag.")]
    [SerializeField] bool useCanClimbTag;
    //[Tooltip("Makes climbing easier for the player.\n\n- Players will be able to continue climbing even while looking away from the wall.\n" +
    //    "- The player will automatically ledge grab when they reach the top of a wall.")]
    //// Make playerVel.y = 0 once they get to the ledge. Attempt to build system that makes the player jump over a wall and land on the surface above automatically.
    //[SerializeField] bool climbAccessibility;
    [SerializeField] float climbSpeed = 10.25f;
    [Tooltip("Amount of time the player is allowed to climb a wall.\n\n- Will be overriden once the player reaches the top of a wall.")]
    [SerializeField] float climbDuration = 0.6f;
    [Tooltip("Max view distance between the player and the wall required for the player to climb a wall.")]
    [SerializeField] float climbWallDetection = 1.25f;
    [Tooltip("Max slope angle of a wall the player can climb.")]
    [Range(0, 180f)][SerializeField] float climbMaxSlopeAngle = 90f;

    [Tooltip("Minimum difference between the angles of both walls required for the player to climb a second wall after jumping from another." +
             "\nThink about it as one wall with an angle of 0 and another wall next to it being the value that is set in this field." +
             "\n\n- If set to 180, the player will only climb walls that are perfectly parallel to each other (180 degrees only)" +
             "\n- If set to 90, the player will be able to climb walls that are parallel or perpendicular to each other (from 90 to 180 only)" +
             "\n- If set to 45, the player will be able to climb walls that have a minimum difference of 45 (from 45 to 180 only)")]
    [Range(0, 180f)][SerializeField] float climbMinAngleDiff = 135f;

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
    [Tooltip("Spawns the player in the Scene Editor's camera location.")]
    [SerializeField] bool debugSpawnAtCamera;
    [Tooltip("Gives the player the ability to climb literally anything.")]
    [SerializeField] bool debugClimbAnything;
    [Tooltip("Gives the player infinite climbing stamina.")]
    [SerializeField] bool debugClimbInfinitely;
    [Tooltip("Gives the player the ability to climb at any given speed set to Debug Climb Speed.\n\n" +
        "- Gravity will not pull you down as fast with high values.")]
    [SerializeField] bool debugFastClimb;
    [Tooltip("Sets the player's climb speed.\n\n- Gravity will not pull you down as fast with high values.")]
    [SerializeField] float debugClimbSpeed = 50f;

    [Header("Audio")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip[] audJump;
    [Range(0, 1)][SerializeField] float audJumpVol;
    [SerializeField] AudioClip[] audHurt;
    [Range(0, 1)][SerializeField] float audHurtVol;
    [SerializeField] AudioClip[] audSteps;
    [Range(0, 1)][SerializeField] float audStepsVol;
    [SerializeField] AudioClip[] audDash;
    [Range(0, 1)][SerializeField] float audDashVol;
    [SerializeField] AudioClip[] audSpawn;
    [Range(0, 1)][SerializeField] float audSpawnVol;
    [SerializeField] AudioClip[] audGun; //mainly for the reload since it's player side //NEW NOTE: this could be a gun specific thing!
    [Range(0, 1)][SerializeField] float audGunVol;
    //RayCast
    RaycastHit hit;
    //Ints
    int gunListIdx;
    int jumpCount;
    int hpOrig = 4;
    //bools
    bool isDashing;
    bool isClimbing;
    bool isJumping;
    bool isInRagdoll;
    bool knockbacked;
    bool gravityOn; //true = gravity active // false = gravity disabled *MAINLY FOR SPRINGS DON'T USE FOR KNOCKBACK THINGS*
    bool frozenOn; //false = not frozen //true = frozen
    public bool invertMove;
    bool isPlayingSteps;
    //Floats
    public float gravityOffTimer; // Used to time a duration of having no gravity.
    public float gravityLockout = 0; //amount of time gravity is disabled
    public float freezeTimer; // Used to time a duration of being frozen.      < HEY BROLY CHECK SPRING CODE FOR AN EXAMPLE OF HOW TO USE THIS
    public float freezeLockout = 0;//amount of time player is disabled         <
    float freezeDelaytimer; //used to know when you can be frozen again. 
    float knockbackTimer;      // Used to know when to start losing knockback.
    public float blindDuration;
    public float invertDuration;

    // Timers
    float jumpTimer;
    float climbTimer;
    float shootTimer;
    float ragdollTimeLeft;
    float dashCooldownTimer;   // Used to track dash cooldown.
    float dashTimer;           // Used to track how long dash will go.
    float speedDeaccelOrig;
    float speedAccelOrig;
    float speedZero = 0.0f;
    float InvincibilityTimer;

    //Movement V2
    float currentSpeedX; //forward+ and back-
    float currentSpeedAccelX;
    float currentSpeedDeaccelX;
    float currentDashSpeedX;
    float currentSpeedZ; //right+ and left-
    float currentSpeedAccelZ;
    float currentSpeedDeaccelZ;
    float currentDashSpeedZ;

    // Vectors for the player
    Vector3 moveDirecX; //Movement V2 input from on X axis
    Vector3 moveDirecZ; //Movement V2 input from on Z axis
    Vector3 playerVel;   // Used for jump and also holds pushback.y.
    Vector3 pushBack;    // Force applied to the player.
    Vector3 momentumDirX; //Movement V2 last input direction on X axis
    Vector3 momentumDirZ;//Movement V2 last input direction on Z axis
    Vector3 knockback;   // Used to hold pushBack.x and z.

    // For climb()
    Vector3 prevWallPos;
    Vector3 newWallPos;
    Vector3 prevWallNorm;
    Vector3 newWallNorm;
	float newWallHeight = 0f;

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

    //what gold needs to do
    //make movement work on two axis speeds. Done
    //make springs add to current speed
    //make springs able to chain halfway
    //add slow down for broly
    //add freeze for broly halfway
    //make a toggle for gravity done
    //knockback

    public int CurrentHealth
    {
        get { return HP; }
        set { HP = value; }
    }

    public int OriginalHealth
    {
        get { return hpOrig; }
        set { hpOrig = value; }
    }

    public int GunListIndex
    {
        get { return gunListIdx; }
        set { gunListIdx = value; }
    }
    public bool DebugSpawnAtCamera
    {
        get { return debugSpawnAtCamera; }
    }
    public float RagdollTimeLeft
    {
        get { return ragdollTimeLeft; }
        set { ragdollTimeLeft = value; }
    }

    public bool IsInRagdoll
    {
        get { return isInRagdoll; }
        set { isInRagdoll = value; }
    }

    public bool Knockbacked
    {
        get { return knockbacked; }
        set { knockbacked = value; }
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

    public bool FrozenOn
    {
        get { return frozenOn; }
        set { frozenOn = value; }
    }

    public Vector3 PlayerVel
    {
        get { return playerVel; }
        set { playerVel = value; }
    }

    public List<GunStats> GunList
    {
        get { return gunList; }
        set { gunList = value; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentSpeedX = speedZero;
        currentSpeedZ = speedZero;
        currentSpeedAccelX = minAccel;
        currentSpeedDeaccelX = minDeaccel;
        currentSpeedAccelZ = minAccel;
        currentSpeedDeaccelZ = minDeaccel;
        speedDeaccelOrig = currentSpeedDeaccelZ;
        speedDeaccelOrig = currentSpeedDeaccelX;
        speedAccelOrig = currentSpeedAccelZ;
        speedAccelOrig = currentSpeedAccelX;
        isDashing = false;
        gravityOn = true;
        isPlayingSteps = false;
        invertMove = false;
        newWallNorm.y = 7f;

        if (debugClimbInfinitely) climbDuration = float.MaxValue;
        if (debugFastClimb) climbSpeed = debugClimbSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.red);
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * climbWallDetection, Color.blue);

        if (!GameManager.instance.isPaused)
        {
            timers();
            shoot();
            playerMovement();
        }
    }

    void playerMovement()
    {
        _Invincibility_();
        knockbackMovement();
        Frozen(); //checking if you're frozen
        if (frozenOn == false) // long as you're not frozen you can do all your usual movement
        {
            climb();
            dash();
            movement();
            dashEnd();
            jump();
        }
        controller.Move(playerVel * Time.deltaTime); // Used here to apply gravity correctly
        Gravityoff(); //checking if you disabled gravity first
        if (gravityOn)
        {
            Gravity();
        }
    }

    bool angularDifference(float norm1, float norm2, float minDegrees)
    {
        float difference = (Mathf.Acos(norm1) * Mathf.Rad2Deg) - (Mathf.Acos(norm2) * Mathf.Rad2Deg);
        if (difference >= climbMinAngleDiff) return true;
        else return false; 
    }

    void climb()
    {
        // Climb Layers
        int noClimbLayers;
        string noClimbTag;
        if (useCanClimbTag) noClimbTag = "CanClimb";
        else noClimbTag = "NoClimb";
        
        if (debugClimbAnything)
        {
            noClimbLayers = 0;
            noClimbTag = "Player";
        }
        else noClimbLayers = ignoreClimbing.value;
        
        // Wall Vaulting
        if (transform.position.y >= newWallHeight) 
            prevWallNorm = new Vector3(7f, 7f, 7f);
        
        // Wall Detection
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, climbWallDetection, ~noClimbLayers))
        {
            // Displays the normal of the object the player is facing.
            Debug.DrawRay(hit.transform.position, hit.normal, Color.red, 5f);
            
            // Storing data of the wall the player is currently facing.
            newWallPos = hit.transform.position;
            newWallPos.y = 0;
            newWallNorm = hit.normal;
			if (!isClimbing) newWallHeight = hit.transform.position.y + 0.5f * hit.transform.localScale.y;
            
            // Angle Calculation
            int wallAngle = (int)Vector3.Angle(hit.normal, Vector3.up);
            int wallDifference = Mathf.RoundToInt(Vector3.Angle(prevWallNorm.normalized, newWallNorm.normalized));

            // Conditions 
            bool canBeClimbed;
            if (useCanClimbTag) canBeClimbed = hit.collider.CompareTag(noClimbTag);
            else canBeClimbed = !hit.collider.CompareTag(noClimbTag);
            bool isAboveMinSlope = controller.slopeLimit <= wallAngle;
            bool isBelowMaxSlope = wallAngle <= climbMaxSlopeAngle;
            bool isFallOrClimb = playerVel.y < -1 || isClimbing;
            bool notSameWall = prevWallPos == null || newWallPos != prevWallPos;
            bool isAboveMinDiff = prevWallNorm.y == 7f || wallDifference >= climbMinAngleDiff;
            bool climbTimeLeft = climbTimer <= climbDuration;
            
            // Check for climbing walls
            if ((canBeClimbed && // If it doesn't have the "NoClimb" tag
                isAboveMinSlope && // Greater than or equal to the max angle a surface can have 
                isBelowMaxSlope && // Less than or equal to the max angle the player can climb
                !controller.isGrounded && // If not on the ground
                isFallOrClimb && // If velocity is less than 1 or if already climbing
                isAboveMinDiff && // Greater or equal to the minimum angle the new wall is compared to the previous.
                notSameWall && climbTimeLeft && gravityOn) || 
                debugClimbAnything && canBeClimbed && isFallOrClimb) // For debugClimbAnything
            {
                if (!isClimbing && jumpCount > 0) --jumpCount;
                playerVel.y = climbSpeed;
                isClimbing = true;
            }
            else // Not climbing
            {
                if (!climbTimeLeft)
                {
                    prevWallPos = newWallPos;
                    prevWallNorm = newWallNorm;
                }

                isClimbing = false;
            }
        }
        else // Not looking at a wall
        {
            // Triggers only on the first frame
            if (isClimbing)
            { 
                jumpTimer = 0;
                prevWallPos = newWallPos;
                prevWallNorm = newWallNorm;
            }

            isClimbing = false;
        }
    }

    void jump()
    {
        if (!isClimbing && Input.GetButtonDown("Jump") && jumpCount < jumpMaxCount && gravityOn)
        {
            if (jumpTimer >= jumpGracePeriod && jumpCount == 0)
            {
                ++jumpCount;
            }
            aud.PlayOneShot(audJump[Random.Range(0, audJump.Length)], audJumpVol);
            isJumping = true;
        }
    }

    void shootApply()
    {
        shootTimer = 0;
        --gunList[gunListIdx].ammoCurrent;

        // ~ignoreLayer will ignore the player later to prevent the player shooting themselves.
        if (weaponIsHitscan && Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist, ~ignoreShooting))
        {
            Instantiate(gunList[gunListIdx].hitEffect, hit.point, Quaternion.identity);

            IDamage dmg = hit.collider.GetComponent<IDamage>();
            IActivate act = hit.collider.GetComponent<IActivate>();  
            
            if (dmg != null)
            {
                dmg.takeDamage(shootDmg);
            }

            if (act != null)
            {
                act.activate();
            }
        }
        else if (projectile != null && !weaponIsHitscan)
            Instantiate(projectile, Camera.main.transform.position, Camera.main.transform.rotation);
    }

    public void takeDamage(int amount)
    {
        if (GameManager.instance.player.layer == 3)
        { 
            HP -= amount;
            InvincibilityTimer = 0;
            updatePlayerUI();
            if(amount > 0)
            {
                if (HP > 0)
                {
                    StartCoroutine(flashDmgScreen());
                }
                aud.PlayOneShot(audHurt[Random.Range(0, audHurt.Length)], audHurtVol);
            }
            else
            {
                StartCoroutine(flashHealScreen());
            }
        }

        if (HP <= 0)
        {
            GameManager.instance.youDied();
        }
    }

    IEnumerator flashDmgScreen()
    {
        float dmgScreenTimer = 0f;

        Color colorOrig = GameManager.instance.playerDamageScreen.color;

        colorOrig = GameManager.instance.playerDamageScreen.color = new Color(colorOrig.r, colorOrig.g, colorOrig.b, 0.3922f);

        while (dmgScreenTimer < InvincibilityDuration)
        {
            float a = Mathf.Lerp(0.3922f, 0f, dmgScreenTimer / InvincibilityDuration);

            GameManager.instance.playerDamageScreen.color = new Color(colorOrig.r, colorOrig.g, colorOrig.b, a);
            dmgScreenTimer += Time.deltaTime;
            yield return null;
        }

        GameManager.instance.playerDamageScreen.color = new Color(colorOrig.r, colorOrig.g, colorOrig.b, 0f);
    }

    IEnumerator flashHealScreen()
    {
        float healScreenTimer = 0f;

        Color colorOrig = GameManager.instance.playerHealScreen.color;

        colorOrig = GameManager.instance.playerHealScreen.color = new Color(colorOrig.r, colorOrig.g, colorOrig.b, 0.3922f);

        while (healScreenTimer < InvincibilityDuration)
        {
            float a = Mathf.Lerp(0.3922f, 0f, healScreenTimer / InvincibilityDuration);

            GameManager.instance.playerHealScreen.color = new Color(colorOrig.r, colorOrig.g, colorOrig.b, a);
            healScreenTimer += Time.deltaTime;
            yield return null;
        }

        GameManager.instance.playerHealScreen.color = new Color(colorOrig.r, colorOrig.g, colorOrig.b, 0f);
    }

    void timers()
    {
        if (!controller.isGrounded) jumpTimer += Time.deltaTime;
        else jumpTimer = 0;

        if (isClimbing) climbTimer += Time.deltaTime;
        else climbTimer = 0;
        
        shootTimer += Time.deltaTime;
        dashCooldownTimer += Time.deltaTime;
        ragdollTimer();
        knockbackTimer += Time.deltaTime;
        gravityOffTimer += Time.deltaTime;
        freezeTimer += Time.deltaTime;
        freezeDelaytimer += Time.deltaTime;
        InvincibilityTimer += Time.deltaTime;
    }

    void shoot()
    {
        if (Input.GetButton("Fire1") && gunList.Count > 0 && gunList[gunListIdx].ammoCurrent > 0 && shootTimer >= shootRate)
        {
            shootApply();
            aud.PlayOneShot(gunList[gunListIdx].shootSound[Random.Range(0, gunList[gunListIdx].shootSound.Length)], gunList[gunListIdx].shootSoundVol);
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

            updatePlayerUI();
        }


        reload();
        if (gunList.Count > 0) switchGun();
    }

    void reload()
    {
        if (Input.GetButtonDown("Reload") && gunList.Count > 0 && gunList[gunListIdx].ammoReserves > 0)
        {
            aud.PlayOneShot(gunList[gunListIdx].reloadSound[Random.Range(0, gunList[gunListIdx].reloadSound.Length)], gunList[gunListIdx].reloadSoundVol);
            int ammoToLoad = gunList[gunListIdx].ammoMax <= gunList[gunListIdx].ammoReserves ? gunList[gunListIdx].ammoMax : gunList[gunListIdx].ammoReserves;
            gunList[gunListIdx].ammoReserves -= ammoToLoad;
            gunList[gunListIdx].ammoCurrent = ammoToLoad;

            updatePlayerUI();
        }
    }

    public void GetGunStats(GunStats gun)
    {
        gunList.Add(gun);
        gunListIdx = gunList.Count - 1;

        changeGun();
    }

    public void changeGun()
    {
        shootDmg = gunList[gunListIdx].hitscanShootDamage;
        shootDist = gunList[gunListIdx].hitscanShootDist;
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

        updatePlayerUI();
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
        //V1
        //if (currentSpeed >= maxSpeed && (Input.GetButton("Horizontal") == true || Input.GetButton("Vertical") == true))
        //{
        //    if (currentSpeed > maxSpeed && Input.GetButton("Shift") == false)
        //    {
        //        if (currentSpeed > maxSpeed + 1)
        //        {
        //            currentSpeed -= currentSpeedDeaccel * Time.deltaTime;
        //            ramp();
        //        }
        //    }
        //    moveDirec = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        //    momentumDir = moveDirec;
        //}
        //else if ((Input.GetButton("Horizontal") == true || Input.GetButton("Vertical") == true) && currentSpeed < maxSpeed)
        //{
        //    if (currentSpeed < minSpeed)
        //    {
        //        currentSpeed = minSpeed;
        //    }
        //    moveDirec = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        //    momentumDir = moveDirec;
        //    ramp();
        //}
        //else if ((Input.GetButton("Horizontal") == false && Input.GetButton("Vertical") == false) && currentSpeed > speedZero)
        //{
        //    currentSpeed -= currentSpeedDeaccel * Time.deltaTime;
        //    ramp();
        //    if (currentSpeed < 0)
        //    {
        //        currentSpeed = 0;
        //    }
        //}
        //V2

        if ((Input.GetButton("UP") == false && Input.GetButton("DOWN") == false && Input.GetButton("LEFT") == false && Input.GetButton("RIGHT") == false) && ((currentSpeedZ > speedZero || currentSpeedZ < speedZero) || (currentSpeedX > speedZero || currentSpeedX < speedZero)))
        {
            ramp();
        }
        else
        {
            //vertical X
            if (((Input.GetButton("UP") == true) && Input.GetButton("DOWN") == true) || ((Input.GetButton("UP") == false) && Input.GetButton("DOWN") == false)) // if both inputed 
            {
                ramp();
            }
            else if ((Input.GetButton("UP") == true) && currentSpeedX >= maxSpeed) // over max speed going forward
            {
                moveDirecX = Input.GetAxis("UP") * transform.forward;
                momentumDirX = moveDirecX;
                ramp();
            }
            else if ((Input.GetButton("DOWN") == true) && currentSpeedX <= -maxSpeed) // over max speed going backwards
            {
                moveDirecX = Input.GetAxis("DOWN") * transform.forward;
                momentumDirX = moveDirecX;
                ramp();
            }
            else if ((Input.GetButton("UP") == true) && currentSpeedX < maxSpeed) // getting speed forward
            {
                moveDirecX = Input.GetAxis("UP") * transform.forward;
                momentumDirX = moveDirecX;
                ramp();
            }
            else if ((Input.GetButton("DOWN") == true) && currentSpeedX > -maxSpeed) // getting speed backwards
            {
                moveDirecX = Input.GetAxis("DOWN") * transform.forward;
                momentumDirX = moveDirecX;
                ramp();
            }
            //horizontal Z
            if (((Input.GetButton("LEFT") == true) && Input.GetButton("RIGHT") == true) || ((Input.GetButton("LEFT") == false) && Input.GetButton("RIGHT") == false)) // if both inputed
            {
                ramp();
            }
            else if ((Input.GetButton("RIGHT") == true) && currentSpeedZ >= maxSpeed) // over max speed going right
            {
                moveDirecZ = -Input.GetAxis("RIGHT") * transform.right; //transform.right and Z
                momentumDirZ = moveDirecZ;
                ramp();
            }
            else if ((Input.GetButton("LEFT") == true) && currentSpeedZ <= -maxSpeed) // over max speed going left
            {
                moveDirecZ = Input.GetAxis("LEFT") * transform.right;
                momentumDirZ = moveDirecZ;
                ramp();
            }
            else if ((Input.GetButton("RIGHT") == true) && currentSpeedZ < maxSpeed) // getting speed right
            {
                moveDirecZ = -Input.GetAxis("RIGHT") * transform.right;
                momentumDirZ = moveDirecZ;
                ramp();
            }
            else if ((Input.GetButton("LEFT") == true) && currentSpeedZ > -maxSpeed) // getting speed left
            {
                moveDirecZ = Input.GetAxis("LEFT") * transform.right;
                momentumDirZ = moveDirecZ;
                ramp();
            }
        }
    }

    void moveLike()// Main way the player moves
    {
        //V2
        if (invertMove == true)
        {
            currentSpeedX = -currentSpeedX;
            currentSpeedZ = -currentSpeedZ;
        }

        if (gravityOn == false) //if gravity is off you can only have knockback, no inputs allowed!
        {
            controller.Move(knockback * Time.deltaTime);
        }
        else if ((Input.GetButton("UP") == true || Input.GetButton("DOWN") == true || Input.GetButton("LEFT") == true || Input.GetButton("RIGHT") == true))
        {
            controller.Move((((moveDirecX * currentSpeedX) + (moveDirecZ * currentSpeedZ)) + (knockback)) * Time.deltaTime);
        }
        else if (Input.GetButton("UP") == false && Input.GetButton("DOWN") == false && Input.GetButton("LEFT") == false && Input.GetButton("RIGHT") == false)
        {
            controller.Move((((momentumDirX * currentSpeedX) + (momentumDirZ * currentSpeedZ)) + (knockback)) * Time.deltaTime);
        }

        if (invertMove == true)
        {
            currentSpeedX = -currentSpeedX;
            currentSpeedZ = -currentSpeedZ;
        }
    }

    void ramp() // Used to make Accel and Deacell higher over time. MovementV2: Will also do the decrease and increase of current speeds
    {

        //V2
        //vertical
        if ((Input.GetButton("UP") == false && Input.GetButton("DOWN") == false && Input.GetButton("LEFT") == false && Input.GetButton("RIGHT") == false) && ((currentSpeedZ > speedZero || currentSpeedZ < speedZero) || (currentSpeedX > speedZero || currentSpeedX < speedZero)))
        {
            //// no input but X or Z still have speed. They decrease until set 0
            if (currentSpeedX > speedZero || currentSpeedX < speedZero)
            {
                if (currentSpeedX < (0.5f + maxAccel) && currentSpeedX > (-0.5f + -maxDeaccel))
                {
                    currentSpeedX = 0;
                }
                else
                {
                    if (currentSpeedX > speedZero)
                    {
                        currentSpeedAccelX = speedAccelOrig;
                        currentSpeedX -= currentSpeedDeaccelX;
                        if (currentSpeedX > speedZero)
                        {
                            currentSpeedX = 0;
                        }
                        else if (currentSpeedDeaccelX < maxDeaccel) //Deaccel ramp
                        {
                            currentSpeedDeaccelX += speedDeaccelRate * Time.deltaTime;
                        }
                    }
                    else if (currentSpeedX < speedZero)
                    {
                        currentSpeedDeaccelX = speedDeaccelOrig;
                        currentSpeedX += currentSpeedAccelX;
                        if (currentSpeedX < speedZero)
                        {
                            currentSpeedX = 0;
                        }
                        else if (currentSpeedAccelX < maxAccel) //Aceel ramp
                        {
                            currentSpeedAccelX += speedAccelRate * Time.deltaTime;
                        }
                    }
                }
            }
            if (currentSpeedZ > speedZero || currentSpeedZ < speedZero)
            {
                if ((currentSpeedZ < (0.5f + maxAccel)) && (currentSpeedZ > (-0.5f + -maxDeaccel)))
                {
                    currentSpeedZ = 0;
                }
                else
                {
                    if (currentSpeedZ > speedZero)
                    {
                        currentSpeedAccelZ = speedAccelOrig;
                        currentSpeedZ -= currentSpeedDeaccelZ;
                        if (currentSpeedZ > speedZero)
                        {
                            currentSpeedZ = 0;
                        }
                        else if (currentSpeedDeaccelZ < maxDeaccel) //Deaccel ramp
                        {
                            currentSpeedDeaccelZ += speedDeaccelRate * Time.deltaTime;
                        }
                    }
                    else if (currentSpeedZ < speedZero)
                    {
                        currentSpeedDeaccelZ = speedDeaccelOrig;
                        currentSpeedZ += currentSpeedAccelZ;
                        if (currentSpeedZ < speedZero)
                        {
                            currentSpeedZ = 0;
                        }
                        else if (currentSpeedAccelZ < maxAccel) //Aceel ramp
                        {
                            currentSpeedAccelZ += speedAccelRate * Time.deltaTime;
                        }
                    }
                }
            }
        }
        else // if there's a input
        {
            if (((Input.GetButton("UP") == true) && Input.GetButton("DOWN") == true) || ((Input.GetButton("UP") == false) && Input.GetButton("DOWN") == false)) // if both inputed or neither, nothing happens direction wise, but you slow down all the same.
            {
                if (currentSpeedX > speedZero || currentSpeedX < speedZero)
                {
                    if (currentSpeedX < (0.5f + maxAccel) && currentSpeedX > (-0.5f + -maxDeaccel))
                    {
                        currentSpeedX = 0;
                    }
                    else
                    {
                        if (currentSpeedX > speedZero)
                        {
                            currentSpeedAccelX = speedAccelOrig;
                            currentSpeedX -= currentSpeedDeaccelX;
                            if (currentSpeedX > speedZero)
                            {
                                currentSpeedX = 0;
                            }
                            else if (currentSpeedDeaccelX < maxDeaccel) //Deaccel ramp
                            {
                                currentSpeedDeaccelX += speedDeaccelRate * Time.deltaTime;
                            }
                        }
                        else if (currentSpeedX < speedZero)
                        {
                            currentSpeedDeaccelX = speedDeaccelOrig;
                            currentSpeedX += currentSpeedAccelX;
                            if (currentSpeedX < speedZero)
                            {
                                currentSpeedX = 0;
                            }
                            else if (currentSpeedAccelX < maxAccel) //Aceel ramp
                            {
                                currentSpeedAccelX += speedAccelRate * Time.deltaTime;
                            }
                        }
                    }
                }
            }
            else if ((Input.GetButton("UP") == true) && currentSpeedX >= maxSpeed) // over max speed going forward, you slow down, and it ramps up until you reach your normal max speed
            {
                if (currentSpeedX > maxSpeed && Input.GetButton("Shift") == false)
                {
                    if (currentSpeedX > maxSpeed + 1)
                    {
                        currentSpeedX -= currentSpeedDeaccelX * Time.deltaTime;
                        if (currentSpeedDeaccelX < maxDeaccel) //Deaccel ramp
                        {
                            currentSpeedDeaccelX += speedDeaccelRate * Time.deltaTime;
                        }
                    }
                }
            }
            else if ((Input.GetButton("DOWN") == true) && currentSpeedX <= -maxSpeed) // over max speed going backwards, you slow down, and it ramps up until you reach your normal NEGATIVE max speed
            {
                if (currentSpeedX < -maxSpeed && Input.GetButton("Shift") == false)
                {
                    if (currentSpeedX < -maxSpeed - 1)
                    {
                        currentSpeedX += currentSpeedAccelX * Time.deltaTime;
                        if (currentSpeedAccelX < maxAccel) //Aceel ramp
                        {
                            currentSpeedAccelX += speedAccelRate * Time.deltaTime;
                        }
                    }
                }
            }
            else if ((Input.GetButton("UP") == true) && currentSpeedX < maxSpeed) // getting speed forward
            {
                if (currentSpeedX > -minSpeed && currentSpeedX < minSpeed)
                {
                    currentSpeedX = minSpeed;
                }
                currentSpeedDeaccelX = speedDeaccelOrig;
                currentSpeedX += currentSpeedAccelX * Time.deltaTime;
                if (currentSpeedAccelX < maxAccel) //Aceel ramp
                {
                    currentSpeedAccelX += speedAccelRate * Time.deltaTime;
                }
            }
            else if ((Input.GetButton("DOWN") == true) && currentSpeedX > -maxSpeed) // getting speed backwards
            {
                if (currentSpeedX > -minSpeed && currentSpeedX < minSpeed)
                {
                    currentSpeedX = -minSpeed;
                }
                currentSpeedAccelX = speedAccelOrig;
                currentSpeedX -= currentSpeedDeaccelX * Time.deltaTime;
                if (currentSpeedDeaccelX < maxDeaccel) //Deaccel ramp
                {
                    currentSpeedDeaccelX += speedDeaccelRate * Time.deltaTime;
                }
            }
            //horizontal
            if (((Input.GetButton("LEFT") == true) && Input.GetButton("RIGHT") == true) || ((Input.GetButton("LEFT") == false) && Input.GetButton("RIGHT") == false)) // if both inputed or neither
            {
                if (currentSpeedZ > speedZero || currentSpeedZ < speedZero)
                {
                    if ((currentSpeedZ < (0.5f + maxAccel)) && (currentSpeedZ > (-0.5f + -maxDeaccel)))
                    {
                        currentSpeedZ = 0;
                    }
                    else
                    {
                        if (currentSpeedZ > speedZero)
                        {
                            currentSpeedAccelZ = speedAccelOrig;
                            currentSpeedZ -= currentSpeedDeaccelZ;
                            if (currentSpeedZ > speedZero)
                            {
                                currentSpeedZ = 0;
                            }
                            else if (currentSpeedDeaccelZ < maxDeaccel) //Deaccel ramp
                            {
                                currentSpeedDeaccelZ += speedDeaccelRate * Time.deltaTime;
                            }
                        }
                        else if (currentSpeedZ < speedZero)
                        {
                            currentSpeedDeaccelZ = speedDeaccelOrig;
                            currentSpeedZ += currentSpeedAccelZ;
                            if (currentSpeedZ < speedZero)
                            {
                                currentSpeedZ = 0;
                            }
                            else if (currentSpeedAccelZ < maxAccel) //Aceel ramp
                            {
                                currentSpeedAccelZ += speedAccelRate * Time.deltaTime;
                            }
                        }
                    }
                }
            }
            else if ((Input.GetButton("RIGHT") == true) && currentSpeedZ >= maxSpeed) // over max speed going right
            {
                if (currentSpeedX > maxSpeed && Input.GetButton("Shift") == false)
                {
                    if (currentSpeedZ > maxSpeed + 1)
                    {
                        currentSpeedZ -= currentSpeedDeaccelZ * Time.deltaTime;
                        if (currentSpeedDeaccelZ < maxDeaccel) //Deaccel ramp
                        {
                            currentSpeedDeaccelZ += speedDeaccelRate * Time.deltaTime;
                        }
                    }
                }
            }
            else if ((Input.GetButton("LEFT") == true) && currentSpeedZ <= -maxSpeed) // over max speed going left
            {
                if (currentSpeedZ < -maxSpeed && Input.GetButton("Shift") == false)
                {
                    if (currentSpeedZ < -maxSpeed - 1)
                    {
                        currentSpeedZ += currentSpeedAccelZ * Time.deltaTime;
                        if (currentSpeedAccelZ < maxAccel) //Aceel ramp
                        {
                            currentSpeedAccelZ += speedAccelRate * Time.deltaTime;
                        }
                    }
                }
            }
            else if ((Input.GetButton("RIGHT") == true) && currentSpeedZ < maxSpeed) // getting speed right
            {
                if (currentSpeedZ > -minSpeed && currentSpeedZ < minSpeed)
                {
                    currentSpeedZ = minSpeed;
                }
                currentSpeedDeaccelZ = speedDeaccelOrig;
                currentSpeedZ += currentSpeedAccelZ * Time.deltaTime;
                if (currentSpeedAccelZ < maxAccel) //Aceel ramp
                {
                    currentSpeedAccelZ += speedAccelRate * Time.deltaTime;
                }
            }
            else if ((Input.GetButton("LEFT") == true) && currentSpeedZ > -maxSpeed) // getting speed left
            {
                if (currentSpeedZ > -minSpeed && currentSpeedZ < minSpeed)
                {
                    currentSpeedZ = -minSpeed;
                }
                currentSpeedAccelZ = speedAccelOrig;
                currentSpeedZ -= currentSpeedDeaccelZ * Time.deltaTime;
                if (currentSpeedDeaccelZ < maxDeaccel) //Deaccel ramp
                {
                    currentSpeedDeaccelZ += speedDeaccelRate * Time.deltaTime;
                }
            }
        }

    }

    void dash() // Dash in a direction. Bool for if you want dash to increase your movement speed
    {
        // Dash Cooldown UI
        GameManager.instance.playerDashCooldown.fillAmount = ButtonFunctions.normalize(0f, dashCooldown, dashCooldownTimer);
        
        //v2
        if (Input.GetButton("Shift") && (Input.GetButton("UP") == true || Input.GetButton("DOWN") == true || Input.GetButton("LEFT") == true || Input.GetButton("RIGHT") == true) && isClimbing == false && isInRagdoll == false && gravityOn)
        {
            if (dashCooldownTimer >= dashCooldown)
            {
                isDashing = true;
                dashCooldownTimer = 0.0f;
                dashTimer = 0.0f;
                if (Input.GetButton("UP") == true && Input.GetButton("DOWN") == true)
                {
                    currentDashSpeedX = 0;
                }
                else if (Input.GetButton("UP") == true)
                {
                    currentDashSpeedX = dashSpeed;
                }
                else if (Input.GetButton("DOWN") == true)
                {
                    currentDashSpeedX = -dashSpeed;
                }
                if (Input.GetButton("LEFT") == true && Input.GetButton("RIGHT") == true)
                {
                    currentDashSpeedZ = 0;
                }
                else if (Input.GetButton("LEFT") == true)
                {
                    currentDashSpeedZ = -dashSpeed;
                }
                else if (Input.GetButton("RIGHT") == true)
                {
                    currentDashSpeedZ = dashSpeed;
                }
                aud.PlayOneShot(audDash[Random.Range(0, audDash.Length)], audDashVol);
                StartCoroutine(dashWait((moveDirecX * currentDashSpeedX) * Time.deltaTime + (moveDirecZ * currentDashSpeedZ) * Time.deltaTime));
            }
        }
    }

    void dashMomentum() // Makes the dash add to the player's speed
    {
        //if (dashCarryOver)
        //{
        //    if (currentSpeed < maxSpeed)
        //    {
        //        if (currentSpeed + currentDashSpeed > maxSpeed)
        //        {
        //            currentSpeed = maxSpeed;
        //        }
        //        else
        //        {
        //            currentSpeed += currentDashSpeed;
        //        }
        //    }
        //}
        //currentDashSpeed = 0;
        //isDashing = false;
        //v2
        if (dashCarryOver)
        {
            //Dash on X axis
            if (currentDashSpeedX > 0) // if dash on X was postive
            {
                if (currentSpeedX < maxSpeed)
                {
                    if (currentSpeedX + currentDashSpeedX > maxSpeed)
                    {
                        currentSpeedX = maxSpeed;
                    }
                    else
                    {
                        currentSpeedX += currentDashSpeedX;
                    }
                }
            }
            else // if dash on X was negative
            {
                if (currentSpeedX > -maxSpeed)
                {
                    if (currentSpeedX + currentDashSpeedX < -maxSpeed)
                    {
                        currentSpeedX = -maxSpeed;
                    }
                    else
                    {
                        currentSpeedX += currentDashSpeedX;
                    }
                }
            }
            // Dash on Z axis
            if (currentDashSpeedZ > 0) // if dash on Z was postive
            {
                if (currentSpeedZ < maxSpeed)
                {
                    if (currentSpeedZ + currentDashSpeedZ > maxSpeed)
                    {
                        currentSpeedZ = maxSpeed;
                    }
                    else
                    {
                        currentSpeedZ += currentDashSpeedZ;
                    }
                }
            }
            else // if dash on Z was negative
            {
                if (currentSpeedZ > -maxSpeed)
                {
                    if (currentSpeedZ + currentDashSpeedZ < -maxSpeed)
                    {
                        currentSpeedZ = -maxSpeed;
                    }
                    else
                    {
                        currentSpeedZ += currentDashSpeedZ;
                    }
                }
            }
        }
        currentDashSpeedX = 0;
        currentDashSpeedZ = 0;
        isDashing = false;
    }

    void dashEnd()// Ends dash
    {
        //if (dashTimer >= dashLength || knockbacked)
        //{
        //    StopCoroutine(dashWait(momentumDir * currentDashSpeed * Time.deltaTime));
        //    dashMomentum();
        //}
        //v2
        if (dashTimer >= dashLength || knockbacked || GameManager.instance.isPaused)
        {
            StopCoroutine(dashWait((momentumDirX * currentDashSpeedX) * Time.deltaTime + (momentumDirZ * currentDashSpeedZ) * Time.deltaTime));
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
            GameManager.instance.playerDashCooldown.fillAmount = ButtonFunctions.normalize(dashLength, 0f, dashTimer);
            yield return null;
        }
    }

    void Gravity()// Made gravity a method for easier use 
    {
        if (isDashing == true)
        {
            //Debug.Log("Dashing or Jumping");
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
            //Debug.Log("On Floor");
            if (((currentSpeedX > 1 || currentSpeedX < -1) || (currentSpeedZ > 1 || currentSpeedZ < -1)) && isPlayingSteps == false)
            {
                StartCoroutine(playStep());
            }

            playerVel.y = -(0.001f);
            jumpCount = 0;
            prevWallPos = new Vector3(0f, 0f, 0f);
            newWallPos = new Vector3(0f, 0f, 0f);
            prevWallNorm = new Vector3(7f, 7f, 7f);
        }
        else if (isDashing == false && isClimbing == false)
        {
            //Debug.Log("Not Floor");
            playerVel.y -= gravity * Time.deltaTime;
        }
    }

    public void applyPushback(Vector3 direction)
    {
        pushBack = direction;
    }

    void knockbackMovement()
    {

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
        // Appying forces when hit

        // How to move the player based on your bool and if you're in ragdoll right now
        if (knockbackOnly && isInRagdoll)
        {
            controller.Move((knockback) * Time.deltaTime); // knock back only
        }
        else if (isInRagdoll)
        {
            controller.Move((momentumDirX + knockback) * Time.deltaTime + (momentumDirZ + knockback) * Time.deltaTime); // knock back is added to the last known input
        }
        // Making knockback decrease
        if (gravityOn == true)
        {
            if (Mathf.Abs(knockback.z) > 0.01f && knockbackTimer > 0.001f)
            {
                knockback.z -= (knockback.z > 0) ? (gravity * Time.deltaTime) : -(gravity * Time.deltaTime);
            }
            if ((Mathf.Abs(knockback.x)) > 0.01f && knockbackTimer > 0.001f)
            {
                knockback.x -= (knockback.x > 0) ? (gravity * Time.deltaTime) : -(gravity * Time.deltaTime);
            }
            if (knockback.z < 0.5f && knockback.z > -0.5f)
            {
                knockback.z = 0;
            }
            if (knockback.x < 0.5f && knockback.x > -0.5f)
            {
                knockback.x = 0;
            }
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
        currentSpeedX = speedZero;
        currentSpeedZ = speedZero;
        currentSpeedAccelX = minAccel;
        currentSpeedDeaccelX = minDeaccel;
        currentSpeedAccelZ = minAccel;
        currentSpeedDeaccelZ = minDeaccel;
    }

    public void clearKnockback()
    {
        knockbacked = false;
        knockback = Vector3.zero;
    }
    void Gravityoff() //checks if you changed gravity lockout and reset the timer.
    {
        if (gravityOffTimer < gravityLockout)
        {
            gravityOn = false;
        }
        else
        {
            gravityOn = true;
        }
    }


    public void Frozen() //checks if you changed freeze lockout and reset the timer.
    {
        if (freezeDelaytimer > freezeDelay)
        {
            if (freezeTimer < freezeLockout)
            {
                GameManager.instance.webScreen.SetActive(true);
                FrozenOn = true;
            }
            else
            {
                GameManager.instance.webScreen.SetActive(false);
                FrozenOn = false;
                freezeDelaytimer = 0;
            }
        }


    }

    public void Blind()
    {
        StartCoroutine(BlindTime());
    }

    IEnumerator BlindTime()
    {
        GameManager.instance.blindScreen.SetActive(true);
        yield return new WaitForSeconds(blindDuration);
        GameManager.instance.blindScreen.SetActive(false);
    }
    void _Invincibility_()
    {
        if (InvincibilityTimer < InvincibilityDuration)
        {
            gameObject.layer = 11;
        }
        else
        {
            gameObject.layer = 3;
        }
    }

    public void invert()
    {
        StartCoroutine(InvertTime());
    }

    IEnumerator InvertTime()
    {
        invertMove = true;
        GameManager.instance.hypnoScreen.SetActive(true);
        yield return new WaitForSeconds(invertDuration);
        invertMove = false;
        GameManager.instance.hypnoScreen.SetActive(false);
    }
    IEnumerator playStep()
    {
        isPlayingSteps = true;
        aud.PlayOneShot(audSteps[Random.Range(0, audSteps.Length)], audStepsVol);

        yield return new WaitForSeconds(0.3f);

        isPlayingSteps = false;
    }

    public void updatePlayerUI()
    {
        GameManager.instance.playerHPBar.fillAmount = (float)HP / hpOrig;
        GameManager.instance.playerHPLabel.text = HP.ToString("F0");

        if (gunList.Count > 0)
        {
            GameManager.instance.ammoCurrent.text = gunList[gunListIdx].ammoCurrent.ToString("F0");
            GameManager.instance.ammoMax.text = gunList[gunListIdx].ammoMax.ToString("F0");
            GameManager.instance.ammoReserves.text = gunList[gunListIdx].ammoReserves.ToString("F0");
        }
    }
}




//Gold's pile of possible features
//add a new layer for dashing, this layer ignores enemy projectiles and would effectly make you invincible based on what it ignores
