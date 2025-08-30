using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Data;

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

        public void SpawnAll(WeatherType today, Action<GameObject> onSpawned = null)
        {
            if (currentRoom == null) return;

            // 오늘 날씨에 맞는 몬스터 세트 가져오기
            var prefabs = currentRoom.GetMonstersForWeather(today);
            foreach (var p in prefabs)
            {
                var go = Instantiate(p, GetRandomPos(), Quaternion.identity);
                alive.Add(go);

                var h = go.GetComponent<Health>();
                if (h != null) h.Died += OnMonsterDied;

                onSpawned?.Invoke(go);
            }
        }

        public void EndRoom()
        {
            foreach (var go in alive)
                if (go) Destroy(go);
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
