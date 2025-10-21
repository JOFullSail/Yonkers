using UnityEngine;

[CreateAssetMenu(fileName = "GunStats", menuName = "Scriptable Objects/GunStats")]
public class GunStats : ScriptableObject
{
    public GameObject gunModel;

    [Range(1, 10)] public int shootDamage;
    [Range(15, 1000)] public int shootDist;
    [Range(0.1f, 3)] public float shootRate;
    public int ammoCurrent;
    [Range(1, 50)] public int ammoMax;
    [Range(0, 999)] public int ammoReserves;
    [Range(0, 60)] public float recoilToCamera;
    [Range(0, 500)] public float recoilToUser;

    public bool isSpecial;

    public bool isExplosive;
    public GameObject projectileExplosionPrefab;

    public bool isHitscan = true;
    public GameObject projectile;

    public ParticleSystem hitEffect;
    public AudioClip[] shootSound;
    [Range(0, 1)] public float shootSoundVol;

    public Vector3 positionWhenHeld = new Vector3(0.2850304f, -0.2229996f, 0.4335518f);
    public Vector3 rotationWhenHeld = new Vector3(-89.98f, 0f, 0f);
    public Vector3 scaleWhenHeld = Vector3.one;
}
