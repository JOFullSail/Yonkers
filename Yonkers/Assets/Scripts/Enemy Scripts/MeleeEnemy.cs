using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class MeleeEnemy : EnemyAI
{
    [Header("Melee Attributes")]
    [SerializeField] Transform enemyPOS;
    [SerializeField] float pushForce;
    [SerializeField] int meleeDamage;
    [SerializeField] float meleeDelay;
    [SerializeField] float meleeRecharge;
    [SerializeField] float dashSpeed;
    [SerializeField] float targetDistance;
    [SerializeField] float reach;
    [SerializeField] bool isSuicider;
    [SerializeField] bool suicideOnTouch;
    [SerializeField] AudioSource audEn;
    [SerializeField] AudioClip[] audPunch;
    [Range(0, 1)][SerializeField] float audPunchVol;
    [SerializeField] TrailRenderer[] dashTrails;

    bool findingLocal = false;
    bool punched;
    float attackTimer;
    float chargeTimer;
    Vector3 playerPosition;
    Vector3 transformPosition;
    Vector3 newPushPosition;
    Vector3 dir;

    private void Awake()
    {
        foreach (TrailRenderer trail in dashTrails)
            trail.emitting = false;
        if (!isSuicider) suicideOnTouch = false;
    }

    void Update()
    {
        transformPosition = new Vector3(transform.position.x, transform.position.y + 2, transform.position.z);
        if (playerDetected)
            playerPosition = GameManager.instance.player.transform.position - transform.position;
        
        if (suicideOnTouch && Vector3.Distance(GameManager.instance.player.transform.position, transformPosition) <= reach)
            punch(pushForce, playerPosition);

        attackTimer += Time.deltaTime;

        if (!findingLocal)
            enemyRoutine();

        if (playerDetected && ((firstTimeMet && initialAttackDelay <= 0.0f) ||
            (!firstTimeMet && attackDelayTimer <= 0.0f)))
        {
            if (attackTimer > meleeDelay && !findingLocal)
            {
                dashattack();
                findingLocal = true;
            }
        }

        if (findingLocal)
        {
            if (chargeTimer < meleeRecharge)
            {
                flashBlue();
            }

            chargeTimer += Time.deltaTime;

            if (chargeTimer >= meleeRecharge)
            {
                originalcolor();

                transform.position = Vector3.Lerp(
                    transform.position,
                    newPushPosition,
                    Time.deltaTime * dashSpeed
                );

                punch(pushForce, playerPosition);
            }

            if (Vector3.Distance(transform.position, newPushPosition) <= 0.1f || punched)
            {
                foreach (TrailRenderer trail in dashTrails)
                    trail.emitting = false;

                punched = false;
                findingLocal = false;
                attackTimer = 0;
                chargeTimer = 0;
            }
        }
    }


    public void punch(float Force, Vector3 dir)
    {
        dir = dir.normalized;
        Vector3 totalPunch = dir * Force;

        if (Vector3.Distance(GameManager.instance.player.transform.position, transformPosition) <= reach)
        {
            animator.SetTrigger("Attack");
            GameManager.instance.playerScript.Knockbacked = true;

            audEn.PlayOneShot(audPunch[Random.Range(0, audPunch.Length)], audPunchVol);

            GameManager.instance.playerScript.applyPushback(totalPunch);
            GameManager.instance.playerScript.takeDamage(meleeDamage);

            punched = true;

            if (isSuicider)
                takeDamage(1000);
        }
    }

    void dashattack()
    {
        RaycastHit detected;
        //Debug.DrawRay(transform.position, playerPosition, Color.blue);

        if (Physics.Raycast(transform.position, playerPosition.normalized, out detected, targetDistance))
        {
            if (detected.collider.CompareTag("Player"))
            {
                foreach (TrailRenderer trail in dashTrails)
                    trail.emitting = true;

                dir = transform.forward;

                Vector3 desired = new Vector3(
                    GameManager.instance.player.transform.position.x,
                    transform.position.y,
                    GameManager.instance.player.transform.position.z);

                NavMeshHit hit;

                if (NavMesh.SamplePosition(desired, out hit, 2f, NavMesh.AllAreas))
                {
                    newPushPosition = hit.position;
                }
                else
                {
                    Vector3 midpoint = transform.position +
                        (desired - transform.position).normalized * 1.5f;

                    if (NavMesh.SamplePosition(midpoint, out hit, 2f, NavMesh.AllAreas))
                        newPushPosition = hit.position;
                    else
                        newPushPosition = transform.position;
                }

                // Slight inward nudge for edge safety
                Vector3 inward = newPushPosition - transform.position;
                inward.y = 0;
                newPushPosition -= inward.normalized * 0.25f;

                if (Vector3.Distance(newPushPosition, transform.position) < 0.1f)
                {
                    foreach (TrailRenderer trail in dashTrails)
                        trail.emitting = false;

                    findingLocal = false;
                    chargeTimer = 0;
                    attackTimer = 0;
                    return;
                }
            }
        }
    }
}

