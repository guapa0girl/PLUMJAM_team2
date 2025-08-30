using UnityEngine;

[DisallowMultipleComponent]
public class SeedDropperTable : MonoBehaviour
{
    [Header("Pickup Prefab (필수)")]
    public GameObject pickupPrefab; // SeedPickup 프리팹

    [Header("Seed Types (4개 할당)")]
    public SeedData heatseed;
    public SeedData rainseed;
    public SeedData windseed;
    public SeedData snowseed;

    [Header("Type Weights (상대 확률, 합이 1일 필요 없음)")]
    public float heatWeight = 1f;
    public float rainWeight = 1f;
    public float windWeight = 1f;
    public float snowWeight = 1f;

    [Header("Count PMF (1~4개 확률) — 기본 평균 2개")]
    [Range(0f, 1f)] public float p1 = 0.35f;
    [Range(0f, 1f)] public float p2 = 0.40f;
    [Range(0f, 1f)] public float p3 = 0.15f;
    [Range(0f, 1f)] public float p4 = 0.10f;

    [Header("Options")]
    public bool alsoDropOnDisable = true;   // 풀링(SetActive(false)) 시에도 드랍
    public float scatterRadius = 0.25f;     // 바닥에 살짝 퍼뜨리기

    bool dropped;

    public void TryDrop()
    {
        if (dropped) return;
        dropped = true;
        if (!pickupPrefab) return;

        int count = SampleCount();
        for (int i = 0; i < count; i++)
        {
            var data = SampleType();
            if (!data) continue;
            SpawnPickup(data, 1);
        }
    }

    int SampleCount()
    {
        float sum = p1 + p2 + p3 + p4;
        if (sum <= 0f)
        {
            // PMF를 안 채웠으면 기본 2개
            return 2;
        }
        float r = Random.value * sum;
        if (r < p1) return 1; r -= p1;
        if (r < p2) return 2; r -= p2;
        if (r < p3) return 3;
        return 4;
    }

    SeedData SampleType()
    {
        // 유효한 씨앗만 모으기
        var s1 = heatseed; var s2 = rainseed; var s3 = windseed; var s4 = snowseed;
        float w1 = (s1 ? Mathf.Max(0f, heatWeight) : 0f);
        float w2 = (s2 ? Mathf.Max(0f, rainWeight) : 0f);
        float w3 = (s3 ? Mathf.Max(0f, windWeight) : 0f);
        float w4 = (s4 ? Mathf.Max(0f, snowWeight) : 0f);
        float sum = w1 + w2 + w3 + w4;

        if (sum <= 0f)
        {
            // 모든 가중치가 0이거나 미할당이면, 할당된 씨앗 중 임의 선택
            if (s1) return s1;
            if (s2) return s2;
            if (s3) return s3;
            if (s4) return s4;
            return null;
        }

        float r = Random.value * sum;
        if (r < w1) return s1; r -= w1;
        if (r < w2) return s2; r -= w2;
        if (r < w3) return s3;
        return s4;
    }

    void SpawnPickup(SeedData seed, int amount)
    {
        Vector3 pos = transform.position + (Vector3)Random.insideUnitCircle * scatterRadius;
        var go = Instantiate(pickupPrefab, pos, Quaternion.identity);
        var pick = go.GetComponent<SeedPickup>();
        if (pick)
        {
            pick.seed = seed;
            pick.amount = amount;
        }
    }

    void OnDestroy() { TryDrop(); }
    void OnDisable() { if (alsoDropOnDisable) TryDrop(); }
}