using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class EnemySpawner : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;           // ��������� �ڵ� Ž��
    public GameObject enemyPrefab;     // �� ������(�ʼ�)

    [Header("Spawn Settings")]
    public float spawnRadius = 8f;     // �÷��̾�κ��� �Ÿ�
    public float spawnInterval = 1f;   // ���� ����(��)
    public int spawnPerTick = 2;     // �� ���� ���� ��
    public int maxAlive = 30;        // ���ÿ� ���� ������ �ִ� ��
    public int maxTotal = -1;        // �� ���� ���� ��(-1�̸� ������)

    [Header("Options")]
    public bool autoFindPlayer = true; // Player �ڵ� ã��
    public bool drawGizmo = true;

    float nextTime;
    int totalSpawned; // ���ݱ��� ������ �� ��

    // ���� ���� ������ ���鸸 ����
    readonly HashSet<SpawnedToken> alive = new HashSet<SpawnedToken>();

    void Awake()
    {
        // �÷��̾� �ڵ� ����
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
            Debug.LogWarning("[EnemySpawner] enemyPrefab�� ����ֽ��ϴ�.", this);
        if (!player)
            Debug.LogWarning("[EnemySpawner] player ������ �����ϴ�.", this);
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
            // ���� ����ִ� ���� ������ �� ��
            int aliveCount = alive.Count;

            // �� ������ ���� üũ
            if (maxTotal >= 0 && totalSpawned >= maxTotal)
            {
                enabled = false; // �� �̻� �������� ����
                return;
            }

            if (aliveCount < maxAlive)
            {
                int canSpawn = maxAlive - aliveCount;
                int toSpawn = Mathf.Min(spawnPerTick, canSpawn);

                // �ѷ� ������ �ִٸ� ���
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

        // ���� ��ū ����(������ �ڵ� �߰�)
        var token = go.GetComponent<SpawnedToken>();
        if (!token) token = go.AddComponent<SpawnedToken>();
        token.owner = this;

        alive.Add(token);
    }

    // ���� ��Ȱ��/�ı��� �� ��ū�� ȣ��
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