using UnityEngine;
using System.Collections.Generic;

public class SeedInventory : MonoBehaviour
{
    [Header("Seed Types")]
    public SeedData[] seeds; // SeedData 배열로 씨앗 종류를 담을 수 있게 함

    private readonly Dictionary<SeedData, int> seedCount = new Dictionary<SeedData, int>();

    void Start()
    {
        foreach (var seed in seeds)
        {
            seedCount[seed] = 0; // 초기화
        }
    }

    public void Add(SeedData seed, int amount)
    {
        if (!seed || amount <= 0) return;

        if (!seedCount.ContainsKey(seed)) seedCount[seed] = 0;
        seedCount[seed] += amount;
    }

    public int GetSeedCount(SeedData seed)
    {
        return seedCount.ContainsKey(seed) ? seedCount[seed] : 0;
    }

    public Dictionary<SeedData, int> GetAllSeedCounts()
    {
        return seedCount;
    }
}