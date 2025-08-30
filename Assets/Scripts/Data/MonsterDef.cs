using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Monster")]
    public class MonsterDef : ScriptableObject
    {
        public string monsterId;
        public string displayName;
        public GameObject prefab;      // 실제 소환할 프리팹
        public int baseHp = 10;
        public int attack = 2;
        // 필요하면 이동속도, AI 타입 등도 여기에
    }
}
