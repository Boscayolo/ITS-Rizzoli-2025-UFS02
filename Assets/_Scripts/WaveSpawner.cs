using System;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] public List<WaveData> waves = new List<WaveData>();
    [SerializeField] float randomSpawnDistance = 20;
    int currentWave = 0;
    int currentWaveEnemies = 0;
    int currentKilledEnemies = 0;

    public Action OnWavesCompleted;

    private void Awake()
    {
        if(waves == null || waves.Count == 0 )
        {
            Debug.LogError("No waves are present in the spawner");
            return;
        }

        GenerateWave(waves[currentWave]);
    }

    private void Start()
    {
        GameManager.Instance?.RegisterSpawner(this);
    }

    void GenerateWave(WaveData wave)
    {
        currentWaveEnemies = wave.enemiesInWave;

        for(int i= 0; i < wave.enemiesInWave; i++)
        {
            float randomX = transform.position.x + UnityEngine.Random.Range(-randomSpawnDistance, randomSpawnDistance);
            float randomZ = transform.position.z + UnityEngine.Random.Range(-randomSpawnDistance, randomSpawnDistance);

            Vector3 randomSpawnPos = new Vector3(randomX,transform.position.y,randomZ);
            EntityDataSO randomCandidate = wave.GetRandomCandidate();
            Entity spawnedEntity = Instantiate(randomCandidate.baseEntity, randomSpawnPos, Quaternion.identity);

            spawnedEntity.InitializeEntity(randomCandidate);
            spawnedEntity.EntityHealth.OnDied += NotifyEntityDeath;
        }
    }

    void NotifyEntityDeath(Health entityHealth)
    {
        currentKilledEnemies++;
        entityHealth.OnDied -= NotifyEntityDeath;

        if(currentKilledEnemies == currentWaveEnemies) //wave has been wiped out
        {
            AdvanceWave();
            currentKilledEnemies = 0;
        }
    }

    void AdvanceWave()
    {
        if(currentWave < waves.Count - 1)
        {
            currentWave++;
            GenerateWave(waves[currentWave]);
        }
        else
        {
            Debug.Log("LEVEL COMPLETED");
            OnWavesCompleted?.Invoke();
        }
    }



}
