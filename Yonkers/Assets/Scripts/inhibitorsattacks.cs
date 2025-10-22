using UnityEngine;
using System.Collections;

public class NewMonoBehaviourScript : MonoBehaviour
{
    enum InhibitType { freeze, slowdown, invert, blind}

    [SerializeField] InhibitType Inhibitor;
    [SerializeField] Rigidbody rb;
    [SerializeField] float speed;
    [SerializeField] int destroytime;
    [SerializeField] float delay;
    [SerializeField] float InhibitDuration;



    int inhibitTime;
    int inhitbitmax;
    void Start()
    {
        Destroy(gameObject, destroytime);
        rb.linearVelocity = transform.forward * speed;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {

            Debug.Log("Player Hit");

            if(Inhibitor == InhibitType.freeze)
            {
                GameManager.instance.playerScript.freezeTimer = 0;
                GameManager.instance.playerScript.freezeLockout = InhibitDuration;
                Debug.Log("Froze Player");

                //Zero player movement - What I want is to have access to the players movement and turing it to zero or stop him from moving for a small while.
            }

            if(Inhibitor == InhibitType.slowdown)
            {
                //divide playermovement by two - Take the player speed and divide it by two to slow him down.
            }

            if(Inhibitor == InhibitType.blind)
            {
               // GameManager.instance.playerScript.Blind();
                //Activate a panel for a few seconds to blind the player
            }
         
            


        }

        Destroy(gameObject);
    }


   
}
