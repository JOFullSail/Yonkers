using UnityEngine;

public class RangedEnemy : EnemyAI
{
    [Header("Ranged Enemy Parameters")]
    [SerializeField] GameObject projectile;
    [SerializeField] MeshRenderer projectileModel;
    [SerializeField] Transform shootPos;
    [SerializeField] float delayBetweenShots;
    [SerializeField] AudioSource shootSoundSource;
    [SerializeField] AudioClip[] audShoot;
    [Range(0, 1)][SerializeField] float audShootVol;
    float shotTimer;
    float projModelTimer;

    bool playerInWeaponRange;

    // Update is called once per frame
    void Update()
    {
        shotTimer += Time.deltaTime;
        projModelTimer = shotTimer + 0.5f;
        if (projModelTimer >= delayBetweenShots && projectileModel != null)
            projectileModel.enabled = true;
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
        projModelTimer = 0;
        shotTimer = 0;
        Instantiate(projectile, shootPos.position, transform.rotation);
        shootSoundSource.PlayOneShot(audShoot[Random.Range(0, audShoot.Length)], audShootVol);
        if(projectileModel != null)
            projectileModel.enabled = false;
    }
}
