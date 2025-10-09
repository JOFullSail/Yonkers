using UnityEngine;
using System.Collections;

public class Damage : MonoBehaviour
{
    enum damageType { moving, stationary, DOT, homing }
    [SerializeField] damageType type;
    [SerializeField] Rigidbody rb;

    [SerializeField] int damageAmount;

    [Tooltip("Checking this will do nothing if damage type is set to DOT")]
    [SerializeField] bool isExplosive;

    [Tooltip("Used only if damage is set as being explosive. Be aware that BOTH this and the regular damage will be applied.")]
    [SerializeField] int splashDamageAmount;

    [SerializeField] float damageRate;
    [SerializeField] int speed;
    [SerializeField] int destroyTime;

    [SerializeField] GameObject explosionPrefab;

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
            rb.linearVelocity = (GameManager.instance.player.transform.position - transform.position).normalized * speed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        IDamage dmg = other.GetComponent<IDamage>();

        if (type == damageType.moving || type == damageType.stationary || type == damageType.homing)
        {
            if (isExplosive && explosionPrefab != null)
            {
                Vector3 explosionPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z - 0.5f);
                GameObject explosion = Instantiate(explosionPrefab, explosionPosition, Quaternion.identity);
                Explosion expl = explosion.GetComponent<Explosion>();
                
                if (expl != null)
                    expl.TriggerExplosion(transform.position, splashDamageAmount);
                else
                    Debug.LogWarning("Explosion prefab does not contain an Explosion component.");
            }
            else
                Debug.LogWarning(gameObject.name + " is set as being explosive but it doesn't have an explosive prefab assigned.");

            if (dmg != null)
            {
                dmg.takeDamage(damageAmount);

            }
        }

        if (type == damageType.homing || type == damageType.moving || (type == damageType.stationary && isExplosive))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.isTrigger)
            return;

        IDamage dmg = other.GetComponent<IDamage>();

        if (dmg != null && type == damageType.DOT)
        {
            if (!isDamaging)
            {
                StartCoroutine(damageOther(dmg));
            }
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
