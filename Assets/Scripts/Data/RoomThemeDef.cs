using UnityEngine;
using System.Collections.Generic;

namespace Game.Data
{
    // ��������������������������������������������������������������������������������������������������������������������������
    // File    : RoomThemeDef.cs
    // Purpose : �� �׸� + ������ ���� ��Ʈ/��� ���̺� ����.
    // Notes   : �������� ������ �ٸ� MonsterDef[]�� ������ �� ����.
    // ��������������������������������������������������������������������������������������������������������������������������
    [CreateAssetMenu(menuName = "Game/RoomTheme")]
    public class RoomThemeDef : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("�� ���� ID (����/�ĺ���)")]
        public string roomId;

        [Header("Theme")]
        [Tooltip("�� ���� �⺻ ���� �׸� (ǥ��/�з���)")]
        public WeatherType weatherTag;

        // ������ ���� ��Ʈ ����
        [System.Serializable]
        public class MonsterSet
        {
            [Tooltip("�� ��Ʈ�� ����� ����")]
            public WeatherType weather;

            [Tooltip("�ش� �������� ������ ���� ������")]
            public MonsterDef[] monsters;
        }

        [Header("Monster Sets by Weather")]
        [Tooltip("�������� ������ �ٸ� ���� ����� ����")]
        public MonsterSet[] monsterSets;

        /// <summary>
        /// ���� ������ �ش��ϴ� MonsterDef �迭�� ��ȯ. ������ �� �迭.
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