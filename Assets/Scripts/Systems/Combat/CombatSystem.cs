// Assets/Scripts/Systems/Combat/CombatSystem.cs
// ��������������������������������������������������������������������������������������������������������������������������
// File: CombatSystem.cs
// Layer : Systems (Game.Systems.Combat)
// Purpose : �� ���ۡ���� ������Ÿ�̸� �����Ŭ���� ó������ ������ ���� �帧�� ����.
// Responsibilities :
//   - �Ϸ� �� Ŭ���� Ƚ�� ����(clearsPerDayLimit) ����
//   - �� ����(Spawner.BeginRoom) �� ���� �ϰ� ����(Spawner.SpawnAll)
//   - ���Ϳ��� �κ��丮/��� ��� ����(MonsterDropper.Init)
//   - Ÿ�̸�(roomTimeLimitSec)�� ���� ���� ���·� Ŭ���� ����
//   - ���������� �� Ŭ���� ����(Chest) ����
// Public API :
//   - IEnumerator PlayOneClear(RoomThemeDef room, Inventory inv, float dropMultByWeather = 1f)
// Serialized Fields :
//   - RoomThemeDef[] rooms, float roomTimeLimitSec, int clearsPerDayLimit
//   - MonsterSpawner spawner, bool giveClearChest
// Dependencies : MonsterSpawner, MonsterDropper, Inventory, RoomThemeDef
// Notes : ����� "���� óġ ��"�� �����̰�, Ŭ���� ������ �ɼ��̴�.
// ��������������������������������������������������������������������������������������������������������������������������

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
