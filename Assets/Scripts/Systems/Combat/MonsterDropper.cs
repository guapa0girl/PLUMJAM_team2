// Assets/Scripts/Systems/Combat/MonsterDropper.cs
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
