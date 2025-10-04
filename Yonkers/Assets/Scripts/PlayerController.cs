using UnityEditor.Rendering;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] CharacterController controller;

    [SerializeField] float Speed0;
    [SerializeField] float MinSpeed;
    [SerializeField] float MaxSpeed;
    [SerializeField] float speedAccel;
    [SerializeField] float speedDeaccel;
    [SerializeField] float sprintMod;
    [SerializeField] int jumpSpeed;
    [SerializeField] int jumpCountMax;


    [SerializeField] int gravity;

    [SerializeField] int shootDamage;
    [SerializeField] int shootDist;
    [SerializeField] float shootRate;

    Vector3 moveDir;
    Vector3 MomentumDir;
    Vector3 CameraDir; //here in case you want to use camera as the way of controlling momentum.
    Vector3 playerVel;
    public GameObject player;
    public PlayerController playerScript;
    Vector3 camfor = Camera.main.transform.forward;
    int jumpCount;

    float shootTimer;
    float currentSpeed;
    bool isSprinting;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentSpeed = Speed0;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, camfor * shootDist, Color.yellow);
        Vector3 camdown = camfor;
        camdown.y /= 2;
        Debug.DrawRay(Camera.main.transform.position, camdown * shootDist, Color.blue);
        shootTimer += Time.deltaTime;
        CameraDir = camfor;
        movement();
        sprint();
    }

    void movement()
    {
        if (controller.isGrounded)
        {
            playerVel = Vector3.zero;
            jumpCount = 0;
        }
        else
        {
            //currentSpeed += speedAccel; slopes?
            playerVel.y -= gravity * Time.deltaTime;
        }
        //if (controller.isGrounded && player.DrawRay
        //{

        //}
        if ((Input.GetButton("Horizontal") == true || Input.GetButton("Vertical") == true) && currentSpeed < MaxSpeed)
        {
            moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
            MomentumDir = moveDir;
            currentSpeed += speedAccel * Time.deltaTime;
            controller.Move(moveDir * currentSpeed * Time.deltaTime);
        }
        else if ((Input.GetButton("Horizontal") == false && Input.GetButton("Vertical") == false) && currentSpeed > Speed0)
        {
            currentSpeed -= speedDeaccel * Time.deltaTime;
            controller.Move(MomentumDir * currentSpeed * Time.deltaTime);
        }
        //else if (currentSpeed >= MaxSpeed) // Uncomment this block of code to prevent the player from stopping immediately after reaching max speed.
        //{
        //    moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        //    MomentumDir = moveDir;
        //    currentSpeed = MaxSpeed;
        //    controller.Move(moveDir * currentSpeed * Time.deltaTime);
        //}

        jump();
        controller.Move(playerVel * Time.deltaTime);

        if (Input.GetButton("Fire1") && shootTimer >= shootRate)
            shoot();
    }

    void sprint()
    {
        if(Input.GetButtonDown("Shift"))
        {
            currentSpeed *= sprintMod;
        }
        else if(Input.GetButtonUp("Shift"))
        {
            currentSpeed /= sprintMod;
        }
    }

    void jump()
    {
        if(Input.GetButtonDown("Jump") && jumpCount < jumpCountMax)
        {
            playerVel.y = jumpSpeed;
            jumpCount++;
        }
    }

    void shoot()
    {
        shootTimer = 0;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist, ~ignoreLayer))
        {
            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if(dmg != null)
            {
                dmg.takeDamage(shootDamage);
            }

            Debug.Log(hit.collider.name);
        }
    }
}
