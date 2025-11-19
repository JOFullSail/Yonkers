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

    [Tooltip("Respawns the player if they take damage.")]
    [SerializeField] bool respawnUponTouch;

    [SerializeField] bool delayRespawnUponTouch;
    [SerializeField] float delayTime;

    [Tooltip("Used only if damage is set as being explosive. Be aware that BOTH this and the regular damage will be applied.")]
    [SerializeField] int splashDamageAmount;

    [SerializeField] float damageRate;
    [SerializeField] int speed;
    [SerializeField] int destroyTime;

    [SerializeField] GameObject explosionPrefab;

    [SerializeField] bool isBossProjectile;

    bool isDamaging;

	// Activation Switches
	bool isPlayerProjectile;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (type == damageType.moving || type == damageType.homing)
        {
            Destroy(gameObject, destroyTime);

            if (type == damageType.moving && isBossProjectile)
            {
                rb.linearVelocity = (GameManager.instance.player.transform.position - transform.position).normalized * speed;
            }
            else if (type == damageType.moving)
                rb.linearVelocity = transform.forward * speed;
        }

		if (LayerMask.NameToLayer("Player Projectile") != -1) isPlayerProjectile = true;
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
        IActivate act = other.GetComponent<IActivate>();

        if (type == damageType.moving || type == damageType.stationary || type == damageType.homing)
        {
            if (isExplosive && explosionPrefab != null)
            {
                Vector3 projectileExplosionLoc = type != damageType.stationary ? new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z - 0.25f) : transform.position;
                GameObject explosion =  Instantiate(explosionPrefab, projectileExplosionLoc, Quaternion.identity);
                Explosion expl = explosion.GetComponent<Explosion>();
                
                if (expl != null)
                    expl.TriggerExplosion(transform.position, splashDamageAmount);
                    //Debug.LogWarning("Explosion prefab does not contain an Explosion component.");
            }
            else if(isExplosive && explosionPrefab == null)
            {
                //Debug.LogWarning(gameObject.name + " is set as being explosive but it doesn't have an explosive prefab assigned.");
            }   
            
            if (dmg != null)
            {
                dmg.takeDamage(damageAmount);

                if (respawnUponTouch && !delayRespawnUponTouch && GameManager.instance.playerScript.CurrentHealth > 0)
                {
                    GameManager.instance.RespawnFromCheckpoint(false);
                }
                else if (delayRespawnUponTouch)
                {
                    StartCoroutine(delayRespawn());
                }
            }

			if (isPlayerProjectile && act != null)
			{
				act.activate();
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

    IEnumerator delayRespawn()
    {
        //GameManager.instance.playerScript.controller.enabled = false;
        GameManager.instance.playerScript.PlayerVel = new Vector3(0, 0, 0);
        GameManager.instance.playerScript.gravityOffTimer = 0;
        GameManager.instance.playerScript.gravityLockout = delayTime;
        yield return new WaitForSeconds(delayTime);
        GameManager.instance.player.transform.parent = null;
        //GameManager.instance.playerScript.controller.enabled = false;
        GameManager.instance.RespawnFromCheckpoint(true);
    }
}
