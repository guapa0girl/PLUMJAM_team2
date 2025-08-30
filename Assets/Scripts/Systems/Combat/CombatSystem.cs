// Assets/Scripts/Systems/Combat/CombatSystem.cs
using System.Collections;
using UnityEngine;
using Game.Core;     // Inventory
using Game.Data;     // RoomThemeDef

namespace Game.Systems.Combat
{
    public class CombatSystem : MonoBehaviour
    {
        [SerializeField] RoomThemeDef[] rooms;
        [SerializeField] float roomTimeLimitSec = 300f;
        [SerializeField] int clearsPerDayLimit = 2;
        int clearsToday = 0;

        [SerializeField] MonsterSpawner spawner;
        [SerializeField] bool giveClearChest = false;

        public IEnumerator PlayOneClear(RoomThemeDef room, Inventory inv, float dropMultByWeather = 1f)
        {
            if (clearsToday >= clearsPerDayLimit) yield break;

            spawner.BeginRoom(room);
            spawner.SpawnAll(mon => {
                var dropper = mon.GetComponent<MonsterDropper>();
                if (dropper) dropper.Init(inv, dropMultByWeather);
            });

            float t = 0f;
            while (t < roomTimeLimitSec && spawner.HasAliveMonsters) { t += Time.unscaledDeltaTime; yield return null; }

            spawner.EndRoom();

            if (giveClearChest && room.loot)
            {
                var (seed, count) = room.loot.Roll();
                inv.AddSeed(seed, count);
            }
            clearsToday++;
        }
    }
}
