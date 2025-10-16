using UnityEngine;


public class Spring : MonoBehaviour
{
    [Range(0.2f, 100)][SerializeField] float SpringForce = 3;
    [SerializeField] GameObject teleportlocation;
    public float errorcheck = 1.6f;
    Vector3 SpringDirection = Vector3.zero;
    Vector3 MinorTeleport = Vector3.zero;

    private void Start()
    {

    }

    private void Update()
    {
        //all in update in case you want to try adding springs to moving platforms.
        SpringDirection = transform.up.normalized;
        Debug.DrawRay(teleportlocation.transform.position, SpringDirection * (((SpringForce * SpringForce) / 70) * errorcheck), Color.red);
    }
    private void OnTriggerEnter(Collider other)
    {
        SpringDirection = transform.up.normalized;
        MinorTeleport = teleportlocation.transform.position;
        if (other.isTrigger)
            return;
        if (other.CompareTag("Player"))
        {
            MinorTeleport.y = other.GetComponent<CharacterController>().height + 0.5f;
            other.enabled = false;
            other.transform.position = MinorTeleport;
            Debug.Log("Teleport");
            other.enabled = true;
            if (other.GetComponent<IPushback>() != null)
            {
                other.GetComponent<IPushback>().applyPushback(SpringDirection.normalized * SpringForce);
            }
            if (other.GetComponentInParent<PlayerController>() != null)
            {
                other.GetComponentInParent<PlayerController>().knockbacked = true;
            }
        } 
    }
}

