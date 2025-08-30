// Assets/Scripts/Systems/Combat/CombatSystem.cs
// ─────────────────────────────────────────────────────────────
// File: CombatSystem.cs
// Layer : Systems (Game.Systems.Combat)
// Purpose : 방 시작→몬스터 스폰→타이머 진행→클리어 처리까지 전투의 상위 흐름을 관리.
// Responsibilities :
//   - 하루 당 클리어 횟수 제한(clearsPerDayLimit) 집행
//   - 방 입장(Spawner.BeginRoom) 및 몬스터 일괄 스폰(Spawner.SpawnAll)
//   - 몬스터에게 인벤토리/드랍 배수 전달(MonsterDropper.Init)
//   - 타이머(roomTimeLimitSec)와 생존 몬스터 상태로 클리어 판정
//   - 선택적으로 방 클리어 보상(Chest) 지급
// Public API :
//   - IEnumerator PlayOneClear(RoomThemeDef room, Inventory inv, float dropMultByWeather = 1f)
// Serialized Fields :
//   - RoomThemeDef[] rooms, float roomTimeLimitSec, int clearsPerDayLimit
//   - MonsterSpawner spawner, bool giveClearChest
// Dependencies : MonsterSpawner, MonsterDropper, Inventory, RoomThemeDef
// Notes : 드랍은 "몬스터 처치 시"가 메인이고, 클리어 보상은 옵션이다.
// ─────────────────────────────────────────────────────────────

using System.Collections;
using UnityEngine;
using Game.Core;     // Inventory
using Game.Data;     // RoomThemeDef

namespace Game.Systems.Combat
{
    public class CombatSystem : MonoBehaviour
    {
        [SerializeField] RoomThemeDef[] rooms;
        [SerializeField] float roomTimeLimitSec = 60f;
        [SerializeField] int clearsPerDayLimit = 1;
        int clearsToday = 0;

        [SerializeField] MonsterSpawner spawner;

        public IEnumerator PlayOneClear(RoomThemeDef room, Inventory inv, float dropMultByWeather = 1f)
        {
            if (clearsToday >= clearsPerDayLimit) yield break;

            spawner.BeginRoom(room);
            spawner.SpawnAll(room.weatherTag, inv, dropMultByWeather, mon => {
                var dropper = mon.GetComponent<MonsterDropper>();
                if (dropper) dropper.Init(inv, dropMultByWeather, null);
            });

            float t = 0f;
            while (t < roomTimeLimitSec && spawner.HasAliveMonsters) { t += Time.unscaledDeltaTime; yield return null; }

            spawner.EndRoom();
            
            clearsToday++;
        }
    }
}
