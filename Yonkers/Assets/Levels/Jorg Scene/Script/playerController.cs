using UnityEngine;

public class playerController : MonoBehaviour, IDamage
{
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    [SerializeField] int HP;
    [SerializeField] int speed;
    [SerializeField] int sprintMod;
    [SerializeField] int jumpSpeed;
    [SerializeField] int jumpCountMax;
    [SerializeField] int gravity;

    [SerializeField] int shootDmg;
    [SerializeField] int shootDist;
    [SerializeField] float shootRate;


    Vector3 moveDirec;
    Vector3 playerVel;

    int jumpCount;
    int hpOrig;

    float shootTimer;

    bool isSprinting;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hpOrig = HP;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.blue);

        shootTimer += Time.deltaTime;

        movement();

        sprint(); // will cause a bug (warning for the future lol)
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

        if (Input.GetButton("Fire1") && shootTimer >= shootRate)
        {
            shoot();
        }
    }

    void sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod;
        }
    }

    void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpCountMax)
        { 
            playerVel.y = jumpSpeed;
            ++jumpCount;
        }
    }

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
