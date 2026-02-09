using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpSpawner : MonoBehaviour
{
    [Flags]
    public enum PickupFlags
    {
        None = 0,
        Heal = 1 << 0,
        Weapon = 1 << 1,
        Ammo = 1 << 2
    }

    [Header("Spawn Rules")]
    [SerializeField] private PickupFlags allowedPickups = PickupFlags.Heal;
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private float firstSpawnWaitTime = 2f;

    [SerializeField] private float respawnDelay = 5f;

    [Header("Spawn Point")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool parentSpawnedToSpawner = false;
    [SerializeField] private bool randomYRotation = false;

    [Header("Prefabs (assegna 1+ prefab per tipo)")]
    [SerializeField] private GameObject healPrefab;
    [SerializeField] private GameObject weaponPrefab;
    [SerializeField] private GameObject ammoPrefab;

    bool isFreeForRespawn = true;
    private Coroutine _spawnRoutine;

    private void Awake()
    {
        if (spawnPoint == null)
            spawnPoint = transform;
    }

    private void Start()
    {
        if (spawnOnStart) TrySpawnNow();
        else _spawnRoutine = StartCoroutine(SpawnAfter(firstSpawnWaitTime));
    }

    private void Update()
    {
        // Se il pickup è stato raccolto/distrutto, Unity lo considera "null"
        if (isFreeForRespawn && _spawnRoutine == null)
        {
            _spawnRoutine = StartCoroutine(SpawnAfter(respawnDelay));
        }
    }

    public bool HasActivePickup() => !isFreeForRespawn;

    public void TrySpawnNow()
    {
        if (!isFreeForRespawn) return;

        var candidates = BuildCandidateList();
        if (candidates.Count == 0)
        {
            Debug.LogWarning($"{name} PickUpSpawner: nessun prefab candidato. Controlla AllowedPickups e le liste prefab.");
            return;
        }

        var prefab = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        var pos = spawnPoint.position;

        Quaternion rot = spawnPoint.rotation;
        if (randomYRotation)
            rot = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);

        var parent = parentSpawnedToSpawner ? transform : null;
        GameObject nPickUp = Instantiate(prefab, pos, rot, parent);

        if(nPickUp.TryGetComponent(out PickUp pu))
        {
            pu.OnPickedUp += FreePickUpSpace;
        }
    }

    void FreePickUpSpace(PickUp freedPickUp)
    {
        isFreeForRespawn = true;
        freedPickUp.OnPickedUp -= FreePickUpSpace;
    }

    public void DestroyCurrentAndRespawn(float delayOverride = -1f)
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }

        float d = (delayOverride >= 0f) ? delayOverride : respawnDelay;
        _spawnRoutine = StartCoroutine(SpawnAfter(d));
    }

    private IEnumerator SpawnAfter(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        _spawnRoutine = null;
        TrySpawnNow();
    }

    private List<GameObject> BuildCandidateList()
    {
        var list = new List<GameObject>(16);

        if ((allowedPickups & PickupFlags.Heal) != 0)
            AddValidPrefab(healPrefab, list);

        if ((allowedPickups & PickupFlags.Weapon) != 0)
            AddValidPrefab(weaponPrefab, list);

        if ((allowedPickups & PickupFlags.Ammo) != 0)
            AddValidPrefab(ammoPrefab, list);

        return list;
    }

    private static void AddValidPrefab(GameObject source, List<GameObject> dest)
    {
        if (source == null) return;
        if (source != null) dest.Add(source);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Transform p = spawnPoint != null ? spawnPoint : transform;
        Gizmos.matrix = Matrix4x4.TRS(p.position, p.rotation, Vector3.one);
        Gizmos.DrawWireSphere(Vector3.zero, 0.35f);
        Gizmos.DrawLine(Vector3.zero, Vector3.forward * 0.75f);
    }
#endif
}
