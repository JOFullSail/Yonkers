using UnityEngine;
using System.Collections;
public class PlayerController : MonoBehaviour, IDamage
{
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    [Header("General")]
    [SerializeField] int HP; // 10 (Could be changed)

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
    
    [Header("Bools")]
    [SerializeField] bool is_Momentum; //Best true// how momentum works, true = use last input momentum false = use camera forward to as the direction of momentum
    [SerializeField] bool dash_is_camera; //Best false // true = dash at the camera, false = dash at input
    [SerializeField] bool dash_can_go_up; //Best false // if dash resets your jump vel, also based on if dash based on camera is true, this enables the use of camera.y accordingly
    [SerializeField] bool dashcarryover; //Best false //used to see if you want your dash speed to carry over into your current speed
    [SerializeField] bool can_gain_speed_while_climbing;//Best false // prevents accel while climbing

    //"INTERNAL FILEDS //Used in the background for various things

    //Vectors for player
    Vector3 moveDirec;
    Vector3 playerVel;
    Vector3 MomentumDir; //last input direction is used
    Vector3 CameraDir; //here in case you want to use camera as the way of controlling momentum.
    //RaycastHit
    RaycastHit hit;
    
    //Ints
    int jumpCount;
    int hpOrig;
    //Floats
    float jumpTimer;
    float shootTimer;
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
    bool isClimbing;
    bool is_dashing;
    // - UNUSED -
    //[SerializeField] Collider slopecheck; // no use yet
    // bool _onSlope; //use will be added later
    //Debug.DrawRay(GameManager.instance.player.transform.position, camdown * shootDist, Color.blue); //ray that looks at the floor
    //if (controller.isGrounded && _onSlope == true){}// possible slope code
    //bool onslope() //more possible slope code
    //{bool check = false;RaycastHit slopehit;if (Physics.Raycast(player.transform.position, camdown * shootDist, out slopehit, slopecheck.GetComponent<>))
    // {check = true;}return check;}   

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
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * climbWallDetection, Color.blue);

        if (!controller.isGrounded) jumpTimer += Time.deltaTime;
        else jumpTimer = 0;

        shootTimer += Time.deltaTime;
        Dashcdtimer += Time.deltaTime;

        Vector3 camwithouty = Camera.main.transform.forward;
        camwithouty.y = 0;
        CameraDir = camwithouty; //no y = no fly!

        climb();
        dash();
        DashMomentumEnd();
        movement();
        Gravity();
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
    void movement()
    {

        Movement();
        jump();
        controller.Move(playerVel * Time.deltaTime);
        if (Input.GetButton("Fire1") && shootTimer >= shootRate)
            shoot();
    }
    void Movement()
    {
        if (is_dashing == false)
        {
            if (can_gain_speed_while_climbing == false && isClimbing == false)
            {
                Movementincrementation();
            }
            else if(can_gain_speed_while_climbing == true && isClimbing == true)
            {
                Movementincrementation();
            }
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
                controller.Move(moveDirec * currentSpeed * Time.deltaTime);
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
                controller.Move(moveDirec * currentSpeed * Time.deltaTime);
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
        else if (is_dashing == false && !isClimbing)
        {

            playerVel.y -= gravity * Time.deltaTime;
        }
    }
    void dash() //dash in a direction but has bools for how you want to specifically dash.
    {

        if (Input.GetButton("Shift") && (Input.GetButton("Horizontal") == true || Input.GetButton("Vertical") == true) && isClimbing == false)
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
                }
                else
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


}
