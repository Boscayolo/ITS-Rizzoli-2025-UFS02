using System.Collections;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    [SerializeField] protected Transform muzzle;
    [SerializeField] protected int bulletsLeft;

    //Dati armi, da spostare in scriptable object
    [SerializeField] protected float range;
    [SerializeField] protected int bulletDamage;
    [SerializeField] protected int clipSize;
    [SerializeField] protected float fireRate;
    [SerializeField] protected float reloadTime;

    protected bool readyToShoot;
    protected bool reloading;

    protected Coroutine reloadingCrt;

    protected virtual void Awake()
    {
        bulletsLeft = clipSize;
        readyToShoot = true;
    }

    protected virtual void TryShoot()
    {
        if (readyToShoot == false) return;
        Shoot();

        readyToShoot = false;
        float secondsBetweenShots = 1f / Mathf.Max(fireRate, 0.0001f);
        StartCoroutine(FireRateCd(secondsBetweenShots));
    }

    protected virtual void Shoot()
    {

    }

    protected virtual void Reload()
    {
        if (reloadingCrt != null) return;

        Debug.Log($"{gameObject.name} Reloading");

        reloading = true;
        reloadingCrt = StartCoroutine(ReloadWait(reloadTime));
        
    }

    protected IEnumerator ReloadWait(float _reloadTime)
    {
        yield return new WaitForSeconds(_reloadTime);
        bulletsLeft = clipSize;
        reloading = false;
        reloadingCrt = null;
    }

    protected IEnumerator FireRateCd(float _fireRate)
    {
        yield return new WaitForSeconds(_fireRate);
        readyToShoot = true;
    }

}
