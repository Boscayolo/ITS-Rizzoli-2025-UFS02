using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum PickUpSpawnType
{
    None = 0,
    HEAL = 1,
    WEAPON = 2,
    AMMO = 3
}

public class PickUpSpawner : MonoBehaviour
{
    [SerializeField] PickUp healPickUp;
    [SerializeField] PickUp weaponPickUp;
    [SerializeField] PickUp ammoPickUp;

    [SerializeField] PickUpSpawnType spawnType;
    [SerializeField] List<WeaponData> elegibleWeapons = new();

    [SerializeField] bool startsWithPickUp;
    [SerializeField] Transform container;
    [SerializeField] float cooldown;

    GameObject currentPickUp;

    private void Awake()
    {
        //if(startsWithPickUp)
        //{
        //    SpawnPickup();
        //    return;
        //}

        //StartCoroutine(WaitCooldown(cooldown));
    }

    void SpawnPickUp()
    {
        if(currentPickUp == null)
        {
            bool multiState = spawnType == PickUpSpawnType.HEAL | spawnType == PickUpSpawnType.WEAPON | spawnType == PickUpSpawnType.AMMO;
            //if (Enum.)
        }
    }

}
