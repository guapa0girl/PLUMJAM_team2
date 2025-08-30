using UnityEngine;

// ─────────────────────────────────────────────────────────────
// File: Inventory.cs  (샘플)
// Purpose : 씨앗 인벤토리 최소 구현. Add/Count만 제공.
// Defines : class Inventory : MonoBehaviour
// Notes   : 실제 게임에 맞게 UI/세이브 연동 확장. UI 연동 안해놨어요 !!!!!!!!!!!!!!!!!!!!!!!!!!
// ─────────────────────────────────────────────────────────────
using System.Collections.Generic;
using Game.Data;
namespace Game.Core
{
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
        // Inventory.cs (Game.Core)
        public bool TryConsume(SeedDef def, int count)
        {
            if (def == null || count <= 0) return false;
            int have = Count(def);
            if (have < count) return false;
            // 내부 딕셔너리 접근 로직에 맞춰 감소
            // 예시:
            System.Reflection.FieldInfo f = typeof(Inventory).GetField("seeds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var dict = (System.Collections.Generic.Dictionary<SeedDef, int>)f.GetValue(this);
            dict[def] = have - count;
            if (dict[def] <= 0) dict.Remove(def);
            // TODO: UI 갱신 이벤트
            return true;
        }

        public int Count(SeedDef def) => def != null && seeds.TryGetValue(def, out var n) ? n : 0;
    }
}