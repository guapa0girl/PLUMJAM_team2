using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class EnemySpawner : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;
    public GameObject enemyPrefab;

    [Header("Spawn Settings")]
    public float spawnRadius = 8f;
    public float spawnInterval = 1f;
    public int spawnPerTick = 2;
    public int maxAlive = 30;
    public int maxTotal = -1;        // -1이면 한 웨이브 한도 없음(무제한)

    [Header("Loop Options")]
    public bool resetWhenCleared = true; // 전멸 시 다음 웨이브 반복
    public float waveClearDelay = 1f;   // 전멸 확인 후 다음 웨이브까지 대기

    [Header("Options")]
    public bool autoFindPlayer = true;
    public bool drawGizmo = false;

    float nextTime;
    int totalSpawned;           // 이번 웨이브에서 스폰한 누적
    bool waitingForNextWave;
    float nextWaveAt;

    readonly HashSet<SpawnedToken> alive = new HashSet<SpawnedToken>();

    void Awake()
    {
        if (!player && autoFindPlayer)
        {
            if (PlayerStat.instance) player = PlayerStat.instance.transform;
            if (!player)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p) player = p.transform;
            }
        }
    }

    void OnEnable()
    {
        nextTime = 0f;
        waitingForNextWave = false;
        totalSpawned = 0;
    }

    void Update()
    {
        if (!CanRun()) return;

        // 전멸 후 다음 웨이브 대기
        if (waitingForNextWave)
        {
            if (alive.Count == 0 && Time.time >= nextWaveAt)
            {
                totalSpawned = 0;      // 웨이브 누적 리셋
                waitingForNextWave = false;
            }
            else return;
        }

        if (Time.time >= nextTime)
        {
            // 이번 웨이브 한도 도달 시: 전멸까지 대기
            if (maxTotal >= 0 && totalSpawned >= maxTotal)
            {
                if (resetWhenCleared)
                {
                    waitingForNextWave = true;
                    nextWaveAt = Time.time + waveClearDelay;
                }
                else
                {
                    enabled = false; // 더 이상 스폰하지 않음
                }
                nextTime = Time.time + spawnInterval;
                return;
            }

            int canSpawn = Mathf.Max(0, maxAlive - alive.Count);
            int toSpawn = Mathf.Min(spawnPerTick, canSpawn);

            // 웨이브 남은 수량 고려
            if (maxTotal >= 0)
                toSpawn = Mathf.Min(toSpawn, maxTotal - totalSpawned);

            for (int i = 0; i < toSpawn; i++) SpawnOne();

            nextTime = Time.time + Mathf.Max(0.01f, spawnInterval);
        }
    }

    bool CanRun()
    {
        if (!enabled || !gameObject.activeInHierarchy) return false;
        if (!enemyPrefab || !player) return false;
        return true;
    }

    void SpawnOne()
    {
        Vector2 dir = Random.insideUnitCircle.normalized;
        Vector3 pos = player.position + new Vector3(dir.x, dir.y, 0f) * spawnRadius;

        GameObject go = Instantiate(enemyPrefab, pos, Quaternion.identity);
        totalSpawned++;

        var token = go.GetComponent<SpawnedToken>();
        if (!token) token = go.AddComponent<SpawnedToken>();
        token.owner = this;

        alive.Add(token);
    }

    public void NotifyDespawn(SpawnedToken token)
    {
        if (token == null) return;
        alive.Remove(token);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!drawGizmo || !player) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(player.position, spawnRadius);
    }
#endif
}