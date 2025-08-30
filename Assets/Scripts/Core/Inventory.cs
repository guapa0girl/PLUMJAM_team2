using UnityEngine;

// ��������������������������������������������������������������������������������������������������������������������������
// File: Inventory.cs  (����)
// Purpose : ���� �κ��丮 �ּ� ����. Add/Count�� ����.
// Defines : class Inventory : MonoBehaviour
// Notes   : ���� ���ӿ� �°� UI/���̺� ���� Ȯ��. UI ���� ���س���� !!!!!!!!!!!!!!!!!!!!!!!!!!
// ��������������������������������������������������������������������������������������������������������������������������
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
            // TODO: UI ���� �̺�Ʈ ���
        }
        // Inventory.cs (Game.Core)
        public bool TryConsume(SeedDef def, int count)
        {
            if (def == null || count <= 0) return false;
            int have = Count(def);
            if (have < count) return false;
            // ���� ��ųʸ� ���� ������ ���� ����
            // ����:
            System.Reflection.FieldInfo f = typeof(Inventory).GetField("seeds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var dict = (System.Collections.Generic.Dictionary<SeedDef, int>)f.GetValue(this);
            dict[def] = have - count;
            if (dict[def] <= 0) dict.Remove(def);
            // TODO: UI ���� �̺�Ʈ
            return true;
        }

        public int Count(SeedDef def) => def != null && seeds.TryGetValue(def, out var n) ? n : 0;
    }
}