using UnityEngine;

public class playerController : MonoBehaviour, IDamage
{
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    [SerializeField] int HP; // Amount of Health Points the player will start with.
    [SerializeField] int speed; // Normal speed of the player.
    [SerializeField] int jumpSpeed; // Speed/height of the jump.
    [SerializeField] int jumpMaxCount; // Total amount of jumps a player can make while on the air (this includes the initial jump).
    [SerializeField] float jumpGracePeriod; // Time (in seconds) allowed to do the initial jump after falling off a platform.
    [SerializeField] int gravity; // Player's gravitational pull.

    [SerializeField] int shootDmg; // Amount of HP the player gun can make.
    [SerializeField] int shootDist; // Player gun range.
    [SerializeField] float shootRate; // Time (in seconds) until allowed to shoot again.


    Vector3 moveDirec;
    Vector3 playerVel;

    int jumpCount;
    int hpOrig;

    float jumpTimer;
    float shootTimer;

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
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.blue);

        if (!controller.isGrounded) jumpTimer += Time.deltaTime;
        else jumpTimer = 0;
        shootTimer += Time.deltaTime;

        movement();
    }

    void movement()
    {
        if (controller.isGrounded)
        {
            playerVel.y = 0;
            jumpCount = 0;
        }
        else playerVel.y -= gravity * Time.deltaTime;

        moveDirec = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDirec * speed * Time.deltaTime);

        jump();
        controller.Move(playerVel * Time.deltaTime);

        if (!gameManager.instance.isPaused && Input.GetButton("Fire1") && shootTimer >= shootRate)
        {
            shoot();
        }
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

        // Only do this if you want to return information on what you did (pos, what it contains, the name)
        RaycastHit hit;

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
