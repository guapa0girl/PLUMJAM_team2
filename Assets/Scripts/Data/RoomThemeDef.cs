// ─────────────────────────────────────────────────────────────
// File    : RoomThemeDef.cs
// Namespace : Game.Data
// Purpose : 전투 방(던전 룸)의 테마 데이터를 정의하는 ScriptableObject.
//           - 방이 어떤 날씨 테마(WeatherType)에 속하는지 지정
//           - 해당 방에서 사용할 드랍 테이블(LootTable) 지정
// Defines : class RoomThemeDef : ScriptableObject
// Fields  : roomId      → 내부/저장용 방 ID (string)
//           weatherTag  → 이 방의 날씨 테마 (Heat, Rain, Snow, Cloud 중 하나)
//           loot        → 몬스터/보상 드랍 테이블 참조
// Used By : CombatSystem, MonsterSpawner, RunManager 등 전투 관련 로직
// Notes   : 에셋 생성 → Project 창에서 Create → Game → RoomTheme
//           룸마다 다른 날씨/드랍 구성을 쉽게 확장 가능
// ─────────────────────────────────────────────────────────────

using UnityEngine;

namespace Game.Data
{
    /// <summary>
    /// 전투 방(던전 룸)의 테마 정의.
    /// - 어떤 날씨 조건(WeatherType)에 해당하는 방인지 지정합니다.
    /// - 방 클리어 시 적용할 드랍 테이블을 연결합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/RoomTheme")]
    public class RoomThemeDef : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("방 고유 ID (저장/식별용)")]
        public string roomId;

        [Header("Theme")]
        [Tooltip("이 방의 날씨 테마 (Heat, Rain, Snow, Cloud 중 하나)")]
        public WeatherType weatherTag;

        [Header("Loot Table")]
        [Tooltip("방에서 드랍될 아이템/씨앗 테이블")]
        public LootTable loot;
    }
}
