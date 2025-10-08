using UnityEditor.Rendering;
using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;

public class PlayerController : MonoBehaviour
{
    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] CharacterController controller;
    [SerializeField] float DashSpeed;
    [SerializeField] float MinSpeed;
    [SerializeField] float MaxSpeed;
    [SerializeField] float MinAccel;
    [SerializeField] float MaxAccel;
    [SerializeField] float speedAccelRATE;//how fast the Aceel ramps up
    [SerializeField] float MinDeaccel;
    [SerializeField] float MaxDeaccel;
    [SerializeField] float speedDeaccelRATE; //how fast the Deaceel ramps up
    [SerializeField] int jumpSpeed;
    [SerializeField] int jumpCountMax;
    [SerializeField] int gravity;
    [SerializeField] int shootDamage;
    [SerializeField] int shootDist;
    [SerializeField] float shootRate;
    //[SerializeField] Collider slopecheck; // no use yet
    [SerializeField] bool is_Momentum; // how momentum works true = use last input momentum false = use camera as the momentum
    [SerializeField] bool dash_is_camera; // true = dash at the camera, false = dash at input
    [SerializeField] bool dash_can_go_up; // if dash resets your jump vel also based on if dash based on camera is true, this enables the use of camera.y accordingly
    [SerializeField] float DashLength; //how long dash last
    [SerializeField] float Dashcd;
    [SerializeField] bool dashcarryover; //used to see if you want your dash speed to carry over into your current speed
    bool is_dashing;  
    float currDashSpeed;
    float currspeedAccel;
    float currspeedDeaccel;
    float currentSpeed;
    float shootTimer;
    float Dashcdtimer;//used to track dash cd.
    float Dashtimer; //used to track how long dash will go.
    float speedDeaccelOrig;
    float speedAccelOrig;
    Vector3 moveDir;
    Vector3 MomentumDir;
    Vector3 CameraDir; //here in case you want to use camera as the way of controlling momentum.
    Vector3 playerVel; //used for jump 
    int jumpCount;
    float Speed0;
    bool _onSlope; //use will be added later
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.green);
        //Debug.DrawRay(GameManager.instance.player.transform.position, camdown * shootDist, Color.blue); //ray that looks at the floor
        shootTimer += Time.deltaTime;
        Dashcdtimer += Time.deltaTime;
        Vector3 camwithouty = Camera.main.transform.forward;
        camwithouty.y = 0;
        CameraDir  = camwithouty; //no y = no fly!
        dash();
        DashMomentumEnd();
        movement();
        Gravity();
    }

    void movement()
    {
        //if (controller.isGrounded && _onSlope == true)// possible slope code
        //{
        //}
        Movementdection();
        jump();
        controller.Move(playerVel * Time.deltaTime);
        if (Input.GetButton("Fire1") && shootTimer >= shootRate)
            shoot();
    }
    void Movementdection()
    {
        if (is_dashing == false)
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
    void DashMomentumEnd()// ends dash
    {
        if (Dashtimer >= DashLength)
        {
            if (dash_can_go_up == true && dash_is_camera == true)
            {
                StopCoroutine(dashwait(Camera.main.transform.forward * currDashSpeed * Time.deltaTime));
                DashMomentum();
            }
            else if (dash_can_go_up == false && dash_is_camera == true)
            {
                StopCoroutine(dashwait(CameraDir * currDashSpeed * Time.deltaTime));
                DashMomentum();
            }
            else
            {
                StopCoroutine(dashwait(MomentumDir * currDashSpeed * Time.deltaTime));
                DashMomentum();
            }

        }
    }
    void Gravity()// made gravity a method for easier use has checks for the bool dash_can_go_up
    {
        if (controller.isGrounded)
        {
            playerVel = Vector3.zero;
            jumpCount = 0;
        } 
        else if (is_dashing == true && dash_can_go_up == false)
        {
            playerVel.y = 0;
        } 
        else if (is_dashing == false)
        {
         
            playerVel.y -= gravity * Time.deltaTime;
        }


    }
    void dash() //dash in a direction but has bools for how you want to specifically dash.
    {
        
        if (Input.GetButton("Shift") && (Input.GetButton("Horizontal") == true || Input.GetButton("Vertical") == true))
        {
            is_dashing = true;
            if (Dashcdtimer >= Dashcd)
            {
                Dashcdtimer = 0;
                Dashtimer = 0;
                currDashSpeed = DashSpeed;
                if (dash_can_go_up == true && dash_is_camera == true)
                {
                    StartCoroutine(dashwait(Camera.main.transform.forward * currDashSpeed * Time.deltaTime)); 
                }
                else if (dash_can_go_up == false && dash_is_camera == true)
                {
                    StartCoroutine(dashwait(CameraDir * currDashSpeed * Time.deltaTime));
                } else 
                {
                    StartCoroutine(dashwait(MomentumDir * currDashSpeed * Time.deltaTime));
                }                
            }
        }
    }
    IEnumerator dashwait(Vector3 move) //used to make dash function
    {
        Dashtimer = 0;
        while (Dashtimer < DashLength)
        {
            Dashtimer += Time.deltaTime;
            Dashtimer += Time.deltaTime;
            controller.Move(move);

            yield return null;
        }
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
                currspeedAccel = MaxAccel;
            }
            else if (currspeedAccel < MaxAccel)
            {
                currspeedAccel += speedAccelRATE * Time.deltaTime;
            }
           
        }
    } 
    //bool onslope() //more possible slope code
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
