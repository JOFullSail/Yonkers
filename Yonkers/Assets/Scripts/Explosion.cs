using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] GameObject explosionEffect;
    [SerializeField] float explosionRadius = 5f;
    [SerializeField] float explosionForce = 10f;
    [SerializeField] float upwardsForceModifier = 2f;

    /// <summary>
    /// Trigger an explosion at a given location.
    /// Damage will be taken ONLY by objects with IDamage implemented.
    /// Explosive force will ONLY get applied to objects that have a Rigidbody component.
    /// </summary>
    /// <param name="explosionLocation"></param>
    public void TriggerExplosion(Vector3 explosionLocation, int splashDamage = 0)
    {
        Instantiate(explosionEffect, explosionLocation, Quaternion.identity);

        Collider[] colliders = Physics.OverlapSphere(explosionLocation, explosionRadius);

        foreach (Collider col in colliders)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 explosionDirection = col.transform.position - explosionLocation;
                float distance = explosionDirection.magnitude;
                float force = Mathf.Lerp(explosionForce, 0, distance / explosionRadius);
                rb.AddForce(explosionDirection.normalized * force + Vector3.up * upwardsForceModifier, ForceMode.Impulse);
            }

            IDamage dmg = col.GetComponent<IDamage>();
            if (dmg != null)
            {
                dmg.takeDamage(splashDamage);
            }
        }
    }
}
