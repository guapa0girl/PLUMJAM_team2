using UnityEngine;

// LootTable.cs (가중치 드랍)
namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/LootTable")]
    public class LootTable : ScriptableObject
    {
        [System.Serializable]
        public class Entry
        {
            public SeedDef seed; public float weight = 1f;
            public int min; public int max;
        }
        public Entry[] entries;
        public (SeedDef, int) Roll()
        {
            if (entries == null || entries.Length == 0) return (null, 0);

            float sum = 0f;
            foreach (var e in entries) if (e != null && e.weight > 0f) sum += e.weight;
            if (sum <= 0f) return (null, 0);

            float r = Random.value * sum; // [0, sum)
            foreach (var e in entries)
            {
                if (e == null || e.weight <= 0f) continue;
                r -= e.weight;
                if (r <= 0f)
                {
                    int cnt = Mathf.Clamp(Random.Range(e.min, e.max + 1), 0, int.MaxValue);
                    return (e.seed, cnt);
                }
            }
            var last = entries[entries.Length - 1];
            int lastCnt = Mathf.Clamp(Random.Range(last.min, last.max + 1), 0, int.MaxValue);
            return (last.seed, lastCnt);
        }

    }
}