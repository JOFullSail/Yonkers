using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject itemPrefab;      // health pickup, ammo pickup, etc.
    public float spawnInterval = 10f;  // seconds between spawns
    public int maxActive = 1;          // limit spawns in arena
    public bool dynamicSpawn = true;   // scale with player need?

    [Header("Dynamic Scaling")]
    public float minMultiplier = 0.5f;
    public float maxMultiplier = 2.0f;

    float timer;
    GameObject currentItem;

    void Update()
    {
        // If something is already spawned, do nothing
        if (currentItem != null)
            return;

        timer += Time.deltaTime;

        float interval = spawnInterval;

        if (dynamicSpawn)
            interval *= CalculateDynamicScale();

        if (timer >= interval)
        {
            SpawnItem();
            timer = 0f;
        }
    }

    float CalculateDynamicScale()
    {
        var player = GameManager.instance.playerScript;

        if (itemPrefab.CompareTag("HealthDrop") && player != null)
        {
            float hpPct = (float)player.CurrentHealth / player.OriginalHealth;
            float need = 1f - hpPct;
            return Mathf.Lerp(minMultiplier, maxMultiplier, need);
        }

        if (itemPrefab.CompareTag("AmmoLight") ||
            itemPrefab.CompareTag("AmmoMedium") ||
            itemPrefab.CompareTag("AmmoHeavy"))
        {
            float pct = GetAmmoPct();
            float need = 1f - pct;
            return Mathf.Lerp(minMultiplier, maxMultiplier, need);
        }

        return 1f;
    }

    float GetAmmoPct()
    {
        float cur = 0f;
        float max = 0f;

        if (GameManager.instance.playerScript != null)
            foreach (var gun in GameManager.instance.playerScript.GunList)
            {
                if (itemPrefab.CompareTag("AmmoLight") && gun.ammoType == GunStats.AmmoType.Light)
                {
                    cur += gun.ammoReserves;
                    max += gun.maxAmmoReserves;
                }

                if (itemPrefab.CompareTag("AmmoMedium") && gun.ammoType == GunStats.AmmoType.Medium)
                {
                    cur += gun.ammoReserves;
                    max += gun.maxAmmoReserves;
                }

                if (itemPrefab.CompareTag("AmmoHeavy") && gun.ammoType == GunStats.AmmoType.Heavy)
                {
                    cur += gun.ammoReserves;
                    max += gun.maxAmmoReserves;
                }
            }

        if (max == 0) return 1f;
        return cur / max;
    }

    void SpawnItem()
    {
        Vector3 pos = transform.position + Vector3.up * 0.5f;
        currentItem = Instantiate(itemPrefab, pos, Quaternion.identity);
    }
}
