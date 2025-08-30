using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class EnemySpawner : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;           // 비어있으면 자동 탐색
    public GameObject enemyPrefab;     // 적 프리팹(필수)

    [Header("Spawn Settings")]
    public float spawnRadius = 8f;     // 플레이어로부터 거리
    public float spawnInterval = 1f;   // 생성 간격(초)
    public int spawnPerTick = 2;     // 한 번에 생성 수
    public int maxAlive = 30;        // 동시에 존재 가능한 최대 수
    public int maxTotal = -1;        // 총 스폰 가능 수(-1이면 무제한)

    [Header("Options")]
    public bool autoFindPlayer = true; // Player 자동 찾기
    public bool drawGizmo = true;

    float nextTime;
    int totalSpawned; // 지금까지 스폰된 총 수

    // 내가 직접 스폰한 적들만 추적
    readonly HashSet<SpawnedToken> alive = new HashSet<SpawnedToken>();

    void Awake()
    {
        // 플레이어 자동 참조
        if (!player && autoFindPlayer)
        {
            if (PlayerStat.instance) player = PlayerStat.instance.transform;
            if (!player)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p) player = p.transform;
            }
        }

        if (!enemyPrefab)
            Debug.LogWarning("[EnemySpawner] enemyPrefab이 비어있습니다.", this);
        if (!player)
            Debug.LogWarning("[EnemySpawner] player 참조가 없습니다.", this);
    }

    void OnEnable()
    {
        nextTime = 0f;
    }

    void Update()
    {
        if (!CanRun()) return;

        if (Time.time >= nextTime)
        {
            // 현재 살아있는 내가 스폰한 적 수
            int aliveCount = alive.Count;

            // 총 스폰량 제한 체크
            if (maxTotal >= 0 && totalSpawned >= maxTotal)
            {
                enabled = false; // 더 이상 스폰하지 않음
                return;
            }

            if (aliveCount < maxAlive)
            {
                int canSpawn = maxAlive - aliveCount;
                int toSpawn = Mathf.Min(spawnPerTick, canSpawn);

                // 총량 제한이 있다면 고려
                if (maxTotal >= 0)
                    toSpawn = Mathf.Min(toSpawn, maxTotal - totalSpawned);

                for (int i = 0; i < toSpawn; i++)
                    SpawnOne();
            }

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

        // 스폰 토큰 부착(없으면 자동 추가)
        var token = go.GetComponent<SpawnedToken>();
        if (!token) token = go.AddComponent<SpawnedToken>();
        token.owner = this;

        alive.Add(token);
    }

    // 적이 비활성/파괴될 때 토큰이 호출
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