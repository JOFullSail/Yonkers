using UnityEngine;

public class RangedEnemy : EnemyAI
{
    [SerializeField] GameObject projectile;
    [SerializeField] Transform shootPos;
    [SerializeField] float delayBetweenShots;

    float shotTimer;

    bool playerInWeaponRange;

    // Update is called once per frame
    void Update()
    {
        shotTimer += Time.deltaTime;

        if (LookForPlayer())
        {
            if (shotTimer > delayBetweenShots)
                shoot();
        }
    }

    void shoot()
    {
        shotTimer = 0;

        Instantiate(projectile, shootPos.position, transform.rotation);
    }
}
