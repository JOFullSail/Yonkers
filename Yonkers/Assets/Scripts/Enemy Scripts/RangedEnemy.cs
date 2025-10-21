using UnityEngine;

public class RangedEnemy : EnemyAI
{
    [Header("Ranged Enemy Parameters")]
    [SerializeField] GameObject projectile;
    [SerializeField] Transform shootPos;
    [SerializeField] float delayBetweenShots;

    float shotTimer;

    bool playerInWeaponRange;

    // Update is called once per frame
    void Update()
    {
        shotTimer += Time.deltaTime;

        enemyRoutine(); // roamRoutine();
        if (playerDetected && (firstTimeMet && initialAttackDelay <= 0.0f ||
            !firstTimeMet && attackDelayTimer <= 0.0f))
        { 
            if (shotTimer > delayBetweenShots)
                shoot();
        }
    }

    void shoot()
    {
        animator.SetTrigger("Shoot");
        shotTimer = 0;

        Instantiate(projectile, shootPos.position, transform.rotation);
    }
}
