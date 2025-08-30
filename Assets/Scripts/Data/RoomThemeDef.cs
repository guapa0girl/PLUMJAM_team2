using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/RoomTheme")]
    public class RoomThemeDef : ScriptableObject
    {
        public string roomId;
        public WeatherType weatherTag;   // �� ���� ���� �׸�
        public LootTable loot;

        [System.Serializable]
        public class MonsterSet
        {
            public WeatherType weather;       // � ������ ��
            public GameObject[] monsterPrefabs; // �� �������� ������ ���͵�
        }

        [Header("Monster Sets")]
        public MonsterSet[] monsterSets;

        /// <summary>
        /// ���� ������ �ش��ϴ� ���� ��Ʈ�� ��ȯ
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
