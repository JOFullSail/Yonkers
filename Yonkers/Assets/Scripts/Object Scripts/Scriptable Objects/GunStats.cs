using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "GunStats", menuName = "Scriptable Objects/GunStats")]
public class GunStats : ScriptableObject
{
    public GameObject gunModel;

    [Header("Universal Stats")]
    [Range(0.1f, 3)] public float shootRate;
    public int ammoCurrent;
    [Range(1, 50)] public int ammoMax;
    [Range(0, 999)] public int ammoReserves;
    [Range(0, 999)] public int maxAmmoReserves;

    public float recoilDistance = 0.1f;
    public float recoilRecoverySpeed = 10f;
    public float recoilSharpness = 20f;

    [Tooltip("Special Weapons are dropped when ammunition runs out.")]
    public bool isSpecial;

    [Tooltip("Will not be used if weapon is set as hitscan.")]
    public GameObject projectile;

    [Header("Stats for Hitscan Weapons")]
    [Tooltip("If false, damage, range, and hit effect will be handled by the projectile itself.")]
    public bool isHitscan = true;
    [Range(1, 10)] public int hitscanShootDamage;
    [Range(15, 1000)] public int hitscanShootDist;
    public ParticleSystem hitEffect;

    [Header("Audio")]
    public AudioClip[] shootSound;
    [Range(0, 1)] public float shootSoundVol;
    public AudioClip[] reloadSound;
    [Range(0, 1)] public float reloadSoundVol;

    [Header("Transform Modifiers")]
    public Vector3 positionWhenHeld = new Vector3(0.2850304f, -0.2229996f, 0.4335518f);
    public Quaternion rotationWhenHeld = new Quaternion(-89.98f, 0f, 0f, 0f);
    public Vector3 scaleWhenHeld = Vector3.one;

    [Header("Reload Animation")]
    public float reloadMoveDistance = 0.2f;
    public float reloadRotateAngle = 25f;
    public float reloadSpeed = 8f;
}
