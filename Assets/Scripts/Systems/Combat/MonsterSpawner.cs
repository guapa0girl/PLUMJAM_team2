using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Data;
using Game.Core;

namespace Game.Systems.Combat
{
    public class MonsterSpawner : MonoBehaviour
    {
        readonly List<GameObject> alive = new();
        RoomThemeDef currentRoom;

        public bool HasAliveMonsters => alive.Count > 0;

        public void BeginRoom(RoomThemeDef room)
        {
            alive.Clear();
            currentRoom = room;
        }

        public void SpawnAll(WeatherType today, Inventory inv, float dropMult, System.Action<GameObject> onSpawned = null)
        {
            if (currentRoom == null) return;

            var monsterDefs = currentRoom.GetMonstersForWeather(today);
            foreach (MonsterDef def in monsterDefs)
            {
                var go = Instantiate(def.prefab, GetRandomPos(), Quaternion.identity);
                alive.Add(go);

                // Dropper √ ±‚»≠
                var dropper = go.GetComponent<MonsterDropper>();
                if (dropper) dropper.Init(inv, dropMult, def.loot);

                var h = go.GetComponent<Health>();
                if (h != null) h.Died += OnMonsterDied;

                onSpawned?.Invoke(go);
            }
        }

        public void EndRoom()
        {
            foreach (var go in alive) if (go) Destroy(go);
            alive.Clear();
            currentRoom = null;
        }

        void OnMonsterDied(Health h)
        {
            if (h != null) h.Died -= OnMonsterDied;
            if (h) alive.Remove(h.gameObject);
        }

        Vector3 GetRandomPos()
            => new(UnityEngine.Random.Range(-4f, 4f), 
                UnityEngine.Random.Range(-2f, 2f), 
                0f);
    }

}
