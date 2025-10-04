using UnityEngine;
using System.Collections;

public class damage : MonoBehaviour
{
    enum damageType
    {
        moving,
        stationary,
        DOT, // damage over time (poison)
        homing // following bullets
    }

    [SerializeField] damageType type;
    [SerializeField] Rigidbody rb; // In Unity, one of two objects needs a rigid body for collision to register.

    [SerializeField] int damageAmount;
    [SerializeField] float damageRate;
    [SerializeField] int speed;
    [SerializeField] int destroyTime; // Time that it takes to call out the bullet

    bool isDamaging;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (type == damageType.moving || type == damageType.homing)
        {
            Destroy(gameObject, destroyTime);

            if (type == damageType.moving)
            {
                rb.linearVelocity = transform.forward * speed;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (type == damageType.homing)
        {
            // Normalized vectors will always give you a magnitude from 0 to 1 for an easy check instead of checking large numbers.
            rb.linearVelocity = (gameManager.instance.player.transform.position - transform.position).normalized * speed * Time.deltaTime;
        }
    }

    // This function will execute once an object's collider enters the trigger zone
    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;


        IDamage dmg = other.GetComponent<IDamage>();
        if (dmg != null && (type == damageType.moving || type == damageType.homing || type == damageType.stationary))
        {
            dmg.takeDamage(damageAmount);
        }
        
        if (type == damageType.homing || type == damageType.moving)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.isTrigger) return;

        IDamage dmg = other.GetComponent<IDamage>();
        if (dmg != null && type == damageType.DOT)
        {
            if (!isDamaging) StartCoroutine(damageOther(dmg));
        }
    }

    IEnumerator damageOther(IDamage d)
    {
        isDamaging = true;
        d.takeDamage(damageAmount);
        yield return new WaitForSeconds(damageRate);
        isDamaging = false;
    }
}
