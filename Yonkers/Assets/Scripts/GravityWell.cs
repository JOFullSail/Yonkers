using UnityEngine;

public class GravityWell : MonoBehaviour
{
    [SerializeField] float pullStrength = 4f;
    [SerializeField] Collider trigger;
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Vector3 center = trigger.bounds.center;
            Vector3 playerPos = other.transform.position;

            Vector3 toCenter = (center - playerPos).normalized;

            Vector3 impulse = toCenter * pullStrength;

            GameManager.instance.playerScript.Knockbacked = true;
            GameManager.instance.playerScript.applyPushback(impulse);
        }
    }
}
