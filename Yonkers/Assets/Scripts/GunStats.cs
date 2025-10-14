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
    [Range(0, 60)] public float recoilToCamera;
    [Range(0, 500)] public float recoilToUser;
    public bool isExplosive;

    public ParticleSystem hitEffect;
    public AudioClip[] shootSound;
    [Range(0, 1)] public float shootSoundVol;
}
