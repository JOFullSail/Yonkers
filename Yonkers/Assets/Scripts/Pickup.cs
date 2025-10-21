using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField] GunStats gun;

    Vector3 rotation = new Vector3(0, 0, 100);
    private void OnTriggerEnter(Collider other)
    {
        IPickup pickup = other.GetComponent<IPickup>();

        if (pickup != null && gun != null)
        {
            gun.ammoCurrent = gun.ammoMax;
            pickup.GetGunStats(gun);
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        transform.Rotate(rotation * Time.deltaTime);
    }
}
