using UnityEngine;

public class PlatformCollision : MonoBehaviour
{
    [SerializeField] string playerTag = "Player";
    [SerializeField] Transform platform;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            other.transform.parent = platform;
        }
    }

    [System.Obsolete]
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        bool onAnotherPlatform = false;

        PlatformCollision[] allPlatforms = FindObjectsOfType<PlatformCollision>();
        foreach (PlatformCollision otherPlatform in allPlatforms)
        {
            if (otherPlatform == this) continue;
            if (otherPlatform.GetComponent<Collider>() != null && otherPlatform.GetComponent<Collider>().bounds.Contains(other.transform.position))
            {
                onAnotherPlatform = true;
                break;
            }
        }

        if (!onAnotherPlatform && other.transform.parent == platform)
        {
            other.transform.parent = null;
            other.transform.localScale = Vector3.one;
            other.transform.rotation = Quaternion.Euler(0f, other.transform.rotation.eulerAngles.y, 0f);
        }
    }
}