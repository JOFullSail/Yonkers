using UnityEngine;

public class playerController : MonoBehaviour, IDamage
{
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    [Header("General")] 
    [SerializeField] int HP;
    [SerializeField] int speed; // 12

    [Header("Jumping")]
    [SerializeField] int jumpSpeed; // 12
    [SerializeField] int jumpMaxCount; // 2
    [SerializeField] float jumpGracePeriod; // 0.175
    [SerializeField] int gravity; // 35

    [Header("Shooting")]
    [SerializeField] int shootDmg; // 1
    [SerializeField] int shootDist; // 15
    [SerializeField] float shootRate; // 0.5

    [Header("Climbing")]
    [SerializeField] float climbSpeed; // 
    [SerializeField] int climbingDist; // 
    [SerializeField] float climbWallDetection; // 
    [SerializeField] float climbMaxAngle; // 90


    Vector3 moveDirec;
    Vector3 playerVel;

    RaycastHit hit;

    int jumpCount;
    int hpOrig;

    float jumpTimer;
    float shootTimer;

    bool isClimbing;

    // - UNUSED -

    //[SerializeField] int sprintMod; // Speed multiplier.

    //bool isSprinting;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hpOrig = HP;
    }

    // Update is called once per frame
    void Update()
    {

        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * climbWallDetection, Color.blue);

        if (!controller.isGrounded) jumpTimer += Time.deltaTime;
        else jumpTimer = 0;
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

        moveDirec = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDirec * speed * Time.deltaTime);

        jump();
        controller.Move(playerVel * Time.deltaTime);

        if (!gameManager.instance.isPaused && Input.GetButton("Fire1") && shootTimer >= shootRate)
        {
            shoot();
        }
    }

    void climb()
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, climbWallDetection, ~ignoreLayer))
        {
            int wallAngle = (int)Vector3.Angle(hit.normal, Vector3.up);
            if (hit.collider.CompareTag("CanClimb") && controller.slopeLimit <= wallAngle && wallAngle <= climbMaxAngle && !controller.isGrounded && (playerVel.y < 0 || isClimbing))
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
            gameManager.instance.playerLose();
        }
    }
}
