using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour
{
    [Header("FX")]
    [SerializeField] ParticleSystem explosionEffect;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] explosionSound;
    [Range(0,1)] [SerializeField] float explosionVolume;

    [Header("Blast Settings")]
    [SerializeField] float explosionRadius = 5f;
    [SerializeField] float explosionForce = 10f;
    [SerializeField] float upwardsForceModifier = 0.5f;

    [Header("Collision/LOS")]
    [Tooltip("Which layers can be affected by the blast?")]
    [SerializeField] LayerMask overlapMask = ~0;

    [Tooltip("Can the explosion be blocked by obstacles?")]
    [SerializeField] bool blockByObstacles = false;

    [Tooltip("Which layers can block the explosion?")]
    [SerializeField] LayerMask obstacleMask = ~0;

    /// <summary>
    /// Trigger an explosion at a given location.
    /// Damage is applied to anything implementing IDamage.
    /// Pushback is applied to anything implementing IPushback.
    /// </summary>
    public void TriggerExplosion(Vector3 explosionLocation, int splashDamage = 0)
    {
        if (explosionEffect)
        {
            ParticleSystem.MainModule mainModule = explosionEffect.main;

            mainModule.stopAction = ParticleSystemStopAction.Destroy;
            Instantiate(explosionEffect, explosionLocation, Quaternion.identity);
        }

        if (audioSource)
        {
            int soundToPlay = Random.Range(0, explosionSound.Length);
            audioSource.PlayOneShot(explosionSound[soundToPlay], explosionVolume);
            StartCoroutine(cleanupExplosion(explosionSound[soundToPlay].length));
        }
        else
            StartCoroutine(cleanupExplosion(0.5f));

        Collider[] cols = Physics.OverlapSphere(explosionLocation, explosionRadius, overlapMask, QueryTriggerInteraction.Ignore);

        foreach (Collider col in cols)
        {
            // Take LOS into account if enabled
            if (blockByObstacles)
            {
                Vector3 dir = (col.bounds.center - explosionLocation);
                float dist = dir.magnitude;
                if (Physics.Raycast(explosionLocation, dir.normalized, dist, obstacleMask, QueryTriggerInteraction.Ignore))
                    continue;
            }

            // Damage
            IDamage dmg = col.GetComponentInParent<IDamage>();
            if (dmg != null && splashDamage != 0)
            {
                dmg.takeDamage(splashDamage);
            }

            // Pushback
            IPushback pb = col.GetComponentInParent<IPushback>();
            if (pb != null)
            {
                Vector3 contactPoint = Physics.ClosestPoint(explosionLocation, col, col.transform.position, col.transform.rotation);
                Vector3 direction = (contactPoint - explosionLocation);
                float distance = Mathf.Max(0.0001f, direction.magnitude);
                direction /= distance;

                Vector3 launchDir = Vector3.Normalize(direction + Vector3.up * upwardsForceModifier);

                Vector3 launch = launchDir * explosionForce;

                pb.applyPushback(launch);

                if (col.CompareTag("Player"))
                {
                    GameManager.instance.playerScript.IsInRagdoll = true;
                    GameManager.instance.playerScript.Knockbacked = true;

                    float lockTime = Mathf.Max(GameManager.instance.playerScript.MinRagdollTime, direction.magnitude * GameManager.instance.playerScript.RagdollPerSpeed);
                    GameManager.instance.playerScript.RagdollTimeLeft = Mathf.Max(GameManager.instance.playerScript.RagdollTimeLeft, lockTime);
                }
            }
        }
    }

    IEnumerator cleanupExplosion(float soundLength)
    {
        yield return new WaitForSeconds(soundLength);
        Destroy(gameObject);
    }
}
