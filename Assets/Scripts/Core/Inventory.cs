using UnityEngine;

// ─────────────────────────────────────────────────────────────
// File: Inventory.cs  (샘플)
// Purpose : 씨앗 인벤토리 최소 구현. Add/Count만 제공.
// Defines : class Inventory : MonoBehaviour
// Notes   : 실제 게임에 맞게 UI/세이브 연동 확장.
// ─────────────────────────────────────────────────────────────
using System.Collections.Generic;
using Game.Data;

public class Inventory : MonoBehaviour
{
    readonly Dictionary<SeedDef, int> seeds = new();

    public void AddSeed(SeedDef def, int count)
    {
        if (def == null || count <= 0) return;
        if (!seeds.ContainsKey(def)) seeds[def] = 0;
        seeds[def] += count;
        // TODO: UI 갱신 이벤트 쏘기
    }
    public int Count(SeedDef def) => def != null && seeds.TryGetValue(def, out var n) ? n : 0;
}

