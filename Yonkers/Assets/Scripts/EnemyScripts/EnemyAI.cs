using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;

    [SerializeField] NavMeshAgent agent;

    [SerializeField] int HP;

    [SerializeField] float moveSpeed;

    [SerializeField] bool canMove;

    [SerializeField] bool canFly;

    [SerializeField] bool canDodge;

    [SerializeField] bool enragesWhenDamaged;

    [SerializeField] int enrageSpeedIncrease;

    [SerializeField] bool explodesOnDeath;
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] int explosionDamage;

    Color colorOrig;

    protected bool playerInRange;

    protected bool alreadyEnraged;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        alreadyEnraged = false;

        colorOrig = model.material.color;
    }

    // Update is called once per frame
    void Update()
    {
        LookForPlayer();
    }

    public void takeDamage(int amount)
    {
        HP -= amount;

        if(HP <= 0)
        {
            if (explodesOnDeath && explosionPrefab != null)
            {
                GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Explosion expl = explosion.GetComponent<Explosion>();
                if (expl != null)
                    expl.TriggerExplosion(transform.position, explosionDamage);
            }

            Destroy(gameObject);

        }
        else
        {
            StartCoroutine(flashRed());
        }

        if(enragesWhenDamaged && !alreadyEnraged)
        {
            moveSpeed += enrageSpeedIncrease;

            alreadyEnraged = true;
        }
            
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    protected bool LookForPlayer()
    {
        if (playerInRange && canMove)
            agent.SetDestination(GameManager.instance.player.transform.position);

        return playerInRange;
    }
}
