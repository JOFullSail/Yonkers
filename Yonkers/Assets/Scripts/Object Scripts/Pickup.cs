using System;
using NUnit.Framework;
using TMPro;
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

    public Vector3 rotation = new Vector3(0, 0, 100);

    private bool wasConsumed = false;
    private bool sameGunCheck = false;
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
                        sameGunCheck = false;

                        string pickupName = gun.name.Replace("(Clone)", "").Trim();

                        for (int indx = 0; indx < GameManager.instance.playerScript.GunList.Count; indx++)
                        {
                            string inventoryName = GameManager.instance.playerScript.GunList[indx]
                                .name.Replace("(Clone)", "").Trim();

                            if (pickupName == inventoryName)
                            {
                                var g = GameManager.instance.playerScript.GunList[indx];

                                g.ammoReserves = g.maxAmmoReserves;
                                g.ammoCurrent = g.ammoMax;

                                sameGunCheck = true;
                                break;
                            }
                        }

                        if (!sameGunCheck)
                        {
                            gun.ammoCurrent = gun.ammoMax;
                            pickup.GetGunStats(gun);
                        }

                        wasConsumed = true;
                        GameManager.instance.playerScript.updatePlayerUI();
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

                    GunStats.AmmoType ammoType = gun.ammoType;

                    bool gaveAmmo = false;

                    foreach (var g in GameManager.instance.playerScript.GunList)
                    {
                        if (g.ammoType == ammoType)
                        {
                            if (g.ammoReserves < g.maxAmmoReserves)
                            {
                                g.ammoReserves = Mathf.Min(
                                    g.ammoReserves + ammoAmount,
                                    g.maxAmmoReserves
                                );
                                gaveAmmo = true;
                            }
                        }
                    }

                    if (gaveAmmo)
                    {
                        wasConsumed = true;
                        GameManager.instance.playerScript.updatePlayerUI();
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
