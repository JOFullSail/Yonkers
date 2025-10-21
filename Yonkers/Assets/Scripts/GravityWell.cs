using UnityEngine;
using System.Collections;

public class GravityWell : MonoBehaviour
{
    [SerializeField] float pullStrength = 4f;
    [SerializeField] Collider trigger;

    [SerializeField] bool canBeEscapedByDashing;
    [Tooltip("How long the player has after dashing before it starts pulling again")]
    [SerializeField] float dashGracePeriod = 1.5f;

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

            if (GameManager.instance.playerScript.IsDashing && canBeEscapedByDashing && trigger != null)
            {
                trigger.enabled = false;
                StartCoroutine(reenableTrigger());
            }
        }
    }

    IEnumerator reenableTrigger()
    {
        yield return new WaitForSeconds(dashGracePeriod);
        if(trigger != null)
            trigger.enabled = true;
    }
}
