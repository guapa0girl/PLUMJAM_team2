// Assets/Scripts/Systems/Combat/MonsterDropper.cs

// ─────────────────────────────────────────────────────────────
// File: MonsterDropper.cs
// Layer : Systems (Game.Systems.Combat)
// Purpose : 몬스터가 "죽었을 때" 씨앗을 드랍하여 인벤토리에 지급.
// Responsibilities :
//   - Health.Died 이벤트 구독/해제
//   - LootTable.Roll()로 (씨앗, 수량) 산출
//   - dropMultiplier에 따라 추가 추첨(정수부 만큼 반복) 적용
// Public API :
//   - void Init(Inventory inv, float multiplier = 1f)  → 전투 시작 시 CombatSystem이 호출
// Serialized Fields :
//   - LootTable lootTable, float dropMultiplier
// Dependencies : LootTable, Inventory, Health
// Notes : lootTable 또는 targetInventory가 없으면 드랍하지 않음(안전 가드).
// ─────────────────────────────────────────────────────────────

using UnityEngine;
using Game.Data;        // LootTable, SeedDef
using Game.Core;       // Inventory

namespace Game.Systems.Combat
{
    public class MonsterDropper : MonoBehaviour
    {
        [SerializeField] LootTable lootTable;
        [SerializeField] float dropMultiplier = 1f;
        Inventory targetInventory;
        Health health;
        void Awake() { health = GetComponent<Health>(); if (health != null) health.Died += OnDied; }
        void OnDestroy() { if (health != null) health.Died -= OnDied; }
        public void Init(Inventory inv, float multiplier = 1f) { targetInventory = inv; dropMultiplier = multiplier; }
        void OnDied(Health _)
        {
            if (!lootTable || !targetInventory) return;
            var (seed, count) = lootTable.Roll();
            int extra = Mathf.FloorToInt(dropMultiplier - 1f);
            for (int i = 0; i < 1 + Mathf.Max(0, extra); i++)
            {
                var res = (i == 0) ? (seed, count) : lootTable.Roll();
                targetInventory.AddSeed(res.Item1, res.Item2);
            }
        }
    }
}
