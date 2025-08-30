using UnityEngine;

namespace Game.Data
{
    // RoomThemeDef.cs  (전투 방 테마 = 날씨 매핑)
    [CreateAssetMenu(menuName = "Game/RoomTheme")]
    public class RoomThemeDef : ScriptableObject
    {
        public string roomId;
        public WeatherType weatherTag;   // 이 방의 날씨 테마
        public LootTable loot;           // 드랍 테이블
    }
}