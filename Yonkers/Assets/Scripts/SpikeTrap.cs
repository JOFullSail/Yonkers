using UnityEngine;

public class spikeTrap : MonoBehaviour
{
    [Header("Spike Settings")]
    [SerializeField] private Transform spike;
    [SerializeField] private Transform extendedPosition;
    [SerializeField] private Transform retractedPosition;

    [Header("Trap Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float resetDelay = 2f;
    [SerializeField] private int damageAmount = 20;
    [SerializeField] private string playerTag = "Player";

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag(playerTag))
        {
            hasTriggered = true;
            StartCoroutine(ActivateTrap(other));
        }
    }

    private System.Collections.IEnumerator ActivateTrap(Collider player)
    {
        while (Vector3.Distance(spike.position, extendedPosition.position) > 0.01f)
        {
            spike.position = Vector3.MoveTowards(spike.position, extendedPosition.position, speed * Time.deltaTime);
            yield return null;
        }

        // Damage the player
       
        yield return new WaitForSeconds(resetDelay);

        while (Vector3.Distance(spike.position, retractedPosition.position) > 0.01f)
        {
            spike.position = Vector3.MoveTowards(spike.position, retractedPosition.position, speed * Time.deltaTime);
            yield return null;
        }

        hasTriggered = false;
    }
}
