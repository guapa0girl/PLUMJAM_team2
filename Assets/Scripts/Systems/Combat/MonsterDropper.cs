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
using Game.Systems.Combat;

public class MonsterDropper : MonoBehaviour
{
    Inventory inv;
    float dropMult;
    LootTable loot;   // �� Prefab���� �� �ְ� Init���� ����

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
