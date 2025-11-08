using System;
using NUnit.Framework;
using UnityEngine;

enum PickupType
{
    Gun,
    Health,
    Ammo
}

public class Pickup : MonoBehaviour
{
    [SerializeField] PickupType type;

    [Header("For Gun and Ammo Pickups")]
    [SerializeField] GunStats gun;

    [Header("Just For Ammo Pickups")]
    [SerializeField] int ammoAmount;

    [Header("For Health Pickups")]
    [SerializeField] int healingAmount = 1;

    Vector3 rotation = new Vector3(0, 0, 100);

    private bool wasConsumed = false;
    private void OnTriggerEnter(Collider other)
    {
        IPickup pickup = other.GetComponent<IPickup>();

        if (pickup != null)
        {
            switch (type)
            {
                case PickupType.Gun:
                    if (gun != null)
                    {
                        gun.ammoCurrent = gun.ammoMax;
                        pickup.GetGunStats(gun);
                        wasConsumed = true;
                    }
                    break;

                case PickupType.Health:
                    if (GameManager.instance.playerScript.CurrentHealth < GameManager.instance.playerScript.OriginalHealth)
                    {
                        GameManager.instance.playerScript.takeDamage(-healingAmount);
                        wasConsumed = true;
                    }
                    break;

                case PickupType.Ammo:
                    int index = GameManager.instance.playerScript.GunList.IndexOf(gun); // returns -1 if item not found
                    if (index != -1 && gun.ammoReserves < gun.maxAmmoReserves)
                    {
                        gun.ammoReserves += ammoAmount;

                        wasConsumed = true;
                    }
                    break;
            }

            if (wasConsumed)
                Destroy(gameObject);
        }
    }
    private void Update()
    {
        transform.Rotate(rotation * Time.deltaTime);
    }
}
