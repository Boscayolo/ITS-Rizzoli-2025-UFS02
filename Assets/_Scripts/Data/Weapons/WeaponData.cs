using UnityEngine;

public enum BulletPattern
{
    Linear,
    Ballistic,
    Shotgun
}

public enum ShootingBehavior
{
    Auto,
    Semi_Auto
}

[CreateAssetMenu(menuName = "Weapons/Weapon Data", fileName = "WeaponData_")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponId = "weapon_id";
    public string displayName = "Weapon";

    [Header("Behavior")]
    public BulletPattern bulletPattern = BulletPattern.Linear;
    public ShootingBehavior shootingBehavior = ShootingBehavior.Auto;

    [Header("Core Stats")]
    [Min(0.1f)] public float range = 50f;
    [Min(1)] public int bulletDamage = 10;
    [Min(1)] public int clipSize = 12;
    [Min(0.1f)] public float fireRate = 5f;      // shots per second
    [Min(0.05f)] public float reloadTime = 1.2f; // seconds

    [Header("Projectile")]
    public GameObject bulletPrefab;
    [Min(0.1f)] public float bulletSpeed = 20f;

    [Header("Tracer (Visual)")]
    [Tooltip("Prefab con BulletTracer component per visualizzare i colpi hitscan")]
    public GameObject tracerPrefab;
    [Tooltip("Colore del tracer per quest'arma")]
    public Color tracerColor = Color.yellow;
    [Tooltip("Durata del tracer in secondi")]
    [Range(0.05f, 1f)] public float tracerLifetime = 0.2f;

    [Header("Weapon Model")]
    public GameObject weaponModel;

    [Header("Shotgun (only if Shotgun)")]
    [Min(1)] public int pellets = 8;
    [Range(0f, 25f)] public float spreadAngle = 6f;

    [Header("Ballistic (only if Ballistic)")]
    [Tooltip("Se true, prova a calcolare una traiettoria balistica verso l'aimPoint.")]
    public bool useBallisticAim = true;

    [Tooltip("Moltiplicatore del modulo Physics.gravity usato nel calcolo (solo per aim).")]
    [Min(0.1f)] public float gravityMultiplier = 1f;
}