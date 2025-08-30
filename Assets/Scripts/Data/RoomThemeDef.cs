using UnityEngine;

namespace Game.Data
{
    // RoomThemeDef.cs  (���� �� �׸� = ���� ����)
    [CreateAssetMenu(menuName = "Game/RoomTheme")]
    public class RoomThemeDef : ScriptableObject
    {
        public string roomId;
        public WeatherType weatherTag;   // �� ���� ���� �׸�
        public LootTable loot;           // ��� ���̺�
    }
}