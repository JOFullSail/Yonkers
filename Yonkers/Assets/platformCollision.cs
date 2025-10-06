using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class platformCollision : MonoBehaviour
{
    [SerializeField] string playerTag = "Player";
    [SerializeField] Transform platform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals(playerTag))
        {
            other.gameObject.transform.parent = platform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals(playerTag))
        {
            other.gameObject.transform.parent = null;
        }
    }
}
