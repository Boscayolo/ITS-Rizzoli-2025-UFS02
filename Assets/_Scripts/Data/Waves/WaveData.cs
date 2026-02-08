using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "Waves/WaveData")]
public class WaveData : ScriptableObject
{
    [SerializeField] public int enemiesInWave = 5;
    [SerializeField] public List<EntityDataSO> candidateEntities = new();

    public EntityDataSO GetRandomCandidate()
    {
        int entityIndex = Random.Range(0, candidateEntities.Count);

        return candidateEntities[entityIndex];
    }
}
