using UnityEngine;
using System.Collections.Generic;

namespace Game.Data
{
    // ─────────────────────────────────────────────────────────────
    // File    : RoomThemeDef.cs
    // Purpose : 방 테마 + 날씨별 몬스터 세트/드랍 테이블 정의.
    // Notes   : 날씨별로 완전히 다른 MonsterDef[]를 연결할 수 있음.
    // ─────────────────────────────────────────────────────────────
    [CreateAssetMenu(menuName = "Game/RoomTheme")]
    public class RoomThemeDef : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("방 고유 ID (저장/식별용)")]
        public string roomId;

        [Header("Theme")]
        [Tooltip("이 방의 기본 날씨 테마 (표시/분류용)")]
        public WeatherType weatherTag;

        // 날씨별 몬스터 세트 정의
        [System.Serializable]
        public class MonsterSet
        {
            [Tooltip("이 세트가 적용될 날씨")]
            public WeatherType weather;

            [Tooltip("해당 날씨에서 스폰할 몬스터 종류들")]
            public MonsterDef[] monsters;
        }

        [Header("Monster Sets by Weather")]
        [Tooltip("날씨별로 완전히 다른 몬스터 목록을 정의")]
        public MonsterSet[] monsterSets;

        /// <summary>
        /// 오늘 날씨에 해당하는 MonsterDef 배열을 반환. 없으면 빈 배열.
        /// </summary>
        public MonsterDef[] GetMonstersForWeather(WeatherType today)
        {
            if (monsterSets == null) return System.Array.Empty<MonsterDef>();
            for (int i = 0; i < monsterSets.Length; i++)
            {
                var set = monsterSets[i];
                if (set != null && set.weather == today)
                    return set.monsters ?? System.Array.Empty<MonsterDef>();
            }
            return System.Array.Empty<MonsterDef>();
        }
    }
}