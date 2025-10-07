using UnityEditor.Rendering;
using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;

public class PlayerController : MonoBehaviour
{
    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] CharacterController controller;
    [SerializeField] float Speed0;
    [SerializeField] float MinSpeed;
    [SerializeField] float MaxSpeed;
    [SerializeField] float MaxDashSpeed;
    [SerializeField] float MinAccel;
    [SerializeField] float MinDeaccel;
    [SerializeField] float MaxAccel;
    [SerializeField] float MaxDeaccel;
    [SerializeField] float speedAccelRATE;
    [SerializeField] float speedDeaccelRATE; //how fast the Deceel ramps up
    [SerializeField] float sprintMod;
    [SerializeField] int jumpSpeed;
    [SerializeField] int jumpCountMax;
    [SerializeField] int gravity;
    [SerializeField] int shootDamage;
    [SerializeField] int shootDist;
    [SerializeField] float shootRate;
    [SerializeField] Collider slopecheck;
    [SerializeField] bool is_Momentum;
    [SerializeField] bool dash_is_camera;
    [SerializeField] bool dash_can_go_up;
    [SerializeField] float internaldashtimer; //how long dash last
    [SerializeField] float Dashcd;
    float currDashSpeed;
    float currspeedAccel;
    float currspeedDeaccel;
    float speedDeaccelOrig;
    float speedAccelOrig;
    float currentSpeed;
    Vector3 moveDir;
    Vector3 MomentumDir;
    Vector3 CameraDir; //here in case you want to use camera as the way of controlling momentum.
    Vector3 playerVel;
    Vector3 camfor = Camera.main.transform.forward;
    int jumpCount;
    float shootTimer;
    float Dashcdtimer;
    float Dashtimer; //used to track how long dash will go.
    bool _onSlope;
    Vector3 camdown = Vector3.down;
    bool is_dashing;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        currentSpeed = Speed0;
        currspeedAccel = MinAccel;
        currspeedDeaccel = MinDeaccel;
        speedDeaccelOrig = currspeedDeaccel;
        speedAccelOrig = currspeedAccel;
        Dashtimer = 1 / internaldashtimer;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.green);

        //Debug.DrawRay(GameManager.instance.player.transform.position, camdown * shootDist, Color.blue); //ray that looks at the floor
        shootTimer += Time.deltaTime;
        Dashcdtimer += Time.deltaTime;
        Vector3 camwithouty = Camera.main.transform.forward;
        camwithouty.y = 0;
        CameraDir  = camwithouty; //no y = no fly!
        dash();
        movement();
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
            if (!is_dashing)
            {
                playerVel.y -= gravity * Time.deltaTime;
            }
            
        }
        //if (controller.isGrounded && _onSlope == true)// possible slope code
        //{

        //}
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
            moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
            MomentumDir = moveDir;
            controller.Move(moveDir * currentSpeed * Time.deltaTime);
        }
        else if ((Input.GetButton("Horizontal") == true || Input.GetButton("Vertical") == true) && currentSpeed < MaxSpeed)
        {

            if (currentSpeed < MinSpeed)
            {
                currentSpeed = MinSpeed;
            }
            moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
            MomentumDir = moveDir;
            currentSpeed += currspeedAccel * Time.deltaTime;
            Ramp();
            controller.Move(moveDir * currentSpeed * Time.deltaTime);
        }
        else if ((Input.GetButton("Horizontal") == false && Input.GetButton("Vertical") == false) && currentSpeed > Speed0)
        {
            currentSpeed -= currspeedDeaccel * Time.deltaTime;
            Ramp();
            if (currentSpeed < 0)
            {
                currentSpeed = 0;
            }
            Movelike();
        }
        jump();
        controller.Move(playerVel * Time.deltaTime);
        if (Input.GetButton("Fire1") && shootTimer >= shootRate)
            shoot();
    }

    void dash() //dash in a direction but has bools for how you want to specifically dash.
    {
        is_dashing = false;
        if (Input.GetButton("Shift"))
        {
            if (Dashcdtimer >= Dashcd)
            {
                Dashcdtimer = 0;
                Dashtimer = 0;
                currDashSpeed = MaxDashSpeed;
                if (dash_can_go_up == true && dash_is_camera == true)
                {
                    StartCoroutine(dashwait(Camera.main.transform.forward * currDashSpeed * Time.deltaTime));
                    Debug.Log("CorouEnd");
                    if (currentSpeed < MaxSpeed)
                    {
                        Debug.Log("Speed if");
                        if (currentSpeed + currDashSpeed > MaxSpeed)
                        {
                            currentSpeed = MaxSpeed;
                        }
                        else
                        {
                            currentSpeed += currDashSpeed;
                        }
                    }
                    Debug.Log("Dash Speed Reset");
                    currDashSpeed = 0;
                }
                else if (dash_can_go_up == false && dash_is_camera == true)
                {
                    StartCoroutine(dashwait(CameraDir * currDashSpeed * Time.deltaTime));
                    Debug.Log("CorouEnd");
                    if (currentSpeed < MaxSpeed)
                    {
                        Debug.Log("Speed if");
                        if (currentSpeed + currDashSpeed > MaxSpeed)
                        {
                            currentSpeed = MaxSpeed;
                        }
                        else
                        {
                            currentSpeed += currDashSpeed;
                        }
                    }
                    Debug.Log("Dash Speed Reset");
                    currDashSpeed = 0;
                } else 
                {
                    StartCoroutine(dashwait(MomentumDir * currDashSpeed * Time.deltaTime));
                    Debug.Log("CorouEnd");
                    if (currentSpeed < MaxSpeed)
                    {
                        Debug.Log("Speed if");
                        if (currentSpeed + currDashSpeed > MaxSpeed)
                        {
                            currentSpeed = MaxSpeed;
                        }
                        else
                        {
                            currentSpeed += currDashSpeed;
                        }
                    }
                    Debug.Log("Dash Speed Reset");
                    currDashSpeed = 0;
                }                
            }
        }
    }
    IEnumerator dashwait(Vector3 move)
    {
        float i = 0;
        is_dashing = true;
        while ( i < internaldashtimer)
        {
            i += Time.deltaTime;
            Dashtimer += Time.deltaTime;
            controller.Move(move);
            Debug.Log("Still looping");
            yield return null;
        }
        i = 0;
        Debug.Log("DashEnd");
        StopCoroutine(dashwait(move));
    }
    void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpCountMax)
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
            if (dmg != null)
            {
                dmg.takeDamage(shootDamage);
            }

            Debug.Log(hit.collider.name);
        }
    }
    void Movelike()// checks if the player wants their momentum to be based on last input or to hard follow the camera
    {
        if (is_Momentum)
        {
            controller.Move(MomentumDir * currentSpeed * Time.deltaTime);
        }
        else
        {
            controller.Move(CameraDir * currentSpeed * Time.deltaTime);
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
            }else if (currspeedDeaccel < MaxDeaccel)
            {
                    currspeedDeaccel += speedDeaccelRATE * Time.deltaTime;

            }
        }
        else if ((Input.GetButton("Horizontal") == true || Input.GetButton("Vertical") == true) && currentSpeed > Speed0)
        {
            currspeedDeaccel = speedDeaccelOrig;
            if (currentSpeed >= MaxSpeed)
            {
                currspeedAccel = MaxSpeed;
            }
            else if (currspeedAccel < MaxAccel)
            {
                currspeedAccel += speedAccelRATE * Time.deltaTime;
            }
           
        }
    } 
    //bugs: accel goes to speed max upon speed reaching max
    

    //bool onslope()
    //{
    //    bool check = false;
    //    RaycastHit slopehit;
    //    if (Physics.Raycast(player.transform.position, camdown * shootDist, out slopehit, slopecheck.GetComponent<>))
    //    {
    //        check = true;
    //    }
    //    return check;
    //}
}
