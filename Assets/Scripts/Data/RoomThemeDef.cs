using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/RoomTheme")]
    public class RoomThemeDef : ScriptableObject
    {
        public string roomId;
        public WeatherType weatherTag;   // 이 방의 날씨 테마
        public LootTable loot;

        [System.Serializable]
        public class MonsterSet
        {
            public WeatherType weather;       // 어떤 날씨일 때
            public GameObject[] monsterPrefabs; // 그 날씨에서 스폰될 몬스터들
        }

        [Header("Monster Sets")]
        public MonsterSet[] monsterSets;

        /// <summary>
        /// 오늘 날씨에 해당하는 몬스터 세트를 반환
        /// </summary>
        public GameObject[] GetMonstersForWeather(WeatherType today)
        {
            foreach (var set in monsterSets)
                if (set.weather == today)
                    return set.monsterPrefabs;
            return new GameObject[0]; // fallback
        }
    }
}
