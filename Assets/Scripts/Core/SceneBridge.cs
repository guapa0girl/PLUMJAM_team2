using UnityEngine;

// ��������������������������������������������������������������������������������������������������������������������������
// File    : SceneBridge.cs
// Purpose : �� �� ������ ������ ����(���� ���� �� �̸�) + ��ȯ ����
// Notes   : ���� �ý��� �״�� �ΰ� "�� �Ѿ�� �κ�"�� ���� �ֱ��
// ��������������������������������������������������������������������������������������������������������������������������
using UnityEngine.SceneManagement;

namespace Game.Core
{
    public static class SceneBridge
    {
        public static string NextDungeonScene;   // ������ �� ���� �� �̸�
        public static float NextDropMult = 1f;   // ���� �ʿ�� ���

        public static void GoToUpgrade(string nextDungeonScene, float dropMult = 1f)
        {
            NextDungeonScene = nextDungeonScene;
            NextDropMult = dropMult;
            SceneManager.LoadScene("Upgrade");     // ���׷��̵� �� �̸�
        }

        public static void GoToDungeon()
        {
            var name = string.IsNullOrEmpty(NextDungeonScene) ? "dungeon_rain" : NextDungeonScene;
            SceneManager.LoadScene(name);
        }
    }
}
