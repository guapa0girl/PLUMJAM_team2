using UnityEngine;

// ��������������������������������������������������������������������������������������������������������������������������
// File: Inventory.cs  (����)
// Purpose : ���� �κ��丮 �ּ� ����. Add/Count�� ����.
// Defines : class Inventory : MonoBehaviour
// Notes   : ���� ���ӿ� �°� UI/���̺� ���� Ȯ��.
// ��������������������������������������������������������������������������������������������������������������������������
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
        // TODO: UI ���� �̺�Ʈ ���
    }
    public int Count(SeedDef def) => def != null && seeds.TryGetValue(def, out var n) ? n : 0;
}

