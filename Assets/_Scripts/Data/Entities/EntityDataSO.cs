using UnityEngine;

[CreateAssetMenu(menuName = "Entities/Entity Data", fileName = "Entity_")]
public class EntityDataSO : ScriptableObject
{
    public Entity baseEntity;
    public int maxEntityHealth;
    public int startingEntityHealth;
    public Color entityColor = new Color(1,1,1,1);
    public float size;
    public float speed;
    public float engageDistance;
    public WeaponData weapon;
}
