using UnityEditor;
using UnityEngine;


public class MeleeEnemy : EnemyAI
{
    [Header("Melee Attributes")]
    [SerializeField] Transform enemyPOS;// Enemy transfrom
    [SerializeField] float upwardforce; //
    [SerializeField] int meleeDamage;
    [SerializeField] float meleeDelay;
    [SerializeField] float dashDistance;
    [SerializeField] float dashSpeed;
    [SerializeField] float targetDistance;
    [SerializeField] float reach;

    //bool collide;
    //bool attackRange;
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
                punch(upwardforce, (GameManager.instance.player.transform.position - transform.position));

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
            GameManager.instance.playerScript.applyPushback(totalPunch);
            GameManager.instance.playerScript.takeDamage(meleeDamage);
            attackTimer = 0;
        }

    }

    void dashattack()
    {
        RaycastHit detected;
        Debug.DrawRay(transform.position, playerPosition, color: Color.blue);
        if (Physics.Raycast(transform.position, (playerPosition).normalized, out detected, targetDistance))
        {
            Debug.Log("Object detected");
            if (detected.collider.CompareTag("Player"))
            {
                dir = transform.forward;
                newPushPosition = new Vector3(GameManager.instance.player.transform.position.x, transform.position.y, GameManager.instance.player.transform.position.z);
                Debug.Log("Player Dectected");
            }

        }
    }
}
