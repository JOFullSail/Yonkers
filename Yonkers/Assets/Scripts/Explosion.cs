using UnityEngine;

public class Explosion : MonoBehaviour
{
    [Header("VFX")]
    [SerializeField] GameObject explosionEffect;

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
    /// </summary>
    public void TriggerExplosion(Vector3 explosionLocation, int splashDamage = 0)
    {
        if (explosionEffect) Instantiate(explosionEffect, explosionLocation, Quaternion.identity);

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

            // Knockback for Player
            PlayerKnockback playerKnock = col.GetComponentInParent<PlayerKnockback>();
            if (playerKnock != null)
            {
                playerKnock.BeginPhysicsKnockback(explosionLocation, explosionForce, explosionRadius, upwardsForceModifier);
                continue;
            }

            // Knockback for regular rigidbodies
            Rigidbody rb = col.attachedRigidbody;
            if (rb != null && !rb.isKinematic)
            {
                rb.AddExplosionForce(explosionForce, explosionLocation, explosionRadius, upwardsForceModifier, ForceMode.Impulse);
            }
        }
    }
}
