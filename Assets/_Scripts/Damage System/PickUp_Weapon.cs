using UnityEngine;

public class PickUp_Weapon : PickUp
{
    [SerializeField] WeaponData weapon;
    [SerializeField] Transform modelContainer;

    private void Awake()
    {
        Initialize();
    }

    void Initialize()
    {
        if (weapon == null) return;

        Transform model = Instantiate(weapon.weaponModel, modelContainer).transform;
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.Euler(-45, 0, 0);
    }

    public void InjectWeapon(WeaponData _weapon)
    {
        weapon = _weapon;
        Initialize();
    }

    protected override void Absorption(Collider other)
    {
        if(other.gameObject.TryGetComponent(out Shooter shooter))
        {
            shooter.EquipWeapon(weapon);
            gameObject.SetActive(false);

            base.Absorption(other);
        }
    }
}
