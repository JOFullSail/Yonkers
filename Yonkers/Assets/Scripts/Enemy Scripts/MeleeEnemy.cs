using UnityEditor;
using UnityEngine;


public class MeleeEnemy : EnemyAI
{
    [Header("Melee Attributes")]
    [SerializeField] Transform enemyPOS;// Enemy transfrom
    [SerializeField] float pushForce; //
    [SerializeField] int meleeDamage;
    [SerializeField] float meleeDelay;
    [SerializeField] float dashSpeed;
    [SerializeField] float targetDistance; //This is how far the enemy raycast will be to detect the player's last position
    [SerializeField] float reach;
    [SerializeField] bool diesonimpact;
    [SerializeField] AudioSource audEn;
    [SerializeField] AudioClip[] audPunch;
    [Range(0, 1)][SerializeField] float audPunchVol;

    //bool collide;
    //bool attackRange;
    bool Die;
    bool punched;
    float attackTimer;
    Vector3 playerPosition;
    Vector3 newPushPosition;
    Vector3 dir;

    void Update()
    {
        playerPosition = GameManager.instance.player.transform.position - transform.position;
        attackTimer += Time.deltaTime;



        enemyRoutine(); // roamRoutine()
        if (playerDetected && (firstTimeMet && initialAttackDelay <= 0.0f ||
           !firstTimeMet && attackDelayTimer <= 0.0f))
        {

            if (attackTimer > meleeDelay)
            {
                dashattack();
                transform.position = Vector3.Lerp(transform.position, newPushPosition, Time.deltaTime * dashSpeed);
                punch(pushForce, (GameManager.instance.player.transform.position - transform.position));

    
            }
            if(punched == true && diesonimpact == true)
            {
                Destroy(gameObject);
            }
        }
    }

    void punch(float Force, Vector3 dir)
    {

        dir = dir.normalized;
        Vector3 totalPunch = dir * Force;

        if (Vector3.Distance(GameManager.instance.player.transform.position, transform.position) <= reach)
        {

            animator.SetTrigger("Attack");
            Debug.Log("Ouch!!!");
            GameManager.instance.playerScript.Knockbacked = true;
            audEn.PlayOneShot(audPunch[Random.Range(0, audPunch.Length)], audPunchVol);
            GameManager.instance.playerScript.applyPushback(totalPunch);
            GameManager.instance.playerScript.takeDamage(meleeDamage);
            punched = true;
            attackTimer = 0;
        }
       

    }

    void dashattack()
    {
        RaycastHit detected;
        Debug.DrawRay(transform.position, playerPosition, color: Color.blue);
        if (Physics.Raycast(transform.position, (playerPosition).normalized, out detected, targetDistance))
        {
            if (detected.collider.CompareTag("Player"))
            {
                dir = transform.forward;
                newPushPosition = new Vector3(GameManager.instance.player.transform.position.x, transform.position.y, GameManager.instance.player.transform.position.z);
                Debug.Log("Player Dectected");
            }

        }
    }
}
    