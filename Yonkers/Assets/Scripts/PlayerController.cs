using UnityEngine;

public class PlayerController : MonoBehaviour, IDamage, IPushback
{
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    [Header("General")]
    [SerializeField] int HP; // 10 (Could be changed)
    [SerializeField] int speed; // 12

    [Header("Jumping")]
    [SerializeField] int jumpSpeed; // 12
    [SerializeField] int jumpMaxCount; // 2
    [SerializeField] float jumpGracePeriod; // 0.175
    [SerializeField] int gravity; // 35

    [Header("Shooting")]
    [SerializeField] int shootDmg; // 1 (Could be changed)
    [SerializeField] int shootDist; // 15 (Could be changed)
    [SerializeField] float shootRate; // 0.5 (Could be changed)

    [Header("Climbing")]
    [SerializeField] float climbSpeed; // 10.25
    [SerializeField] int climbingDist; // 0
    [SerializeField] float climbWallDetection; // 1.25
    [SerializeField] float climbMaxAngle; // 90

    [Header("Pushback")]
    [SerializeField] float pushDecay = 3f; // How fast push effects fade away
    [SerializeField] float ragdollPerSpeed = 0.06f;  // seconds of control lockout per 1 m/s moved during ragdoll
    [SerializeField] float minRagdollTime = 0.15f;

    Vector3 moveDirec;
    Vector3 playerVel;
    Vector3 pushBack;

    RaycastHit hit;

    int jumpCount;
    int hpOrig;

    float jumpTimer;
    float shootTimer;
    public float ragdollTimeLeft;
    public float ragPerSpeed;
    public float minRagTime;

    bool isClimbing;
    public bool isInRagdoll = false;

    // - UNUSED -

    //[SerializeField] int sprintMod; // Speed multiplier.

    //bool isSprinting;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ragPerSpeed = ragdollPerSpeed;
        minRagTime = minRagdollTime;
        hpOrig = HP;
    }

    // Update is called once per frame
    void Update()
    {

        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * climbWallDetection, Color.blue);

        if (!controller.isGrounded) 
            jumpTimer += Time.deltaTime; 
        else 
            jumpTimer = 0;

        if (controller.isGrounded && ragdollTimeLeft > 0f)
            ragdollTimeLeft = Mathf.Max(0f, ragdollTimeLeft - Time.deltaTime * 2f); // Recover twice as fast from ragdoll if you are on the ground
        else
            ragdollTimeLeft = Mathf.Max(0f, ragdollTimeLeft - Time.deltaTime);

        isInRagdoll = ragdollTimeLeft > 0f;

        shootTimer += Time.deltaTime;

        movement();
    }

    void movement()
    {
        climb();

        if (controller.isGrounded)
        {
            playerVel.y = 0;
            jumpCount = 0;
        }
        else if (!isClimbing)
        {
            playerVel.y -= gravity * Time.deltaTime;
        }

        if (!isInRagdoll)
        {
            moveDirec = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
            controller.Move(speed * Time.deltaTime * moveDirec);
        }
        else
            controller.Move((moveDirec + pushBack) * Time.deltaTime); // pushBack can cancel out or accelerate moveDirec here


        jump();

        controller.Move(pushBack * Time.deltaTime);
        pushBack = Vector3.Lerp(pushBack, Vector3.zero, pushDecay * Time.deltaTime);
        controller.Move(playerVel * Time.deltaTime);

        
        if (Input.GetButton("Fire1") && shootTimer >= shootRate)
        {
            shoot();
        }
    }

    public void applyPushback(Vector3 direction)
    {
        pushBack = direction;
    }
    void climb()
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, climbWallDetection, ~ignoreLayer))
        {
            int wallAngle = (int)Vector3.Angle(hit.normal, Vector3.up);
            if (!hit.collider.CompareTag("NoClimb") && controller.slopeLimit <= wallAngle && wallAngle <= climbMaxAngle && !controller.isGrounded && (playerVel.y < 0 || isClimbing))
            {
                playerVel.y = climbSpeed;
                isClimbing = true;
            }
        }
        else isClimbing = false;
    }
    void jump()
    {

        if (Input.GetButtonDown("Jump") && jumpCount < jumpMaxCount)
        {
            if (jumpTimer >= jumpGracePeriod && jumpCount == 0)
            {
                ++jumpCount;
            }

            playerVel.y = jumpSpeed;
            ++jumpCount;
        }
    }

    //void sprint()
    //{
    //    if (Input.GetButtonDown("Sprint"))
    //    {
    //        speed *= sprintMod;
    //    }
    //    else if (Input.GetButtonUp("Sprint"))
    //    {
    //        speed /= sprintMod;
    //    }
    //}

    void shoot()
    {
        shootTimer = 0;

        // ~ignoreLayer will ignore the player later to prevent the player shooting themselves.
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist, ~ignoreLayer))
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
            GameManager.instance.youDied();
        }
    }
}
