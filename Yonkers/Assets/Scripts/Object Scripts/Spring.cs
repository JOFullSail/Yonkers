using TMPro;
using UnityEngine;

public class Spring : MonoBehaviour
{
    [Range(0.2f, 100)][SerializeField] float SpringForce = 3;
    [SerializeField] GameObject emptySpringObject; //please use the empty object used to hold the spring and Teleport. It's is used for finding the Teleport object //only for non damaging springs
    [SerializeField] bool SpringGravityDisable; //if spring disables gravity
    [SerializeField] float disableGravityTime; //how long to disable Gravity
    [SerializeField] float springDelay = 0.5f; //how long to delay spring after first touch
    float springDelayTimer;
    Vector3 springDirection = Vector3.zero; 
    Vector3 minorTeleport = Vector3.zero;
    bool damagingSpring = false;
    bool checkforTeleportobject = true;
    

    private void Start()
    {
        damagingSpring = gameObject.GetComponent<Damage>();
    }

    private void Update()
    {//all in update in case you want to try adding springs to moving platforms.
        springDelayTimer += Time.deltaTime;
        if (springDelayTimer > springDelay)
        {
            if (!damagingSpring)
            {
                springDirection = transform.up.normalized;
                if (!checkforTeleportobject)
                {
                    //Debug.DrawRay(emptySpringObject.transform.position, springDirection * (((SpringForce * SpringForce) / 70) * 2), Color.red);
                }
                else if (emptySpringObject.transform.Find("TpLocation") != null)
                {
                    emptySpringObject = emptySpringObject.transform.Find("TpLocation").gameObject;
                    checkforTeleportobject = false; 
                }
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger || other.CompareTag("Player") == false)
                return;
        if (springDelayTimer > springDelay)
        {
            if (damagingSpring) //if a spike
            {
                Vector3 pointofcontact = gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
                Vector3 directionFromPlayer = pointofcontact - other.GetComponent<PlayerController>().transform.position;
                directionFromPlayer = -directionFromPlayer.normalized; //makes the dirction postive
                                                                       //messing with the .y actually changes the launch angle of the player.
                if ((directionFromPlayer.y <= 1 && directionFromPlayer.y > 0.6)) // on top of spike touch
                {
                    float multitude = (directionFromPlayer.y - 0.6f) + 1.6f / (directionFromPlayer.y - 0.6f); //used to make the x and z more consistent regardless of touch postion
                    directionFromPlayer *= multitude;
                    directionFromPlayer.y = 0.45f;
                }
                else if (directionFromPlayer.y <= 0.6 && directionFromPlayer.y >= -0.1)  //near ground touch
                {
                    directionFromPlayer.y = 0.45f;
                }
                else if (directionFromPlayer.y > -0.7 && directionFromPlayer.y < -0.1) //near below spike touch
                {
                    directionFromPlayer.y = -0.45f;
                }
                else if ((directionFromPlayer.y >= -1 && directionFromPlayer.y <= -0.7)) // below spike touch
                {
                    float multitude = ((-directionFromPlayer.y) - 0.7f) + 1.6f / ((-directionFromPlayer.y) - 0.7f); //used to make the x and z more consistent regardless of touch postion
                    directionFromPlayer *= multitude;
                    directionFromPlayer.y = -0.45f;
                }
                springDirection = directionFromPlayer;
                if (other.CompareTag("Player"))
                {
                    if (other.GetComponent<IPushback>() != null)
                    {
                        other.GetComponent<IPushback>().applyPushback(springDirection.normalized * SpringForce);
                    }
                    if (other.GetComponentInParent<PlayerController>() != null)
                    {
                        other.GetComponentInParent<PlayerController>().Knockbacked = true;
                        GameManager.instance.playerScript.IsInRagdoll = true;
                        float lockTime = Mathf.Max(GameManager.instance.playerScript.MinRagdollTime, springDirection.magnitude * GameManager.instance.playerScript.RagdollPerSpeed);
                        GameManager.instance.playerScript.RagdollTimeLeft = Mathf.Max(GameManager.instance.playerScript.RagdollTimeLeft, lockTime);
                    }
                }
                springDelayTimer = 0;
            }
            else //else normal spring logic
            {
                if (SpringGravityDisable)
                {
                    GameManager.instance.playerScript.gravityOffTimer = 0;
                    GameManager.instance.playerScript.gravityLockout = disableGravityTime;
                }
                springDirection = transform.up.normalized;
                minorTeleport = emptySpringObject.transform.position;
                if (other.CompareTag("Player"))
                {

                    other.enabled = false;
                    other.transform.position = minorTeleport;
                    //Debug.Log("Teleport");
                    other.enabled = true;
                    if (other.GetComponent<IPushback>() != null)
                    {
                        other.GetComponent<IPushback>().applyPushback(springDirection.normalized * SpringForce);
                    }
                    if (other.GetComponentInParent<PlayerController>() != null)
                    {
                        other.GetComponentInParent<PlayerController>().Knockbacked = true;
                    }
                }
                springDelayTimer = 0;
            }
        }
    }
}

