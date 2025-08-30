// Assets/Scripts/Systems/Combat/MonsterDropper.cs

// ��������������������������������������������������������������������������������������������������������������������������
// File: MonsterDropper.cs
// Layer : Systems (Game.Systems.Combat)
// Purpose : ���Ͱ� "�׾��� ��" ������ ����Ͽ� �κ��丮�� ����.
// Responsibilities :
//   - Health.Died �̺�Ʈ ����/����
//   - LootTable.Roll()�� (����, ����) ����
//   - dropMultiplier�� ���� �߰� ��÷(������ ��ŭ �ݺ�) ����
// Public API :
//   - void Init(Inventory inv, float multiplier = 1f)  �� ���� ���� �� CombatSystem�� ȣ��
// Serialized Fields :
//   - LootTable lootTable, float dropMultiplier
// Dependencies : LootTable, Inventory, Health
// Notes : lootTable �Ǵ� targetInventory�� ������ ������� ����(���� ����).
// ��������������������������������������������������������������������������������������������������������������������������

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
            if (seed && count > 0) targetInventory.AddSeed(seed, count);
        }
    }
}
