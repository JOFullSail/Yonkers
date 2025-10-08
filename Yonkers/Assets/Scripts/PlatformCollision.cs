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

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            other.transform.parent = null;
            other.transform.rotation = Quaternion.Euler(0f, other.transform.rotation.eulerAngles.y, 0f);
        }
    }
}
