using UnityEngine;

public class SimpleSpawner : MonoBehaviour
{
    [SerializeField] EntityDataSO entityToSpawn;

    private void Awake()
    {
        Entity nEntity = Instantiate(entityToSpawn.baseEntity.gameObject, transform.position, Quaternion.identity).GetComponent<Entity>();

        nEntity.InitializeEntity(entityToSpawn);
    }
}
