using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Monster")]
    public class MonsterDef : ScriptableObject
    {
        public string monsterId;
        public string displayName;
        public GameObject prefab;      // ���� ��ȯ�� ������
        public int baseHp = 10;
        public int attack = 2;
        // �ʿ��ϸ� �̵��ӵ�, AI Ÿ�� � ���⿡
    }
}
