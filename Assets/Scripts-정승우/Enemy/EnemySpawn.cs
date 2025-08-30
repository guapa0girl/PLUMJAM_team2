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
    public int maxTotal = -1;        // -1�̸� �� ���̺� �ѵ� ����(������)

    [Header("Loop Options")]
    public bool resetWhenCleared = true; // ���� �� ���� ���̺� �ݺ�
    public float waveClearDelay = 1f;   // ���� Ȯ�� �� ���� ���̺���� ���

    [Header("Options")]
    public bool autoFindPlayer = true;
    public bool drawGizmo = false;

    float nextTime;
    int totalSpawned;           // �̹� ���̺꿡�� ������ ����
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

        // ���� �� ���� ���̺� ���
        if (waitingForNextWave)
        {
            if (alive.Count == 0 && Time.time >= nextWaveAt)
            {
                totalSpawned = 0;      // ���̺� ���� ����
                waitingForNextWave = false;
            }
            else return;
        }

        if (Time.time >= nextTime)
        {
            // �̹� ���̺� �ѵ� ���� ��: ������� ���
            if (maxTotal >= 0 && totalSpawned >= maxTotal)
            {
                if (resetWhenCleared)
                {
                    waitingForNextWave = true;
                    nextWaveAt = Time.time + waveClearDelay;
                }
                else
                {
                    enabled = false; // �� �̻� �������� ����
                }
                nextTime = Time.time + spawnInterval;
                return;
            }

            int canSpawn = Mathf.Max(0, maxAlive - alive.Count);
            int toSpawn = Mathf.Min(spawnPerTick, canSpawn);

            // ���̺� ���� ���� ���
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