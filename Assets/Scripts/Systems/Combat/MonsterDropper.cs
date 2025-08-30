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
using Game.Systems.Combat;

public class MonsterDropper : MonoBehaviour
{
    Inventory inv;
    float dropMult;
    LootTable loot;   // ← Prefab에서 안 넣고 Init으로 주입

    public void Init(Inventory inv, float dropMult, LootTable loot)
    {
        this.inv = inv;
        this.dropMult = dropMult;
        this.loot = loot;
    }

    void OnDied(Health h)
    {
        if (loot == null || inv == null) return;
        var (seed, count) = loot.Roll();
        if (seed && count > 0)
            inv.AddSeed(seed, Mathf.RoundToInt(count * dropMult));
    }
}
