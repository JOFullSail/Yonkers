using UnityEngine;

public class RangedEnemy : EnemyAI
{
    [SerializeField] int weaponRange;

    [SerializeField] GameObject projectile;
    [SerializeField] Transform shootPos;
    [SerializeField] float delayBetweenShots;

    float shotTimer;

    bool playerInWeaponRange;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        shotTimer += Time.deltaTime;

        if (playerInWeaponRange)
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
