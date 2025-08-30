// Assets/Scripts/Systems/Combat/MonsterSpawner.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Systems.Combat
{
    public class MonsterSpawner : MonoBehaviour
    {
        [SerializeField] GameObject[] monsterPrefabs;
        readonly List<GameObject> alive = new();
        public bool HasAliveMonsters => alive.Count > 0;

        public void BeginRoom(Game.Data.RoomThemeDef room) { alive.Clear(); /* room.weatherTag 활용 가능 */ }
        public void SpawnAll(Action<GameObject> onSpawned = null)
        {
            foreach (var p in monsterPrefabs)
            {
                var go = Instantiate(p, GetRandomPos(), Quaternion.identity);
                alive.Add(go);
                var h = go.GetComponent<Health>();
                if (h != null) h.Died += OnMonsterDied;
                onSpawned?.Invoke(go);
            }
        }
        public void EndRoom() { foreach (var go in alive) if (go) Destroy(go); alive.Clear(); }
        void OnMonsterDied(Health h) { if (h != null) h.Died -= OnMonsterDied; if (h) alive.Remove(h.gameObject); }
        Vector3 GetRandomPos() => new(UnityEngine.Random.Range(-4f, 4f), UnityEngine.Random.Range(-2f, 2f), 0f);
    }
}
