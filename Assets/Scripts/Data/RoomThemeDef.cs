// ��������������������������������������������������������������������������������������������������������������������������
// File    : RoomThemeDef.cs
// Namespace : Game.Data
// Purpose : ���� ��(���� ��)�� �׸� �����͸� �����ϴ� ScriptableObject.
//           - ���� � ���� �׸�(WeatherType)�� ���ϴ��� ����
//           - �ش� �濡�� ����� ��� ���̺�(LootTable) ����
// Defines : class RoomThemeDef : ScriptableObject
// Fields  : roomId      �� ����/����� �� ID (string)
//           weatherTag  �� �� ���� ���� �׸� (Heat, Rain, Snow, Cloud �� �ϳ�)
//           loot        �� ����/���� ��� ���̺� ����
// Used By : CombatSystem, MonsterSpawner, RunManager �� ���� ���� ����
// Notes   : ���� ���� �� Project â���� Create �� Game �� RoomTheme
//           �븶�� �ٸ� ����/��� ������ ���� Ȯ�� ����
// ��������������������������������������������������������������������������������������������������������������������������

using UnityEngine;

namespace Game.Data
{
    /// <summary>
    /// ���� ��(���� ��)�� �׸� ����.
    /// - � ���� ����(WeatherType)�� �ش��ϴ� ������ �����մϴ�.
    /// - �� Ŭ���� �� ������ ��� ���̺��� �����մϴ�.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/RoomTheme")]
    public class RoomThemeDef : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("�� ���� ID (����/�ĺ���)")]
        public string roomId;

        [Header("Theme")]
        [Tooltip("�� ���� ���� �׸� (Heat, Rain, Snow, Cloud �� �ϳ�)")]
        public WeatherType weatherTag;

        [Header("Loot Table")]
        [Tooltip("�濡�� ����� ������/���� ���̺�")]
        public LootTable loot;
    }
}
