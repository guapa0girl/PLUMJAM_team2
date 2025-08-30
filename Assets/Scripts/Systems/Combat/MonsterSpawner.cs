// Assets/Scripts/Systems/Combat/MonsterSpawner.cs
// ─────────────────────────────────────────────────────────────
// File: MonsterSpawner.cs
// Layer : Systems (Game.Systems.Combat)
// Purpose : 룸 시작/종료 수명주기와 몬스터 스폰/정리를 담당.
// Responsibilities :
//   - BeginRoom(room) : 룸 초기화(alive 목록 초기화, room.weatherTag 활용 가능)
//   - SpawnAll(onSpawned) : 프리팹들을 위치에 스폰하고 Health.Died를 구독
//   - EndRoom() : 남은 몬스터 정리, 구독 해제
//   - HasAliveMonsters 프로퍼티로 전투 진행 상태 제공
// Public API :
//   - void BeginRoom(RoomThemeDef room)
//   - void SpawnAll(Action<GameObject> onSpawned = null)
//   - void EndRoom()
// Serialized Fields :
//   - GameObject[] monsterPrefabs
// Internals :
//   - List<GameObject> alive  (현재 살아있는 몬스터 관리)
//   - OnMonsterDied 콜백에서 구독 해제 및 리스트에서 제거
// Notes : 위치 배치는 GetRandomPos()의 간단 랜덤. 프로젝트에 맞게 교체 가능.
// ─────────────────────────────────────────────────────────────

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
