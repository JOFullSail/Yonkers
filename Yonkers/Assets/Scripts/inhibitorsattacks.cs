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
                Debug.Log("Froze Player");
                StartCoroutine(frozeTime());
                //Zero player movement - What I want is to have access to the players movement and turing it to zero or stop him from moving for a small while.
            }

            if(Inhibitor == InhibitType.slowdown)
            {
                //divide playermovement by two - Take the player speed and divide it by two to slow him down.
            }

            if(Inhibitor == InhibitType.invert)
            {
                //invert player movement - invert when the character is moving.
            }

            if(Inhibitor == InhibitType.blind)
            {
                //Activate a panel for a few seconds to blind the player
            }
            else
            
                Debug.Log("Inhibitor not implemented:(");
            


        }

        Destroy(gameObject);
    }

    void Frozen()
    {
       
    }

    IEnumerator frozeTime()
    {
        GameManager.instance.playerScript.isInRagdoll = true;
        yield return new WaitForSeconds(InhibitDuration);
        GameManager.instance.playerScript.isInRagdoll = false;
    }
}
