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

    [SerializeField] bool canJump;

    [SerializeField] bool canFly;

    [SerializeField] bool canDodge;

    [SerializeField] bool enragesWhenDamaged;

    [SerializeField] bool explodesOnDeath;

    Color colorOrig;

    protected bool playerInRange;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(flashRed());
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
        if (playerInRange)
            agent.SetDestination(GameManager.instance.player.transform.position);

        return playerInRange;
    }
}
