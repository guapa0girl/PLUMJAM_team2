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
            float sum = 0; foreach (var e in entries) sum += e.weight;
            float r = UnityEngine.Random.value * sum;  // [0..1] 포함. :contentReference[oaicite:6]{index=6}
            foreach (var e in entries) { if ((r -= e.weight) <= 0) return (e.seed, UnityEngine.Random.Range(e.min, e.max + 1)); }
            var last = entries[^1]; return (last.seed, UnityEngine.Random.Range(last.min, last.max + 1));
        }
    }
}